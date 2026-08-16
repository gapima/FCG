using System.Net;
using System.Text.Json;
using FIAP.CloudGames.IntegrationTests.Support;

namespace FIAP.CloudGames.IntegrationTests.Diagnostics;

public sealed class TestesSaudeESwagger : IClassFixture<FabricaApiCloudGames>
{
    private readonly FabricaApiCloudGames _fabrica;

    public TestesSaudeESwagger(FabricaApiCloudGames fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Saude_RetornaOk()
    {
        using var cliente = _fabrica.CreateClient();

        var resposta = await cliente.GetAsync("/health", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Swagger_DescreveEndpointCriacaoUsuario()
    {
        using var cliente = _fabrica.CreateClient();

        var json = await cliente.GetStringAsync(
            "/swagger/v1/swagger.json",
            TestContext.Current.CancellationToken);
        using var documento = JsonDocument.Parse(json);

        var operacaoCriacaoUsuario = documento.RootElement
            .GetProperty("paths")
            .GetProperty("/api/v1/usuarios")
            .GetProperty("post");

        Assert.Equal("Usuários", operacaoCriacaoUsuario.GetProperty("tags")[0].GetString());
        Assert.True(operacaoCriacaoUsuario.GetProperty("responses").TryGetProperty("201", out _));
    }

    [Fact]
    public async Task Swagger_NaoPublicaEsquemaDeSeguranca()
    {
        using var cliente = _fabrica.CreateClient();

        var json = await cliente.GetStringAsync(
            "/swagger/v1/swagger.json",
            TestContext.Current.CancellationToken);
        using var documento = JsonDocument.Parse(json);

        var componentes = documento.RootElement.GetProperty("components");

        Assert.False(componentes.TryGetProperty("securitySchemes", out _));
    }
}
