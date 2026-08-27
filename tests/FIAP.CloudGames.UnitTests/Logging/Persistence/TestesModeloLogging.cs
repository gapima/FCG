using FIAP.CloudGames.Domain.AccessControl.Entities;
using FIAP.CloudGames.Domain.Catalog.Entities;
using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Domain.Identity.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.UnitTests.Logging.Persistence;

public sealed class TestesModeloLogging
{
    [Fact]
    public void Modelo_ContemSomenteEntidadesDeLogging()
    {
        using var contexto = CriarContexto();

        Assert.Equal(LoggingDbContext.Schema, contexto.Model.GetDefaultSchema());

        var tiposEsperados = new[]
        {
            typeof(LogJogo),
            typeof(LogUsuario)
        };
        var tiposMapeados = contexto.Model.GetEntityTypes()
            .Select(entidade => entidade.ClrType)
            .OrderBy(tipo => tipo.FullName)
            .ToArray();

        Assert.Equal(
            tiposEsperados.OrderBy(tipo => tipo.FullName),
            tiposMapeados);
        Assert.DoesNotContain(typeof(Usuario), tiposMapeados);
        Assert.DoesNotContain(typeof(Perfil), tiposMapeados);
        Assert.DoesNotContain(typeof(Token), tiposMapeados);
        Assert.DoesNotContain(typeof(Permissao), tiposMapeados);
        Assert.DoesNotContain(typeof(Autorizacao), tiposMapeados);
        Assert.DoesNotContain(typeof(Jogo), tiposMapeados);
        Assert.DoesNotContain(typeof(Categoria), tiposMapeados);
        Assert.DoesNotContain(typeof(CategoriaJogo), tiposMapeados);
        Assert.DoesNotContain(typeof(Aquisicao), tiposMapeados);
    }

    [Fact]
    public void Modelo_PreservaIdsEIndicesSemForeignKeysCrossModule()
    {
        using var contexto = CriarContexto();
        var logUsuario = contexto.Model.FindEntityType(typeof(LogUsuario));
        var logJogo = contexto.Model.FindEntityType(typeof(LogJogo));
        Assert.NotNull(logUsuario);
        Assert.NotNull(logJogo);

        Assert.Empty(logUsuario.GetForeignKeys());
        Assert.Empty(logJogo.GetForeignKeys());
        Assert.NotNull(logUsuario.FindProperty(nameof(LogUsuario.UsuarioId)));
        Assert.NotNull(logJogo.FindProperty(nameof(LogJogo.JogoId)));
        Assert.Contains(
            logUsuario.GetIndexes(),
            indice => Assert.Single(indice.Properties).Name == nameof(LogUsuario.UsuarioId));
        Assert.Contains(
            logJogo.GetIndexes(),
            indice => Assert.Single(indice.Properties).Name == nameof(LogJogo.JogoId));
    }

    private static LoggingDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<LoggingDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=fiap_cloud_games_tests;Username=postgres;Password=tests")
            .Options;

        return new LoggingDbContext(opcoes);
    }
}
