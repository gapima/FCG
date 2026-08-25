using FIAP.CloudGames.Domain.AccessControl.Entities;
using FIAP.CloudGames.Domain.Catalog.Entities;
using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Domain.Identity.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.UnitTests.Catalog.Persistence;

public sealed class TestesModeloCatalog
{
    [Fact]
    public void Modelo_ContemSomenteEntidadesDeCatalog()
    {
        using var contexto = CriarContexto();

        var tiposEsperados = new[]
        {
            typeof(Categoria),
            typeof(CategoriaJogo),
            typeof(Jogo)
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
        Assert.DoesNotContain(typeof(Aquisicao), tiposMapeados);
        Assert.DoesNotContain(typeof(LogUsuario), tiposMapeados);
        Assert.DoesNotContain(typeof(LogJogo), tiposMapeados);
    }

    [Fact]
    public void Modelo_CategoriaJogoMantemRelacionamentosInternosEIndiceUnico()
    {
        using var contexto = CriarContexto();
        var categoriaJogo = contexto.Model.FindEntityType(typeof(CategoriaJogo));
        Assert.NotNull(categoriaJogo);

        var chavesEstrangeiras = categoriaJogo.GetForeignKeys().ToArray();
        Assert.Equal(2, chavesEstrangeiras.Length);
        Assert.Contains(
            chavesEstrangeiras,
            chave => chave.PrincipalEntityType.ClrType == typeof(Jogo)
                && Assert.Single(chave.Properties).Name == nameof(CategoriaJogo.JogoId));
        Assert.Contains(
            chavesEstrangeiras,
            chave => chave.PrincipalEntityType.ClrType == typeof(Categoria)
                && Assert.Single(chave.Properties).Name == nameof(CategoriaJogo.CategoriaId));
        Assert.All(
            chavesEstrangeiras,
            chave => Assert.Equal(DeleteBehavior.Cascade, chave.DeleteBehavior));

        Assert.Contains(
            categoriaJogo.GetIndexes(),
            indice => indice.IsUnique
                && indice.Properties.Select(propriedade => propriedade.Name)
                    .SequenceEqual([nameof(CategoriaJogo.JogoId), nameof(CategoriaJogo.CategoriaId)]));
    }

    private static CatalogDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<CatalogDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=fiap_cloud_games_tests;Username=postgres;Password=tests")
            .Options;

        return new CatalogDbContext(opcoes);
    }
}
