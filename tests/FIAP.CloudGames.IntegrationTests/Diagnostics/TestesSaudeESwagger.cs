using System.Net;
using System.Text.Json;
using FIAP.CloudGames.IntegrationTests.Support;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

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
    public async Task Swagger_PublicaLoginEEsquemaBearer()
    {
        using var cliente = _fabrica.CreateClient();

        var json = await cliente.GetStringAsync(
            "/swagger/v1/swagger.json",
            TestContext.Current.CancellationToken);
        using var documento = JsonDocument.Parse(json);

        var raiz = documento.RootElement;
        var componentes = raiz.GetProperty("components");

        Assert.True(
            componentes
                .GetProperty("securitySchemes")
                .TryGetProperty("Bearer", out var bearer));
        Assert.Equal("http", bearer.GetProperty("type").GetString());
        Assert.Equal("bearer", bearer.GetProperty("scheme").GetString());
        Assert.Equal(
            "Bearer",
            raiz.GetProperty("security")[0].EnumerateObject().Single().Name);
        Assert.True(raiz
            .GetProperty("paths")
            .TryGetProperty("/api/v1/auth/login", out var login));
        Assert.Empty(login.GetProperty("post").GetProperty("security").EnumerateArray());
    }

    [Fact]
    public async Task Autenticacao_ConfiguraBearerComoEsquemaPadrao()
    {
        var provedor = _fabrica.Services.GetRequiredService<IAuthenticationSchemeProvider>();

        var esquema = await provedor.GetDefaultAuthenticateSchemeAsync();

        Assert.NotNull(esquema);
        Assert.Equal("Bearer", esquema.Name);
    }
}
