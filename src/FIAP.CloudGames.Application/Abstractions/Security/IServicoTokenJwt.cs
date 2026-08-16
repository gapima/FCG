using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.Application.Abstractions.Security;

public interface IServicoTokenJwt
{
    TokenJwtGerado GerarToken(Usuario usuario, string perfil);
}

public sealed record TokenJwtGerado(
    string AccessToken,
    string TokenType,
    long ExpiresIn,
    DateTimeOffset ExpiresAt);
