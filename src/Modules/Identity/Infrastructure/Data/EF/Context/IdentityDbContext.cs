using FIAP.CloudGames.Domain.AccessControl.Entities;
using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Domain.Identity.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Mappings;
using FIAP.CloudGames.Infrastructure.Data.EF.Mappings.AccessControl;
using FIAP.CloudGames.Infrastructure.Data.EF.Mappings.Identity;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Context;

public sealed class IdentityDbContext : DbContext
{
    public const string Schema = "identity";

    public IdentityDbContext(DbContextOptions<IdentityDbContext> opcoes)
        : base(opcoes)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<Token> Tokens => Set<Token>();
    public DbSet<Permissao> Permissoes => Set<Permissao>();
    public DbSet<Autorizacao> Autorizacoes => Set<Autorizacao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfiguration(new MapeamentoUsuario());
        modelBuilder.ApplyConfiguration(new MapeamentoPerfil());
        modelBuilder.ApplyConfiguration(new TokenMapping());
        modelBuilder.ApplyConfiguration(new MapeamentoPermissao());
        modelBuilder.ApplyConfiguration(new AutorizacaoMapping());
    }
}
