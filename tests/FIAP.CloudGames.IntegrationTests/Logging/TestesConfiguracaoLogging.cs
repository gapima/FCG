using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using FIAP.CloudGames.IntegrationTests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.IntegrationTests.Logging;

public sealed class TestesConfiguracaoLogging : IClassFixture<FabricaApiCloudGames>
{
    private readonly FabricaApiCloudGames _fabrica;

    public TestesConfiguracaoLogging(FabricaApiCloudGames fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public void DependencyInjection_ResolveLoggingDbContext()
    {
        using var escopo = _fabrica.Services.CreateScope();

        var contexto = escopo.ServiceProvider.GetRequiredService<LoggingDbContext>();

        Assert.NotNull(contexto);
    }
}
