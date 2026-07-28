using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Domain.Identity.Entities;
using FIAP.CloudGames.Infrastructure.Data.EF.Context;
using FIAP.CloudGames.Infrastructure.Data.EF.Mappings.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FIAP.CloudGames.Infrastructure.Repositories.Identity;

internal sealed class RepositorioUsuarios : IRepositoryUsuarios
{
    private readonly PostgresqlDbContext _contexto;

    public RepositorioUsuarios(PostgresqlDbContext contexto)
    {
        _contexto = contexto;
    }

    public async Task<bool> TentarAdicionarAsync(
        Usuario usuario,
        CancellationToken tokenCancelamento = default)
    {
        var emailJaCadastrado = await _contexto.Usuarios
            .AsNoTracking()
            .AnyAsync(
                usuarioExistente => usuarioExistente.Email == usuario.Email,
                tokenCancelamento);

        if (emailJaCadastrado)
        {
            return false;
        }

        _contexto.Usuarios.Add(usuario);

        try
        {
            await _contexto.SaveChangesAsync(tokenCancelamento);
            return true;
        }
        catch (DbUpdateException excecao) when (EhViolacaoEmailUnico(excecao))
        {
            _contexto.Entry(usuario).State = EntityState.Detached;
            return false;
        }
    }

    private static bool EhViolacaoEmailUnico(DbUpdateException excecao) =>
        excecao.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: MapeamentoUsuario.NomeIndiceEmailUnico
        };
}
