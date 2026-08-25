using Microsoft.Extensions.Configuration;

namespace FIAP.CloudGames.Infrastructure.Data.EF;

internal static class ConfiguracaoPostgreSql
{
    internal const string NomeConnectionString = "PostgreSql";

    private const string ConexaoPadrao =
        "Host=localhost;Port=5432;Database=fiap_cloud_games;Username=postgres;Password=@Testesenha123456";

    internal static string ObterConnectionString(IConfiguration configuracao) =>
        configuracao.GetConnectionString(NomeConnectionString) ?? ConexaoPadrao;
}
