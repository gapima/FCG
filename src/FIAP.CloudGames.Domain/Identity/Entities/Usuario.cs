using System;

namespace FIAP.CloudGames.Domain.Identity.Entities;

public sealed class Usuario
{
    public const int TamanhoMaximoNome = 100;
    public const int TamanhoMaximoEmail = 150;

    public Guid Id { get; private set; }
    public string Nome { get; private set; }
    public string CPF { get; private set; }
    public DateTimeOffset DataNascimento { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public string PerfilId { get; private set; }
    public bool Ativo { get; private set; }
    public DateTimeOffset CriadoEmUtc { get; private set; }
    public DateTimeOffset? DataInativacao { get; private set; } // Anulável pois nasce ativo

    private Usuario() 
    {
        Nome = string.Empty;
        CPF = string.Empty;
        Email = string.Empty;
        SenhaHash = string.Empty;
        PerfilId = string.Empty;
    }

    public Usuario(Guid id, string nome, string cpf, DateTimeOffset dataNascimento, string email, string senhaHash, string perfilId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("O identificador do usuário não pode ser vazio.");

        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome é obrigatório.");

        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("O CPF é obrigatório.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("O e-mail é obrigatório.");

        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("A senha é obrigatória.");

        if (string.IsNullOrWhiteSpace(perfilId))
            throw new ArgumentException("O perfil é obrigatório.");

        Id = id;
        Nome = nome;
        CPF = cpf;
        DataNascimento = dataNascimento;
        Email = email;
        SenhaHash = senhaHash;
        PerfilId = perfilId;
        Ativo = true;
        CriadoEmUtc = DateTimeOffset.UtcNow;
        DataInativacao = null;
    }

    public Usuario(Guid id, string nome, string email, DateTimeOffset criadoEmUtc)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("O identificador do usuário não pode ser vazio.", nameof(id));

        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        if (nome.Length > TamanhoMaximoNome)
            throw new ArgumentOutOfRangeException(nameof(nome));

        if (email.Length > TamanhoMaximoEmail)
            throw new ArgumentOutOfRangeException(nameof(email));

        if (criadoEmUtc.Offset != TimeSpan.Zero)
            throw new ArgumentException("A data de criação deve estar em UTC.", nameof(criadoEmUtc));

        Id = id;
        Nome = nome;
        CPF = string.Empty;
        Email = email;
        SenhaHash = string.Empty;
        PerfilId = string.Empty;
        Ativo = true;
        CriadoEmUtc = criadoEmUtc;
    }
}
