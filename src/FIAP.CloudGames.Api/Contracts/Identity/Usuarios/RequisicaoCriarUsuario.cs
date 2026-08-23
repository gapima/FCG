namespace FIAP.CloudGames.Api.Contracts.Identity.Usuarios;

public sealed class RequisicaoCriarUsuario
{
    public string Nome { get; init; } = string.Empty;
    public string CPF { get; init; } = string.Empty;
    public DateTimeOffset DataNascimento { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Senha { get; init; } = string.Empty;
}
