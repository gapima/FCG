using FIAP.CloudGames.Domain.Catalog.Entities;
using FIAP.CloudGames.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Context;

/// <summary>
/// Unidade de trabalho do Entity Framework Core para o PostgreSQL da aplicação.
/// </summary>
public sealed class PostgresqlDbContext : DbContext
{
    public PostgresqlDbContext(
        DbContextOptions<PostgresqlDbContext> opcoes)
        : base(opcoes)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();

    public DbSet<Jogo> Jogos => Set<Jogo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PostgresqlDbContext).Assembly);
    }
}
