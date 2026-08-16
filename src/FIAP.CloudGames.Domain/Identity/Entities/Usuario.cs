using System;

namespace FIAP.CloudGames.Domain.Identity.Entities;

public sealed class Usuario
{
    public const int TamanhoMaximoNome = 100;
    public const int TamanhoMaximoEmail = 150;

    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string? CPF { get; private set; }
    public DateTimeOffset? DataNascimento { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public Guid PerfilId { get; private set; }
    public bool Ativo { get; private set; }
    public DateTimeOffset CriadoEmUtc { get; private set; }
    public DateTimeOffset? DataInativacao { get; private set; }

    private Usuario()
    {
        Nome = string.Empty;
        Email = string.Empty;
        SenhaHash = string.Empty;
    }

    public Usuario(
        Guid id,
        string nome,
        string email,
        string senhaHash,
        Guid perfilId,
        DateTimeOffset criadoEmUtc,
        string? cpf = null,
        DateTimeOffset? dataNascimento = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("O identificador do usuário não pode ser vazio.", nameof(id));

        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(senhaHash);

        if (nome.Length > TamanhoMaximoNome)
            throw new ArgumentOutOfRangeException(nameof(nome));

        if (email.Length > TamanhoMaximoEmail)
            throw new ArgumentOutOfRangeException(nameof(email));

        if (perfilId == Guid.Empty)
            throw new ArgumentException("O perfil é obrigatório.", nameof(perfilId));

        if (criadoEmUtc.Offset != TimeSpan.Zero)
            throw new ArgumentException("A data de criação deve estar em UTC.", nameof(criadoEmUtc));

        Id = id;
        Nome = nome;
        CPF = cpf;
        DataNascimento = dataNascimento;
        Email = email;
        SenhaHash = senhaHash;
        PerfilId = perfilId;
        Ativo = true;
        CriadoEmUtc = criadoEmUtc;
        DataInativacao = null;
    }

    public void Inativar(DateTimeOffset dataInativacao)
    {
        if (dataInativacao.Offset != TimeSpan.Zero)
            throw new ArgumentException("A data de inativação deve estar em UTC.", nameof(dataInativacao));

        Ativo = false;
        DataInativacao = dataInativacao;
    }
}
