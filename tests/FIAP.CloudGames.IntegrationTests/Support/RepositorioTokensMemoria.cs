using System.Collections.Concurrent;
using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Domain.Entities;

namespace FIAP.CloudGames.IntegrationTests.Support;

internal sealed class RepositorioTokensMemoria : IRepositorioTokens
{
    private readonly ConcurrentDictionary<string, Token> _tokens = new(StringComparer.Ordinal);
    private readonly object _sincronizacao = new();

    public Task AdicionarAsync(
        Token token,
        CancellationToken tokenCancelamento = default)
    {
        tokenCancelamento.ThrowIfCancellationRequested();

        if (!_tokens.TryAdd(token.TokenHash, token))
            throw new InvalidOperationException("Hash de refresh token duplicado.");

        return Task.CompletedTask;
    }

    public Task<Token?> ObterPorHashAsync(
        string tokenHash,
        CancellationToken tokenCancelamento = default)
    {
        tokenCancelamento.ThrowIfCancellationRequested();
        _tokens.TryGetValue(tokenHash, out var token);
        return Task.FromResult(token);
    }

    public Task<bool> TentarRotacionarAsync(
        string tokenHashAtual,
        Token novoToken,
        DateTimeOffset dataRevogacao,
        CancellationToken tokenCancelamento = default)
    {
        tokenCancelamento.ThrowIfCancellationRequested();

        lock (_sincronizacao)
        {
            if (!_tokens.TryGetValue(tokenHashAtual, out var tokenAtual)
                || !tokenAtual.EstaAtivo(dataRevogacao)
                || _tokens.ContainsKey(novoToken.TokenHash))
            {
                return Task.FromResult(false);
            }

            tokenAtual.Revogar(dataRevogacao);
            _tokens[novoToken.TokenHash] = novoToken;
            return Task.FromResult(true);
        }
    }

    public Task RevogarTokensAtivosDoUsuarioAsync(
        Guid usuarioId,
        DateTimeOffset dataRevogacao,
        CancellationToken tokenCancelamento = default)
    {
        tokenCancelamento.ThrowIfCancellationRequested();

        foreach (var token in _tokens.Values.Where(token => token.UsuarioId == usuarioId))
        {
            if (token.EstaAtivo(dataRevogacao))
                token.Revogar(dataRevogacao);
        }

        return Task.CompletedTask;
    }
}
