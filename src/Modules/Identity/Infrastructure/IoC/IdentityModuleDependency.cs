using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Application.Abstractions.Security;
using FIAP.CloudGames.Application.IoC;
using FIAP.CloudGames.Infrastructure.Repositories.Identity;
using FIAP.CloudGames.Infrastructure.Security;
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

        servicos.RegistrarIdentityApplicationDependency();
        servicos.AddSingleton(ConfiguracaoJwt.Criar(configuracao));
        servicos.AddScoped<IRepositoryUsuarios, RepositorioUsuarios>();
        servicos.AddScoped<IRepositorioTokens, RepositorioTokens>();
        servicos.AddScoped<IServicoHashSenha, ServicoHashSenha>();
        servicos.AddScoped<IServicoTokenJwt, ServicoTokenJwt>();
        servicos.AddScoped<IServicoRefreshToken, ServicoRefreshToken>();

        return servicos;
    }
}
