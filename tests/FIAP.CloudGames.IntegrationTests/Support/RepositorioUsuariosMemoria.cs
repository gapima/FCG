using System.Collections.Concurrent;
using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.IntegrationTests.Support;

internal sealed class RepositorioUsuariosMemoria : IRepositoryUsuarios
{
    private readonly ConcurrentDictionary<string, Usuario> _usuarios =
        new(StringComparer.Ordinal);

    public Task<UsuarioAutenticacao?> ObterPorEmailAsync(
        string email,
        CancellationToken tokenCancelamento = default)
    {
        tokenCancelamento.ThrowIfCancellationRequested();

        return Task.FromResult(
            _usuarios.TryGetValue(email, out var usuario)
                ? new UsuarioAutenticacao(usuario, PerfisSistema.ObterNome(usuario.PerfilId))
                : null);
    }

    public Task<bool> TentarAdicionarAsync(
        Usuario usuario,
        CancellationToken tokenCancelamento = default)
    {
        tokenCancelamento.ThrowIfCancellationRequested();

        return Task.FromResult(_usuarios.TryAdd(usuario.Email, usuario));
    }

    public bool TentarInativar(string email, DateTimeOffset dataInativacao)
    {
        if (!_usuarios.TryGetValue(email, out var usuario))
            return false;

        usuario.Inativar(dataInativacao);
        return true;
    }
}
