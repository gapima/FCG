namespace FIAP.CloudGames.UnitTests.Identity.Architecture;

public sealed class TestesFronteirasIdentity
{
    private static readonly string RaizIdentity = Path.Combine(
        ObterRaizRepositorio(),
        "src",
        "Modules",
        "Identity");

    [Fact]
    public void Domain_NaoDependeDeCamadasExternasOuEntityFramework()
    {
        AssertCamadaNaoContem(
            "Domain",
            "FIAP.CloudGames.Application",
            "FIAP.CloudGames.Infrastructure",
            "FIAP.CloudGames.Api",
            "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Application_NaoDependeDeInfrastructureOuApi()
    {
        AssertCamadaNaoContem(
            "Application",
            "FIAP.CloudGames.Infrastructure",
            "FIAP.CloudGames.Api");
    }

    [Fact]
    public void Api_NaoAcessaIdentityDbContext()
    {
        AssertCamadaNaoContem(
            "Api",
            "IdentityDbContext",
            "FIAP.CloudGames.Infrastructure.Data.EF.Context");
    }

    private static void AssertCamadaNaoContem(
        string camada,
        params string[] dependenciasProibidas)
    {
        var diretorio = Path.Combine(RaizIdentity, camada);
        var arquivos = Directory.EnumerateFiles(
            diretorio,
            "*.cs",
            SearchOption.AllDirectories);

        foreach (var arquivo in arquivos)
        {
            var conteudo = File.ReadAllText(arquivo);
            foreach (var dependencia in dependenciasProibidas)
            {
                Assert.DoesNotContain(
                    dependencia,
                    conteudo,
                    StringComparison.Ordinal);
            }
        }
    }

    private static string ObterRaizRepositorio()
    {
        var diretorio = new DirectoryInfo(AppContext.BaseDirectory);
        while (diretorio is not null)
        {
            if (File.Exists(Path.Combine(diretorio.FullName, "FIAP.CloudGames.sln")))
                return diretorio.FullName;

            diretorio = diretorio.Parent;
        }

        throw new DirectoryNotFoundException(
            "Não foi possível localizar a raiz do repositório para validar Identity.");
    }
}
