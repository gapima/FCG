using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Application.Identity.Usuarios;
using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.UnitTests.Identity.Usuarios;

public sealed class TestesManipuladorObterUsuario
{
    [Fact]
    public async Task Processar_ComUsuarioExistente_RetornaDadosNaoSensiveis()
    {
        var usuario = CriarUsuario();
        var resultado = await new ManipuladorObterUsuario(new RepositorioStub(usuario))
            .ProcessarAsync(new ConsultaObterUsuario(usuario.Id), TestContext.Current.CancellationToken);

        Assert.Equal(StatusObtencaoUsuario.Encontrado, resultado.Status);
        Assert.Equal(usuario.Email, resultado.Usuario!.Email);
    }

    [Fact]
    public async Task Processar_ComUsuarioInexistente_RetornaNaoEncontrado()
    {
        var resultado = await new ManipuladorObterUsuario(new RepositorioStub(null))
            .ProcessarAsync(new ConsultaObterUsuario(Guid.NewGuid()), TestContext.Current.CancellationToken);
        Assert.Equal(StatusObtencaoUsuario.NaoEncontrado, resultado.Status);
    }

    [Fact]
    public async Task Processar_ComIdVazio_RetornaIdInvalidoSemConsultarRepositorio()
    {
        var repositorio = new RepositorioStub(null);
        var resultado = await new ManipuladorObterUsuario(repositorio)
            .ProcessarAsync(new ConsultaObterUsuario(Guid.Empty), TestContext.Current.CancellationToken);
        Assert.Equal(StatusObtencaoUsuario.IdInvalido, resultado.Status);
        Assert.False(repositorio.Consultado);
    }

    private static Usuario CriarUsuario() => new(
        Guid.NewGuid(), "Maria", "12345678900", DateTimeOffset.UtcNow.AddYears(-20),
        "maria@exemplo.com", "hash", Guid.NewGuid(), DateTimeOffset.UtcNow);

    private sealed class RepositorioStub(Usuario? usuario) : IRepositoryUsuarios
    {
        public bool Consultado { get; private set; }
        public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken token = default) { Consultado = true; return Task.FromResult(usuario); }
        public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken token = default) => Task.FromResult<Usuario?>(null);
        public Task<bool> ExisteEmailAsync(string email, Guid? ignorarId, CancellationToken token = default) => Task.FromResult(false);
        public Task<bool> ExisteCpfAsync(string cpf, Guid? ignorarId, CancellationToken token = default) => Task.FromResult(false);
        public Task<bool> PerfilExisteAsync(Guid perfilId, CancellationToken token = default) => Task.FromResult(true);
        public Task<bool> TentarAdicionarAsync(Usuario item, CancellationToken token = default) => Task.FromResult(true);
        public Task AtualizarAsync(Usuario item, CancellationToken token = default) => Task.CompletedTask;
    }
}
