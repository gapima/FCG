namespace FIAP.CloudGames.Application.Identity.Usuarios;

public sealed record ComandoAtualizarUsuario(
    Guid Id,
    string Nome,
    DateTimeOffset DataNascimento,
    string Email,
    Guid PerfilId);
