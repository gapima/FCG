using System.Collections.Concurrent;
using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.IntegrationTests.Support;

internal sealed class RepositorioUsuariosMemoria : IRepositoryUsuarios
{
    internal static readonly Guid PerfilId = Guid.Parse("4f642cbc-3720-4bb2-b456-15a97049da5c");
    private readonly ConcurrentDictionary<Guid, Usuario> _usuarios = new();

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken tokenCancelamento = default)
    {
        tokenCancelamento.ThrowIfCancellationRequested();
        _usuarios.TryGetValue(id, out var usuario);
        return Task.FromResult(usuario);
    }

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken tokenCancelamento = default) =>
        Task.FromResult(_usuarios.Values.SingleOrDefault(usuario => usuario.Email == email));

    public Task<bool> ExisteEmailAsync(string email, Guid? ignorarUsuarioId, CancellationToken tokenCancelamento = default) =>
        Task.FromResult(_usuarios.Values.Any(usuario => usuario.Email == email && usuario.Id != ignorarUsuarioId));

    public Task<bool> ExisteCpfAsync(string cpf, Guid? ignorarUsuarioId, CancellationToken tokenCancelamento = default) =>
        Task.FromResult(_usuarios.Values.Any(usuario => usuario.CPF == cpf && usuario.Id != ignorarUsuarioId));

    public Task<bool> PerfilExisteAsync(Guid perfilId, CancellationToken tokenCancelamento = default) =>
        Task.FromResult(perfilId == PerfilId);

    public Task<bool> TentarAdicionarAsync(Usuario usuario, CancellationToken tokenCancelamento = default)
    {
        tokenCancelamento.ThrowIfCancellationRequested();
        if (_usuarios.Values.Any(existente => existente.Email == usuario.Email))
            return Task.FromResult(false);
        return Task.FromResult(_usuarios.TryAdd(usuario.Id, usuario));
    }

    public Task AtualizarAsync(Usuario usuario, CancellationToken tokenCancelamento = default)
    {
        tokenCancelamento.ThrowIfCancellationRequested();
        _usuarios[usuario.Id] = usuario;
        return Task.CompletedTask;
    }
}
