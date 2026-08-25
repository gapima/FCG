using FIAP.CloudGames.Infrastructure.Data.EF;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.Modules.Logging;

public static class LoggingModuleDependency
{
    public static IServiceCollection AddLoggingModule(
        this IServiceCollection servicos,
        IConfiguration configuracao)
    {
        ArgumentNullException.ThrowIfNull(servicos);
        ArgumentNullException.ThrowIfNull(configuracao);

        var connectionString = ConfiguracaoPostgreSql.ObterConnectionString(configuracao);

        servicos.AddDbContext<LoggingDbContext>(opcoes =>
            opcoes.UseNpgsql(
                connectionString,
                opcoesPostgreSql =>
                {
                    opcoesPostgreSql.MigrationsAssembly(typeof(LoggingDbContext).Assembly.FullName);
                    opcoesPostgreSql.MigrationsHistoryTable(
                        "__EFMigrationsHistory",
                        LoggingDbContext.Schema);
                    opcoesPostgreSql.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null);
                }));

        return servicos;
    }
}
