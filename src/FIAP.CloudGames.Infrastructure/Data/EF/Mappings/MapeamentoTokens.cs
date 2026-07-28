using FIAP.CloudGames.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Mappings;

public class TokenMapping : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.ToTable("tb_Tokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.TokenValue)
            .HasColumnName("Token")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.DataCriacao)
            .IsRequired();

        builder.Property(x => x.DataExpiracao)
            .IsRequired();

        builder.Property(x => x.DataRevogacao)
            .IsRequired(false);
    }
}
