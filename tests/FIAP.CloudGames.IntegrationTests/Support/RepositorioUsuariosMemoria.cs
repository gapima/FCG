using System.Collections.Concurrent;
using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.IntegrationTests.Support;

internal sealed class RepositorioUsuariosMemoria : IRepositoryUsuarios
{
    private readonly ConcurrentDictionary<string, Usuario> _usuarios =
        new(StringComparer.Ordinal);

    public Task<bool> TentarAdicionarAsync(
        Usuario usuario,
        CancellationToken tokenCancelamento = default)
    {
        tokenCancelamento.ThrowIfCancellationRequested();

        return Task.FromResult(_usuarios.TryAdd(usuario.Email, usuario));
    }
}
