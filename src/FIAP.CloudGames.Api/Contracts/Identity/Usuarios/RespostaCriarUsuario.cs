namespace FIAP.CloudGames.Api.Contracts.Identity.Usuarios;

public sealed record RespostaCriarUsuario(
    Guid Id,
    string Nome,
    string Email,
    DateTimeOffset DataCriacao);
