using FIAP.CloudGames.Domain.Identity.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.UnitTests.Identity.Persistence;

public sealed class TestesRelacionamentoUsuarioPerfil
{
    private static readonly Guid PerfilId =
        Guid.Parse("4f642cbc-3720-4bb2-b456-15a97049da5c");

    [Fact]
    public void Construtor_ComPerfilVazio_RejeitaUsuario()
    {
        var excecao = Assert.Throws<ArgumentException>(() => new Usuario(
            Guid.NewGuid(),
            "Maria da Silva",
            "maria@exemplo.com",
            Guid.Empty,
            DateTimeOffset.UtcNow));

        Assert.Equal("perfilId", excecao.ParamName);
    }

    [Fact]
    public void Construtor_ComPerfilValido_CriaUsuarioComPerfilInformado()
    {
        var usuario = new Usuario(
            Guid.NewGuid(),
            "Maria da Silva",
            "maria@exemplo.com",
            PerfilId,
            DateTimeOffset.UtcNow);

        Assert.Equal(PerfilId, usuario.PerfilId);
        Assert.Equal(typeof(Guid), typeof(Usuario).GetProperty(nameof(Usuario.PerfilId))!.PropertyType);
    }

    [Fact]
    public void Modelo_ConfiguraRelacionamentoRestritivoEntreUsuarioEPerfil()
    {
        var opcoes = new DbContextOptionsBuilder<PostgresqlDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=fiap_cloud_games_tests;Username=postgres;Password=tests")
            .Options;
        using var contexto = new PostgresqlDbContext(opcoes);

        var entidadeUsuario = contexto.Model.FindEntityType(typeof(Usuario));
        Assert.NotNull(entidadeUsuario);

        var chaveEstrangeira = Assert.Single(
            entidadeUsuario.GetForeignKeys(),
            chave => chave.PrincipalEntityType.ClrType == typeof(Perfil));

        Assert.Equal(nameof(Usuario.PerfilId), Assert.Single(chaveEstrangeira.Properties).Name);
        Assert.Equal(DeleteBehavior.Restrict, chaveEstrangeira.DeleteBehavior);
        Assert.Equal("fk_usuarios_perfis", chaveEstrangeira.GetConstraintName());
        Assert.NotNull(contexto.Perfis);
    }
}
