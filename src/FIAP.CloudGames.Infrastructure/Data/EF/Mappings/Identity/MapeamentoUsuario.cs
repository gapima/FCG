using FIAP.CloudGames.Domain.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Infrastructure.Data.EF.Mappings.Identity;

internal sealed class MapeamentoUsuario : IEntityTypeConfiguration<Usuario>
{
    public const string NomeIndiceEmailUnico = "ux_usuarios_email";

    public void Configure(EntityTypeBuilder<Usuario> construtor)
    {
        construtor.ToTable("usuarios", tabela =>
            tabela.HasCheckConstraint(
                "ck_usuarios_perfil_id_nao_vazio",
                "\"perfil_id\" <> '00000000-0000-0000-0000-000000000000'::uuid"));

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
            .IsRequired(false);

        construtor.Property(usuario => usuario.DataNascimento)
            .HasColumnName("data_nascimento")
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

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
            .HasColumnType("uuid")
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

        construtor.HasIndex(usuario => usuario.PerfilId)
            .HasDatabaseName("ix_usuarios_perfil_id");

        construtor.HasOne<Perfil>()
            .WithMany()
            .HasForeignKey(usuario => usuario.PerfilId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("fk_usuarios_perfis");
    }
}
