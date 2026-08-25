using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Application.IoC;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using FIAP.CloudGames.Infrastructure.IoC;
using FIAP.CloudGames.Infrastructure.Repositories.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.Modules.Catalog;

public static class CatalogModuleDependency
{
    public static IServiceCollection AddCatalogModule(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        ArgumentNullException.ThrowIfNull(servicos);
        ArgumentNullException.ThrowIfNull(configuracao);

        var connectionString = configuracao.GetConnectionString(
                InfrastructureDependency.NomeConnectionString)
            ?? InfrastructureDependency.ConexaoPostgreSqlPadrao;

        servicos.RegistrarCatalogApplicationDependency();
        servicos.AddDbContext<CatalogDbContext>(opcoes =>
            opcoes.UseNpgsql(
                connectionString,
                opcoesPostgreSql => opcoesPostgreSql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null)));
        servicos.AddScoped<IRepositorioJogos, RepositorioJogos>();

        return servicos;
    }
}
