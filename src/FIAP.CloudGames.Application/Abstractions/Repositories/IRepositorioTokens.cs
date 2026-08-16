using FIAP.CloudGames.Domain.Entities;

namespace FIAP.CloudGames.Application.Abstractions.Repositories;

public interface IRepositorioTokens
{
    Task AdicionarAsync(Token token, CancellationToken tokenCancelamento = default);

    Task<Token?> ObterPorHashAsync(
        string tokenHash,
        CancellationToken tokenCancelamento = default);

    Task RevogarTokensAtivosDoUsuarioAsync(
        Guid usuarioId,
        DateTimeOffset dataRevogacao,
        CancellationToken tokenCancelamento = default);
}
