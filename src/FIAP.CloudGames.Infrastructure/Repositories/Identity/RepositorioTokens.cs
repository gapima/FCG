using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Infrastructure.Repositories.Identity;

internal sealed class RepositorioTokens : IRepositorioTokens
{
    private readonly PostgresqlDbContext _contexto;

    public RepositorioTokens(PostgresqlDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task AdicionarAsync(
        Token token,
        CancellationToken tokenCancelamento = default)
    {
        _contexto.Tokens.Add(token);
        await _contexto.SaveChangesAsync(tokenCancelamento);
    }

    public Task<Token?> ObterPorHashAsync(
        string tokenHash,
        CancellationToken tokenCancelamento = default) =>
        _contexto.Tokens
            .AsNoTracking()
            .SingleOrDefaultAsync(token => token.TokenHash == tokenHash, tokenCancelamento);

    public async Task RevogarTokensAtivosDoUsuarioAsync(
        Guid usuarioId,
        DateTimeOffset dataRevogacao,
        CancellationToken tokenCancelamento = default)
    {
        var tokens = await _contexto.Tokens
            .Where(token => token.UsuarioId == usuarioId
                && token.DataRevogacao == null
                && token.DataExpiracao > dataRevogacao)
            .ToListAsync(tokenCancelamento);

        foreach (var token in tokens)
            token.Revogar(dataRevogacao);

        await _contexto.SaveChangesAsync(tokenCancelamento);
    }
}
