using FIAP.CloudGames.Application.Abstractions.Security;
using FIAP.CloudGames.Application.Identity.Auth;
using FIAP.CloudGames.Application.Identity.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.Application.IoC;

public static class IdentityApplicationDependency
{
    public static IServiceCollection RegistrarIdentityApplicationDependency(
        this IServiceCollection servicos)
    {
        ArgumentNullException.ThrowIfNull(servicos);

        servicos.AddSingleton(TimeProvider.System);
        servicos.AddSingleton<IHashSenha, HashSenhaPbkdf2>();
        servicos.AddScoped<ManipuladorCriarUsuario>();
        servicos.AddScoped<ManipuladorObterUsuario>();
        servicos.AddScoped<ManipuladorAtualizarUsuario>();
        servicos.AddScoped<ManipuladorAlterarPerfilUsuario>();
        servicos.AddScoped<ManipuladorLogin>();
        servicos.AddScoped<ManipuladorRenovarToken>();
        servicos.AddScoped<ManipuladorLogout>();

        return servicos;
    }
}
