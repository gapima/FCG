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

    public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken tokenCancelamento = default) =>
        _contexto.Usuarios.AsNoTracking().SingleOrDefaultAsync(
            usuario => usuario.Id == id,
            tokenCancelamento);

    public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken tokenCancelamento = default) =>
        _contexto.Usuarios.AsNoTracking().SingleOrDefaultAsync(
            usuario => usuario.Email == email,
            tokenCancelamento);

    public Task<UsuarioAutenticacao?> ObterAutenticacaoPorEmailAsync(
        string email,
        CancellationToken tokenCancelamento = default) =>
        (from usuario in _contexto.Usuarios.AsNoTracking()
         join perfil in _contexto.Perfis.AsNoTracking()
             on usuario.PerfilId equals perfil.Id
         where usuario.Email == email
         select new UsuarioAutenticacao(usuario, perfil.Nome))
        .SingleOrDefaultAsync(tokenCancelamento);

    public Task<bool> ExisteEmailAsync(
        string email,
        Guid? ignorarUsuarioId,
        CancellationToken tokenCancelamento = default) =>
        _contexto.Usuarios.AsNoTracking().AnyAsync(
            usuario => usuario.Email == email
                && (!ignorarUsuarioId.HasValue || usuario.Id != ignorarUsuarioId.Value),
            tokenCancelamento);

    public Task<bool> ExisteCpfAsync(
        string cpf,
        Guid? ignorarUsuarioId,
        CancellationToken tokenCancelamento = default) =>
        _contexto.Usuarios.AsNoTracking().AnyAsync(
            usuario => usuario.CPF == cpf
                && (!ignorarUsuarioId.HasValue || usuario.Id != ignorarUsuarioId.Value),
            tokenCancelamento);

    public Task<bool> PerfilExisteAsync(Guid perfilId, CancellationToken tokenCancelamento = default) =>
        _contexto.Perfis.AsNoTracking().AnyAsync(perfil => perfil.Id == perfilId, tokenCancelamento);

    public async Task<bool> TentarAdicionarAsync(
        Usuario usuario,
        CancellationToken tokenCancelamento = default)
    {
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

    public async Task AtualizarAsync(Usuario usuario, CancellationToken tokenCancelamento = default)
    {
        _contexto.Usuarios.Update(usuario);
        await _contexto.SaveChangesAsync(tokenCancelamento);
    }

    private static bool EhViolacaoEmailUnico(DbUpdateException excecao) =>
        excecao.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: MapeamentoUsuario.NomeIndiceEmailUnico
        };
}
