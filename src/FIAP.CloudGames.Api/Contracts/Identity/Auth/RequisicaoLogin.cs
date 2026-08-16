namespace FIAP.CloudGames.Api.Contracts.Identity.Auth;

public sealed class RequisicaoLogin
{
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}
