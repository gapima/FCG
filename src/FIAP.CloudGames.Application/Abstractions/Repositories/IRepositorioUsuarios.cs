using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.Application.Abstractions.Repositories;

public interface IRepositoryUsuarios
{
    Task<UsuarioAutenticacao?> ObterPorEmailAsync(
        string email,
        CancellationToken tokenCancelamento = default);

    /// <summary>
    /// Persiste o usuário e retorna false quando o e-mail já estiver cadastrado.
    /// </summary>
    Task<bool> TentarAdicionarAsync(
        Usuario usuario,
        CancellationToken tokenCancelamento = default);
}

public sealed record UsuarioAutenticacao(Usuario Usuario, string Perfil);
