using FIAP.CloudGames.Application.Catalog.Jogos;
using FIAP.CloudGames.Application.Identity.Usuarios;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.Application.IoC;

/// <summary>
/// Centraliza o registro dos casos de uso e serviços da camada de aplicação.
/// </summary>
public static class ApplicationDependency
{
    public static IServiceCollection RegistrarApplicationDependency(
        this IServiceCollection servicos)
    {
        ArgumentNullException.ThrowIfNull(servicos);

        servicos.AddSingleton(TimeProvider.System);
        servicos.AddScoped<ManipuladorCriarUsuario>();

        servicos.AddScoped<ManipuladorCriarJogo>();
        servicos.AddScoped<ManipuladorObterJogoPorId>();
        servicos.AddScoped<ManipuladorListarJogos>();
        servicos.AddScoped<ManipuladorAtualizarJogo>();

        return servicos;
    }
}
