namespace FIAP.CloudGames.Application.Identity.Usuarios;

/// <summary>
/// Contém os dados recebidos pelo caso de uso de criação de usuário.
/// </summary>
public sealed class ComandoCriarUsuario
{
    public ComandoCriarUsuario(string nome, string email, Guid perfilId)
    {
        Nome = nome;
        Email = email;
        PerfilId = perfilId;
    }

    public string Nome { get; }

    public string Email { get; }

    public Guid PerfilId { get; }
}
