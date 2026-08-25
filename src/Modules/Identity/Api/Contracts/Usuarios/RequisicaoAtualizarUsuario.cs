namespace FIAP.CloudGames.Api.Contracts.Identity.Usuarios;

public sealed class RequisicaoAtualizarUsuario
{
    public string Nome { get; init; } = string.Empty;
    public DateTimeOffset DataNascimento { get; init; }
    public string Email { get; init; } = string.Empty;
}
