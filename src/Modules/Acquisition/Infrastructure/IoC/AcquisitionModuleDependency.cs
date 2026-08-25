using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using FIAP.CloudGames.Infrastructure.IoC;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.Modules.Acquisition;

public static class AcquisitionModuleDependency
{
    public static IServiceCollection AddAcquisitionModule(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        ArgumentNullException.ThrowIfNull(servicos);
        ArgumentNullException.ThrowIfNull(configuracao);

        var connectionString = configuracao.GetConnectionString(
                InfrastructureDependency.NomeConnectionString)
            ?? InfrastructureDependency.ConexaoPostgreSqlPadrao;

        servicos.AddDbContext<AcquisitionDbContext>(opcoes =>
            opcoes.UseNpgsql(
                connectionString,
                opcoesPostgreSql => opcoesPostgreSql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null)));

        return servicos;
    }
}
