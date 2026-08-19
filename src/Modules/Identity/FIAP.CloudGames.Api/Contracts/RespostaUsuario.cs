namespace FIAP.CloudGames.Api.Contracts.Identity.Usuarios;

public sealed record RespostaUsuario(
    Guid Id,
    string Nome,
    string Email,
    Guid PerfilId,
    bool Ativo,
    DateTimeOffset CriadoEmUtc,
    DateTimeOffset? DataInativacao);
