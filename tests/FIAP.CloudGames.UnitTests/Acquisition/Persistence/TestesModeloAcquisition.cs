using FIAP.CloudGames.Domain.AccessControl.Entities;
using FIAP.CloudGames.Domain.Catalog.Entities;
using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Domain.Identity.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.UnitTests.Acquisition.Persistence;

public sealed class TestesModeloAcquisition
{
    [Fact]
    public void Modelo_ContemSomenteAquisicao()
    {
        using var contexto = CriarContexto();

        var tiposMapeados = contexto.Model.GetEntityTypes()
            .Select(entidade => entidade.ClrType)
            .ToArray();

        Assert.Equal([typeof(Aquisicao)], tiposMapeados);
        Assert.DoesNotContain(typeof(Usuario), tiposMapeados);
        Assert.DoesNotContain(typeof(Perfil), tiposMapeados);
        Assert.DoesNotContain(typeof(Token), tiposMapeados);
        Assert.DoesNotContain(typeof(Permissao), tiposMapeados);
        Assert.DoesNotContain(typeof(Autorizacao), tiposMapeados);
        Assert.DoesNotContain(typeof(Jogo), tiposMapeados);
        Assert.DoesNotContain(typeof(Categoria), tiposMapeados);
        Assert.DoesNotContain(typeof(CategoriaJogo), tiposMapeados);
        Assert.DoesNotContain(typeof(LogUsuario), tiposMapeados);
        Assert.DoesNotContain(typeof(LogJogo), tiposMapeados);
    }

    [Fact]
    public void Modelo_PreservaChaveCompostaSemForeignKeysCrossModule()
    {
        using var contexto = CriarContexto();
        var aquisicao = contexto.Model.FindEntityType(typeof(Aquisicao));
        Assert.NotNull(aquisicao);

        Assert.Empty(aquisicao.GetForeignKeys());
        Assert.Equal(
            [nameof(Aquisicao.Id), nameof(Aquisicao.UsuarioId), nameof(Aquisicao.JogoId)],
            aquisicao.FindPrimaryKey()!.Properties.Select(propriedade => propriedade.Name));
    }

    private static AcquisitionDbContext CriarContexto()
    {
        var opcoes = new DbContextOptionsBuilder<AcquisitionDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=fiap_cloud_games_tests;Username=postgres;Password=tests")
            .Options;

        return new AcquisitionDbContext(opcoes);
    }
}
