namespace FIAP.CloudGames.Domain.AccessControl.Entities;

public class Autorizacao
{
    public Guid Id { get; set; }

    public Guid UsuarioId { get; set; }

    public Guid JogoId { get; set; }

    public string Nome { get; set; } = string.Empty;
}
