using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Mappings.Logs;

public class MapeamentoLogUsuario : IEntityTypeConfiguration<LogUsuario>
{
    public void Configure(EntityTypeBuilder<LogUsuario> builder)
    {
        builder.ToTable("tb_LogUsuarios");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.UsuarioId)
            .HasColumnName("UsuarioId")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.Descricao)
            .HasColumnName("Descricao")
            .HasColumnType("varchar(200)")
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .HasColumnName("DataCriacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UsuarioId);
    }
}
