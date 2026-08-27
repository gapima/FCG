using FIAP.CloudGames.Modules.Acquisition;
using FIAP.CloudGames.Modules.Catalog;
using FIAP.CloudGames.Modules.Identity;
using FIAP.CloudGames.Modules.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.IntegrationTests.Configuration;

public sealed class TestesConfiguracaoPostgreSqlModulos
{
    [Fact]
    public void Modulos_SemConnectionStringPostgreSql_FalhamDuranteRegistro()
    {
        var configuracao = new ConfigurationBuilder().Build();

        AssertFalha(servicos => servicos.AddIdentityModule(configuracao));
        AssertFalha(servicos => servicos.AddCatalogModule(configuracao));
        AssertFalha(servicos => servicos.AddAcquisitionModule(configuracao));
        AssertFalha(servicos => servicos.AddLoggingModule(configuracao));
    }

    private static void AssertFalha(Action<IServiceCollection> registrar)
    {
        var excecao = Assert.Throws<InvalidOperationException>(
            () => registrar(new ServiceCollection()));

        Assert.Equal(
            "ConnectionStrings:PostgreSql deve ser configurada.",
            excecao.Message);
    }
}
