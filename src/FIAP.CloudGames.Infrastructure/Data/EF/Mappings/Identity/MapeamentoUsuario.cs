using FIAP.CloudGames.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Mappings.Identity;

internal sealed class MapeamentoUsuario : IEntityTypeConfiguration<Usuario>
{
    public const string NomeIndiceEmailUnico = "ux_usuarios_email";

    public void Configure(EntityTypeBuilder<Usuario> construtor)
    {
        construtor.ToTable("usuarios");

        construtor.HasKey(usuario => usuario.Id)
            .HasName("pk_usuarios");

        construtor.Property(usuario => usuario.Id)
            .HasColumnName("id")
            .ValueGeneratedNever();

        construtor.Property(usuario => usuario.Nome)
            .HasColumnName("nome")
            .HasMaxLength(100)
            .IsRequired();

        construtor.Property(usuario => usuario.CPF)
            .HasColumnName("cpf")
            .HasMaxLength(100)
            .IsRequired();

        construtor.Property(usuario => usuario.DataNascimento)
            .HasColumnName("data_nascimento")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        construtor.Property(usuario => usuario.Email)
            .HasColumnName("email")
            .HasMaxLength(150)
            .IsRequired();

        construtor.Property(usuario => usuario.SenhaHash)
            .HasColumnName("senha_hash")
            .HasMaxLength(255)
            .IsRequired();

        construtor.Property(usuario => usuario.PerfilId)
            .HasColumnName("perfil_id")
            .HasMaxLength(20)
            .IsRequired();

        construtor.Property(usuario => usuario.Ativo)
            .HasColumnName("ativo")
            .HasColumnType("boolean")
            .HasDefaultValue(true)
            .IsRequired();

        construtor.Property(usuario => usuario.CriadoEmUtc)
            .HasColumnName("criado_em_utc")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        construtor.Property(usuario => usuario.DataInativacao)
            .HasColumnName("data_inativacao")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        construtor.HasIndex(usuario => usuario.Email)
            .IsUnique()
            .HasDatabaseName(NomeIndiceEmailUnico);
    }
}
