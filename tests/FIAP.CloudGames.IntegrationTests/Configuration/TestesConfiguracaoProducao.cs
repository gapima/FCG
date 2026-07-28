using System.Net;
using FIAP.CloudGames.IntegrationTests.Support;

namespace FIAP.CloudGames.IntegrationTests.Configuration;

public sealed class TestesConfiguracaoProducao
{
    [Fact]
    public async Task Producao_NaoExpoeSwaggerEMantemSaudeDisponivel()
    {
        using var fabrica = new FabricaApiCloudGames("Production");
        using var cliente = fabrica.CreateClient();
        var tokenCancelamento = TestContext.Current.CancellationToken;

        var respostaSwagger = await cliente.GetAsync(
            "/swagger/v1/swagger.json",
            tokenCancelamento);
        var respostaSaude = await cliente.GetAsync("/health", tokenCancelamento);

        Assert.Equal(HttpStatusCode.NotFound, respostaSwagger.StatusCode);
        Assert.Equal(HttpStatusCode.OK, respostaSaude.StatusCode);
    }
}
