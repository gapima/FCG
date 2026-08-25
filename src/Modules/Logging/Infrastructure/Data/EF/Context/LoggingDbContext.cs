using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Mappings.Logs;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Context;

public sealed class LoggingDbContext : DbContext
{
    public LoggingDbContext(DbContextOptions<LoggingDbContext> opcoes)
        : base(opcoes)
    {
    }

    public DbSet<LogUsuario> LogsUsuarios => Set<LogUsuario>();
    public DbSet<LogJogo> LogsJogos => Set<LogJogo>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new MapeamentoLogUsuario());
        modelBuilder.ApplyConfiguration(new MapeamentoLogJogo());
    }
}
