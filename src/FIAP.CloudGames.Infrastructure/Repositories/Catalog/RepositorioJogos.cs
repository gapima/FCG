using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Domain.Catalog.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Infrastructure.Repositories.Catalog;

internal sealed class RepositorioJogos : IRepositorioJogos
{
    private readonly PostgresqlDbContext _contexto;

    public RepositorioJogos(PostgresqlDbContext contexto)
    {
        _contexto = contexto;
    }

    public Task<Jogo?> ObterPorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _contexto.Jogos.FirstOrDefaultAsync(jogo => jogo.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Jogo>> ListarAsync(
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken = default)
    {
        var jogos = await _contexto.Jogos
            .AsNoTracking()
            .OrderBy(jogo => jogo.Titulo)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);

        return jogos;
    }

    public async Task AdicionarAsync(
        Jogo jogo,
        CancellationToken cancellationToken = default)
    {
        _contexto.Jogos.Add(jogo);
        await _contexto.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(
        Jogo jogo,
        CancellationToken cancellationToken = default)
    {
        if (_contexto.Entry(jogo).State == EntityState.Detached)
        {
            _contexto.Jogos.Update(jogo);
        }

        await _contexto.SaveChangesAsync(cancellationToken);
    }
}
