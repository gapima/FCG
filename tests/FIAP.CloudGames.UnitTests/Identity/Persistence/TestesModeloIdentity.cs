using FIAP.CloudGames.Domain.AccessControl.Entities;
using FIAP.CloudGames.Domain.Catalog.Entities;
using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Domain.Identity.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.UnitTests.Identity.Persistence;

public sealed class TestesModeloIdentity
{
    [Fact]
    public void Modelo_ContemSomenteEntidadesDeIdentity()
    {
        using var contexto = CriarContexto();

        Assert.Equal(IdentityDbContext.Schema, contexto.Model.GetDefaultSchema());

        var tiposEsperados = new[]
        {
            typeof(Autorizacao),
            typeof(Perfil),
            typeof(Permissao),
            typeof(Token),
            typeof(Usuario)
        };
        var tiposMapeados = contexto.Model.GetEntityTypes()
            .Select(entidade => entidade.ClrType)
            .OrderBy(tipo => tipo.FullName)
            .ToArray();

        Assert.Equal(
            tiposEsperados.OrderBy(tipo => tipo.FullName),
            tiposMapeados);
        Assert.DoesNotContain(typeof(Jogo), tiposMapeados);
        Assert.DoesNotContain(typeof(Categoria), tiposMapeados);
        Assert.DoesNotContain(typeof(CategoriaJogo), tiposMapeados);
        Assert.DoesNotContain(typeof(Aquisicao), tiposMapeados);
        Assert.DoesNotContain(typeof(LogUsuario), tiposMapeados);
        Assert.DoesNotContain(typeof(LogJogo), tiposMapeados);
    }

    [Fact]
    public void Modelo_AutorizacaoMantemUsuarioEIndiceSemRelacionarJogo()
    {
        using var contexto = CriarContexto();
        var autorizacao = contexto.Model.FindEntityType(typeof(Autorizacao));
        Assert.NotNull(autorizacao);

        var chaveEstrangeira = Assert.Single(autorizacao.GetForeignKeys());
        Assert.Equal(typeof(Usuario), chaveEstrangeira.PrincipalEntityType.ClrType);
        Assert.Equal(nameof(Autorizacao.UsuarioId), Assert.Single(chaveEstrangeira.Properties).Name);

        Assert.Contains(
            autorizacao.GetIndexes(),
            indice => indice.IsUnique
                && indice.Properties.Select(propriedade => propriedade.Name)
                    .SequenceEqual([nameof(Autorizacao.UsuarioId), nameof(Autorizacao.JogoId)]));
    }

    private static IdentityDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=fiap_cloud_games_tests;Username=postgres;Password=tests")
            .Options;

        return new IdentityDbContext(opcoes);
    }
}
