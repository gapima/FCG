using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using FIAP.CloudGames.IntegrationTests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.IntegrationTests.Acquisition;

public sealed class TestesConfiguracaoAcquisition : IClassFixture<FabricaApiCloudGames>
{
    private readonly FabricaApiCloudGames _fabrica;

    public TestesConfiguracaoAcquisition(FabricaApiCloudGames fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public void DependencyInjection_ResolveAcquisitionDbContext()
    {
        using var escopo = _fabrica.Services.CreateScope();

        var contexto = escopo.ServiceProvider.GetRequiredService<AcquisitionDbContext>();

        Assert.NotNull(contexto);
    }
}
