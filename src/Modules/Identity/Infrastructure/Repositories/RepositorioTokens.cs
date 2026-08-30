using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Infrastructure.Repositories.Identity;

internal sealed class RepositorioTokens : IRepositorioTokens
{
    private readonly IdentityDbContext _contexto;

    public RepositorioTokens(IdentityDbContext contexto)
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

    public async Task<bool> TentarRotacionarAsync(
        string tokenHashAtual,
        Token novoToken,
        DateTimeOffset dataRevogacao,
        CancellationToken tokenCancelamento = default)
    {
        var estrategia = _contexto.Database.CreateExecutionStrategy();
        return await estrategia.ExecuteAsync(async () =>
        {
            await using var transacao = await _contexto.Database.BeginTransactionAsync(tokenCancelamento);

            var quantidadeAtualizada = await _contexto.Tokens
                .Where(token => token.TokenHash == tokenHashAtual
                    && token.DataRevogacao == null
                    && token.DataExpiracao > dataRevogacao)
                .ExecuteUpdateAsync(
                    atualizacao => atualizacao.SetProperty(
                        token => token.DataRevogacao,
                        dataRevogacao),
                    tokenCancelamento);

            if (quantidadeAtualizada != 1)
            {
                await transacao.RollbackAsync(tokenCancelamento);
                return false;
            }

            try
            {
                _contexto.Tokens.Add(novoToken);
                await _contexto.SaveChangesAsync(tokenCancelamento);
                await transacao.CommitAsync(tokenCancelamento);
                return true;
            }
            catch
            {
                _contexto.Entry(novoToken).State = EntityState.Detached;
                throw;
            }
        });
    }

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
