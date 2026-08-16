using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using FIAP.CloudGames.Infrastructure.Repositories.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.Infrastructure.IoC;

/// <summary>
/// Centraliza o registro do acesso a dados e dos adaptadores de infraestrutura.
/// </summary>
public static class InfrastructureDependency
{
    public const string NomeConnectionString = "Host=localhost;Port=5432;Database=fiap_cloud_games;Username=postgres;Password=@Testesenha123456";

    public static IServiceCollection RegistrarInfrastructureDependency(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        ArgumentNullException.ThrowIfNull(servicos);
        ArgumentNullException.ThrowIfNull(configuracao);

        //var connectionString = configuracao.GetConnectionString(NomeConnectionString)
        //    ?? throw new InvalidOperationException(
        //        $"ConnectionStrings:{NomeConnectionString} deve ser configurada por User Secrets "
        //        + "ou por uma variável de ambiente.");

        servicos.AddDbContext<PostgresqlDbContext>(opcoes =>
            opcoes.UseNpgsql(
                NomeConnectionString,
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

        servicos.AddScoped<IRepositoryUsuarios, RepositorioUsuarios>();

        return servicos;
    }
}
