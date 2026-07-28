namespace FIAP.CloudGames.IntegrationTests.Support;

internal static class ConfiguracaoTeste
{
    public static IReadOnlyDictionary<string, string?> CriarValores() =>
        new Dictionary<string, string?>
        {
            ["Swagger:Enabled"] = "true",
            ["ConnectionStrings:PostgreSql"] =
                "Host=localhost;Database=fiap_cloud_games_tests;Username=postgres;Password=tests"
        };
}
