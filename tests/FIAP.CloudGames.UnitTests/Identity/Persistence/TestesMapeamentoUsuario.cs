using FIAP.CloudGames.Domain.Identity.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FIAP.CloudGames.UnitTests.Identity.Persistence;

public sealed class TestesMapeamentoUsuario
{
    [Fact]
    public void Modelo_ConfiguraUsuarioParaPostgreSqlComEmailUnico()
    {
        var opcoes = new DbContextOptionsBuilder<PostgresqlDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=fiap_cloud_games_tests;Username=postgres;Password=tests")
            .Options;
        using var contexto = new PostgresqlDbContext(opcoes);

        var entidade = contexto.Model.FindEntityType(typeof(Usuario));
        Assert.NotNull(entidade);

        var tabela = StoreObjectIdentifier.Table(
            entidade.GetTableName()!,
            entidade.GetSchema());
        var indiceEmail = entidade.GetIndexes().Single(indice => indice.IsUnique);

        Assert.Equal("Npgsql.EntityFrameworkCore.PostgreSQL", contexto.Database.ProviderName);
        Assert.Equal("usuarios", entidade.GetTableName());
        Assert.Equal("ux_usuarios_email", indiceEmail.GetDatabaseName());
        Assert.Equal(
            "criado_em_utc",
            entidade.FindProperty(nameof(Usuario.CriadoEmUtc))!.GetColumnName(tabela));
    }
}
