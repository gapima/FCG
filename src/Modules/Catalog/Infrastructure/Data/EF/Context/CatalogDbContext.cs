using FIAP.CloudGames.Domain.Catalog.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Mappings.Catalog;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Context;

public sealed class CatalogDbContext : DbContext
{
    public const string Schema = "catalog";

    public CatalogDbContext(DbContextOptions<CatalogDbContext> opcoes)
        : base(opcoes)
    {
    }

    public DbSet<Jogo> Jogos => Set<Jogo>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<CategoriaJogo> CategoriasJogos => Set<CategoriaJogo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new MapeamentoJogo());
        modelBuilder.ApplyConfiguration(new MapeamentoCategoria());
        modelBuilder.ApplyConfiguration(new MapeamentoCategoriaJogo());
    }
}
