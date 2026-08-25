using FIAP.CloudGames.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Mappings.Logs;

public class MapeamentoLogJogo : IEntityTypeConfiguration<LogJogo>
{
    public void Configure(EntityTypeBuilder<LogJogo> builder)
    {
        builder.ToTable("tb_LogJogos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("Id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(x => x.JogoId)
            .HasColumnName("JogoId")
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

        builder.HasIndex(x => x.JogoId);
    }
}
