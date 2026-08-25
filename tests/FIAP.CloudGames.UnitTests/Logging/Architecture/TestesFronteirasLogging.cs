namespace FIAP.CloudGames.UnitTests.Logging.Architecture;

public sealed class TestesFronteirasLogging
{
    private static readonly string RaizLogging = Path.Combine(
        ObterRaizRepositorio(),
        "src",
        "Modules",
        "Logging");

    [Fact]
    public void Domain_NaoDependeDeInfrastructureOuEntityFramework()
    {
        AssertCamadaNaoContem(
            "Domain",
            "FIAP.CloudGames.Infrastructure",
            "Microsoft.EntityFrameworkCore");
    }

    [Fact]
    public void Infrastructure_NaoDependeDeOutrosModulos()
    {
        AssertCamadaNaoContem(
            "Infrastructure",
            "FIAP.CloudGames.Modules.Identity",
            "FIAP.CloudGames.Modules.Catalog",
            "FIAP.CloudGames.Modules.Acquisition");
    }

    private static void AssertCamadaNaoContem(
        string camada,
        params string[] dependenciasProibidas)
    {
        var diretorio = Path.Combine(RaizLogging, camada);
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
            "Não foi possível localizar a raiz do repositório para validar Logging.");
    }
}
