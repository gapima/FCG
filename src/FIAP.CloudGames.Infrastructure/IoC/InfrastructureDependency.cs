using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Application.Abstractions.Security;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using FIAP.CloudGames.Infrastructure.Repositories.Identity;
using FIAP.CloudGames.Infrastructure.Security;
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

    public static IServiceCollection RegistrarInfrastructureDependency(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        ArgumentNullException.ThrowIfNull(servicos);
        ArgumentNullException.ThrowIfNull(configuracao);

        var connectionString = configuracao.GetConnectionString(NomeConnectionString)
            ?? throw new InvalidOperationException(
                $"ConnectionStrings:{NomeConnectionString} deve ser configurada por User Secrets "
                + "ou por uma variável de ambiente.");
        var configuracaoJwt = ConfiguracaoJwt.Criar(configuracao);

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

        servicos.AddScoped<IRepositoryUsuarios, RepositorioUsuarios>();
        servicos.AddScoped<IRepositorioTokens, RepositorioTokens>();
        servicos.AddSingleton(configuracaoJwt);
        servicos.AddSingleton<IServicoHashSenha, ServicoHashSenha>();
        servicos.AddSingleton<IServicoTokenJwt, ServicoTokenJwt>();
        servicos.AddSingleton<IServicoRefreshToken, ServicoRefreshToken>();

        return servicos;
    }
}
