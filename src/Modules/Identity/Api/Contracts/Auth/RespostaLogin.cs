namespace FIAP.CloudGames.Api.Contracts.Identity.Auth;

public sealed record RespostaUsuarioLogado(
    Guid Id,
    string Nome,
    string Email,
    Guid PerfilId,
    string Perfil);

public sealed record RespostaLogin(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    long ExpiresIn,
    DateTimeOffset ExpiresAt,
    RespostaUsuarioLogado Usuario);
