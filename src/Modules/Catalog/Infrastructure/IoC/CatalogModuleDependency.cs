using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Application.IoC;
using FIAP.CloudGames.Infrastructure.Repositories.Catalog;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.Modules.Catalog;

public static class CatalogModuleDependency
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection servicos)
    {
        ArgumentNullException.ThrowIfNull(servicos);

        servicos.RegistrarCatalogApplicationDependency();
        servicos.AddScoped<IRepositorioJogos, RepositorioJogos>();

        return servicos;
    }
}
