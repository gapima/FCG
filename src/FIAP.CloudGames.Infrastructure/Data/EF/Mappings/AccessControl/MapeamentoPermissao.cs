using FIAP.CloudGames.Domain.AccessControl.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Mappings.AccessControl;

internal sealed class MapeamentoPermissao : IEntityTypeConfiguration<Permissao>
{
    public void Configure(EntityTypeBuilder<Permissao> construtor)
    {
        construtor.ToTable("permissoes");

        construtor.HasKey(permissao => new { permissao.Id, permissao.PerfilId })
            .HasName("pk_permissoes");

        construtor.Property(permissao => permissao.Id)
            .HasColumnName("id");

        construtor.Property(permissao => permissao.PerfilId)
            .HasColumnName("perfil_id");

        construtor.Property(permissao => permissao.Nome)
            .HasColumnName("nome")
            .HasMaxLength(200)
            .IsRequired();

        construtor.Property(permissao => permissao.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(200)
            .IsRequired(false);
    }
}
