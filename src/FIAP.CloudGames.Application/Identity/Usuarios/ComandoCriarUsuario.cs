using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.Application.Identity.Usuarios;

/// <summary>
/// Contém os dados recebidos pelo caso de uso de criação de usuário.
/// </summary>
public sealed class ComandoCriarUsuario
{
    public ComandoCriarUsuario(string nome, string email, string senha)
        : this(nome, email, senha, PerfisSistema.UsuarioId)
    {
    }

    public ComandoCriarUsuario(string nome, string email, string senha, Guid perfilId)
    {
        Nome = nome;
        Email = email;
        Senha = senha;
        PerfilId = perfilId;
    }

    public string Nome { get; }

    public string Email { get; }

    public string Senha { get; }

    public Guid PerfilId { get; }
}
