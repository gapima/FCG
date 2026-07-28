namespace FIAP.CloudGames.Application.Identity.Usuarios;

/// <summary>
/// Contém os dados recebidos pelo caso de uso de criação de usuário.
/// </summary>
public sealed class ComandoCriarUsuario
{
    public ComandoCriarUsuario(string nome, string email)
    {
        Nome = nome;
        Email = email;
    }

    public string Nome { get; }

    public string Email { get; }
}
