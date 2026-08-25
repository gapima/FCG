using FIAP.CloudGames.Infrastructure.Data.EF;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
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

        var connectionString = ConfiguracaoPostgreSql.ObterConnectionString(configuracao);

        servicos.AddDbContext<AcquisitionDbContext>(opcoes =>
            opcoes.UseNpgsql(
                connectionString,
                opcoesPostgreSql =>
                {
                    opcoesPostgreSql.MigrationsAssembly(typeof(AcquisitionDbContext).Assembly.FullName);
                    opcoesPostgreSql.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        AcquisitionDbContext.Schema);
                    opcoesPostgreSql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));

        return servicos;
    }
}
