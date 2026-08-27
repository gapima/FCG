using FIAP.CloudGames.Application.Catalog.Jogos;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.Application.IoC;

public static class CatalogApplicationDependency
{
    public static IServiceCollection RegistrarCatalogApplicationDependency(
        this IServiceCollection servicos)
    {
        ArgumentNullException.ThrowIfNull(servicos);

        servicos.AddScoped<ManipuladorCriarJogo>();
        servicos.AddScoped<ManipuladorObterJogoPorId>();
        servicos.AddScoped<ManipuladorListarJogos>();
        servicos.AddScoped<ManipuladorAtualizarJogo>();

        return servicos;
    }
}
