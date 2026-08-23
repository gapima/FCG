using FIAP.CloudGames.Domain.Catalog.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using Microsoft.EntityFrameworkCore;
namespace FIAP.CloudGames.UnitTests.Catalog.Persistence;

public sealed class TestesMapeamentoCategoria
{
    // Verifica se o modelo de dados do EF Core configura corretamente 
    // a propriedade Nome da entidade Categoria como obrigatória (não nula).
    [Fact]
    public void Modelo_ConfiguraNomeDaCategoriaComoObrigatorio()
    {
        // Arrange
        var opcoes = new DbContextOptionsBuilder<PostgresqlDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=FIAP.CloudGames;Username=postgres;Password=postgres")
            .Options;
        using var contexto = new PostgresqlDbContext(opcoes);

        // Act
        var entidade = contexto.Model.FindEntityType(typeof(Categoria));   
        Assert.NotNull(entidade);

        var propriedade = entidade.FindProperty(nameof(Categoria.Nome));

        // Assert
        Assert.NotNull(propriedade);
        Assert.False(propriedade.IsNullable);
    }

    // Verifica se o modelo de dados do EF Core configura corretamente 
    // a propriedade Nome da entidade Categoria com o tamanho máximo de 200 caracteres.
    [Fact]
    public void Modelo_ConfiguraNomeDaCategoriaComTamanhoMaximoDe200Caracteres()
    {
        // Arrange
        var opcoes = new DbContextOptionsBuilder<PostgresqlDbContext>()
            .UseNpgsql(
                "Host=localhost;Database=FIAP.CloudGames;Username=postgres;Password=postgres")
            .Options;
        using var contexto = new PostgresqlDbContext(opcoes);

        // Act
        var entidade = contexto.Model.FindEntityType(typeof(Categoria));   
        Assert.NotNull(entidade);

        var propriedade = entidade.FindProperty(nameof(Categoria.Nome));

        // Assert
        Assert.NotNull(propriedade);
        Assert.Equal(200, propriedade.GetMaxLength());
    }

}

