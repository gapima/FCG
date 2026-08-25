using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using FIAP.CloudGames.Infrastructure.Repositories.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.Infrastructure.IoC;

/// <summary>
/// Centraliza o registro do acesso a dados e dos adaptadores de infraestrutura.
/// </summary>
public static class InfrastructureDependency
{
    public const string NomeConnectionString = "PostgreSql";

    // Espelha os valores padrão definidos em docker-compose.yml (POSTGRES_USER/POSTGRES_PASSWORD/POSTGRES_DB/porta).
    private const string ConexaoPostgreSqlPadrao =
        "Host=localhost;Port=5432;Database=fiap_cloud_games;Username=postgres;Password=@Testesenha123456";

    public static IServiceCollection RegistrarInfrastructureDependency(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        ArgumentNullException.ThrowIfNull(servicos);
        ArgumentNullException.ThrowIfNull(configuracao);

        var connectionString = configuracao.GetConnectionString(NomeConnectionString)
            ?? ConexaoPostgreSqlPadrao;

        servicos.AddDbContext<PostgresqlDbContext>(opcoes =>
            opcoes.UseNpgsql(
                connectionString,
                opcoesPostgreSql =>
                {
                    opcoesPostgreSql.MigrationsAssembly(
                        typeof(PostgresqlDbContext).Assembly.FullName);
                    opcoesPostgreSql.MigrationsHistoryTable("__ef_migrations_history");
                    opcoesPostgreSql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));

        servicos.AddScoped<IRepositorioJogos, RepositorioJogos>();

        return servicos;
    }
}
