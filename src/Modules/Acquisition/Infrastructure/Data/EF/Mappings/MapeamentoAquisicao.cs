using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using FIAP.CloudGames.Domain.Entities;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Mappings;

internal sealed class MapeamentoAquisicao : IEntityTypeConfiguration<Aquisicao>
{
    public void Configure(EntityTypeBuilder<Aquisicao> construtor)
    {
        construtor.ToTable("aquisicoes");

        construtor.HasKey(aquisicao => new { aquisicao.Id, aquisicao.UsuarioId, aquisicao.JogoId })
            .HasName("pk_aquisicoes");

        construtor.Property(aquisicao => aquisicao.Id)
            .HasColumnName("id");

        construtor.Property(aquisicao => aquisicao.UsuarioId)
            .HasColumnName("usuario_id");

        construtor.Property(aquisicao => aquisicao.JogoId)
            .HasColumnName("jogo_id");

        // Ajustado para DateTimeOffset
        construtor.Property(aquisicao => aquisicao.DataAquisicao)
            .HasColumnName("data_aquisicao")
            .HasColumnType("timestamp with time zone")
            .IsRequired();
    }
}
