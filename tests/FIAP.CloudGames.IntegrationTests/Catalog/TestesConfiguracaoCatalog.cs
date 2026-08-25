using System.Text.Json;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using FIAP.CloudGames.IntegrationTests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.IntegrationTests.Catalog;

public sealed class TestesConfiguracaoCatalog : IClassFixture<FabricaApiCloudGames>
{
    private readonly FabricaApiCloudGames _fabrica;

    public TestesConfiguracaoCatalog(FabricaApiCloudGames fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public void DependencyInjection_ResolveCatalogDbContext()
    {
        using var escopo = _fabrica.Services.CreateScope();

        var contexto = escopo.ServiceProvider.GetRequiredService<CatalogDbContext>();

        Assert.NotNull(contexto);
    }

    [Fact]
    public async Task Swagger_PublicaEndpointsDeCatalog()
    {
        using var cliente = _fabrica.CreateClient();

        var json = await cliente.GetStringAsync(
            "/swagger/v1/swagger.json",
            TestContext.Current.CancellationToken);
        using var documento = JsonDocument.Parse(json);
        var caminhos = documento.RootElement.GetProperty("paths");

        var colecao = caminhos.GetProperty("/api/v1/jogos");
        Assert.True(colecao.TryGetProperty("get", out _));
        Assert.True(colecao.TryGetProperty("post", out _));

        var item = caminhos.GetProperty("/api/v1/jogos/{id}");
        Assert.True(item.TryGetProperty("get", out _));
        Assert.True(item.TryGetProperty("put", out _));
    }
}
