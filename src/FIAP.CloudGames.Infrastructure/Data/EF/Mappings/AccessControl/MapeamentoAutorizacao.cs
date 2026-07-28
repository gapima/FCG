using FIAP.CloudGames.Domain.AccessControl.Entities;
using FIAP.CloudGames.Domain.Catalog.Entities;
using FIAP.CloudGames.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Mappings.AccessControl;

public class AutorizacaoMapping : IEntityTypeConfiguration<Autorizacao>
{
    public void Configure(EntityTypeBuilder<Autorizacao> builder)
    {
        builder.ToTable("tb_Autorizacao");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Nome)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Jogo>()
            .WithMany()
            .HasForeignKey(x => x.JogoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UsuarioId, x.JogoId })
            .IsUnique();
    }
}
