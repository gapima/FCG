using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.Application.Abstractions.Repositories;

/// <summary>
/// Define as operações de persistência necessárias pelos casos de uso de usuários.
/// </summary>
public interface IRepositoryUsuarios
{
    /// <summary>
    /// Persiste o usuário e retorna false quando o e-mail já estiver cadastrado.
    /// </summary>
    Task<bool> TentarAdicionarAsync(
        Usuario usuario,
        CancellationToken tokenCancelamento = default);
}
