using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Application.Abstractions.Security;
using FIAP.CloudGames.Application.IoC;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using FIAP.CloudGames.Infrastructure.Repositories.Identity;
using FIAP.CloudGames.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.Modules.Identity;

public static class IdentityModuleDependency
{
    public static IServiceCollection AddIdentityModule(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        ArgumentNullException.ThrowIfNull(servicos);
        ArgumentNullException.ThrowIfNull(configuracao);

        var connectionString = configuracao.GetConnectionString("PostgreSql");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "ConnectionStrings:PostgreSql deve ser configurada.");

        servicos.RegistrarIdentityApplicationDependency();
        servicos.AddDbContext<IdentityDbContext>(opcoes =>
            opcoes.UseNpgsql(
                connectionString,
                opcoesPostgreSql =>
                {
                    opcoesPostgreSql.MigrationsAssembly(typeof(IdentityDbContext).Assembly.FullName);
                    opcoesPostgreSql.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        IdentityDbContext.Schema);
                    opcoesPostgreSql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));
        servicos.AddSingleton(ConfiguracaoJwt.Criar(configuracao));
        servicos.AddScoped<IRepositoryUsuarios, RepositorioUsuarios>();
        servicos.AddScoped<IRepositorioTokens, RepositorioTokens>();
        servicos.AddScoped<IServicoHashSenha, ServicoHashSenha>();
        servicos.AddScoped<IServicoTokenJwt, ServicoTokenJwt>();
        servicos.AddScoped<IServicoRefreshToken, ServicoRefreshToken>();

        return servicos;
    }
}
