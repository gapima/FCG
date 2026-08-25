using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Mappings;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Context;

public sealed class AcquisitionDbContext : DbContext
{
    public AcquisitionDbContext(DbContextOptions<AcquisitionDbContext> opcoes)
        : base(opcoes)
    {
    }

    public DbSet<Aquisicao> Aquisicoes => Set<Aquisicao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MapeamentoAquisicao());
    }
}
