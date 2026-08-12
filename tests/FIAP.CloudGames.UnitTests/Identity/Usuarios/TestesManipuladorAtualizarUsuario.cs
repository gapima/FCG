using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Application.Identity.Usuarios;
using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.UnitTests.Identity.Usuarios;

public sealed class TestesManipuladorAtualizarUsuario
{
    private static readonly DateTimeOffset Agora = new(2026, 7, 18, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid PerfilId = Guid.Parse("4f642cbc-3720-4bb2-b456-15a97049da5c");

    [Fact]
    public async Task Processar_ComDadosValidos_AtualizaSomenteCamposPermitidos()
    {
        var usuario = CriarUsuario();
        var cpf = usuario.CPF;
        var senhaHash = usuario.SenhaHash;
        var repositorio = new RepositorioStub(usuario);

        var resultado = await CriarManipulador(repositorio).ProcessarAsync(CriarComando(usuario.Id), TestContext.Current.CancellationToken);

        Assert.Equal(StatusAtualizacaoUsuario.Atualizado, resultado.Status);
        Assert.True(repositorio.Atualizado);
        Assert.Equal("Novo Nome", usuario.Nome);
        Assert.Equal("novo@exemplo.com", usuario.Email);
        Assert.Equal(cpf, usuario.CPF);
        Assert.Equal(senhaHash, usuario.SenhaHash);
        Assert.Equal(usuario.Id, repositorio.IdIgnoradoNaConsultaEmail);
    }

    [Fact]
    public async Task Processar_ComUsuarioInexistente_RetornaNaoEncontrado()
    {
        var resultado = await CriarManipulador(new RepositorioStub(null))
            .ProcessarAsync(CriarComando(Guid.NewGuid()), TestContext.Current.CancellationToken);
        Assert.Equal(StatusAtualizacaoUsuario.NaoEncontrado, resultado.Status);
    }

    [Fact]
    public async Task Processar_ComEmailDeOutroUsuario_RetornaConflitoSemAlterarEstado()
    {
        var usuario = CriarUsuario();
        var repositorio = new RepositorioStub(usuario) { EmailExiste = true };
        var resultado = await CriarManipulador(repositorio).ProcessarAsync(CriarComando(usuario.Id), TestContext.Current.CancellationToken);

        Assert.Equal(StatusAtualizacaoUsuario.EmailJaCadastrado, resultado.Status);
        Assert.Equal("Nome Original", usuario.Nome);
        Assert.Equal("original@exemplo.com", usuario.Email);
        Assert.False(repositorio.Atualizado);
    }

    [Fact]
    public async Task Processar_ComPerfilInexistente_RetornaErroSemAlterarEstado()
    {
        var usuario = CriarUsuario();
        var repositorio = new RepositorioStub(usuario) { PerfilExiste = false };
        var resultado = await CriarManipulador(repositorio).ProcessarAsync(CriarComando(usuario.Id), TestContext.Current.CancellationToken);

        Assert.Equal(StatusAtualizacaoUsuario.PerfilNaoEncontrado, resultado.Status);
        Assert.Equal("Nome Original", usuario.Nome);
        Assert.False(repositorio.Atualizado);
    }

    [Fact]
    public async Task Processar_ComDadosInvalidos_NaoConsultaNemAlteraUsuario()
    {
        var usuario = CriarUsuario();
        var repositorio = new RepositorioStub(usuario);
        var comando = CriarComando(usuario.Id) with { Nome = "", Email = "invalido" };
        var resultado = await CriarManipulador(repositorio).ProcessarAsync(comando, TestContext.Current.CancellationToken);

        Assert.Equal(StatusAtualizacaoUsuario.DadosInvalidos, resultado.Status);
        Assert.Equal("Nome Original", usuario.Nome);
        Assert.False(repositorio.Consultado);
        Assert.False(repositorio.Atualizado);
    }

    private static Usuario CriarUsuario() => new(
        Guid.NewGuid(), "Nome Original", "12345678900", Agora.AddYears(-20),
        "original@exemplo.com", "hash-original", PerfilId, Agora.AddDays(-1));

    private static ComandoAtualizarUsuario CriarComando(Guid id) => new(
        id, "  Novo   Nome ", Agora.AddYears(-18), " NOVO@EXEMPLO.COM ", PerfilId);

    private static ManipuladorAtualizarUsuario CriarManipulador(RepositorioStub repositorio) =>
        new(repositorio, new RelogioFixo(Agora));

    private sealed class RepositorioStub(Usuario? usuario) : IRepositoryUsuarios
    {
        public bool EmailExiste { get; init; }
        public bool PerfilExiste { get; init; } = true;
        public bool Consultado { get; private set; }
        public bool Atualizado { get; private set; }
        public Guid? IdIgnoradoNaConsultaEmail { get; private set; }
        public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken token = default) { Consultado = true; return Task.FromResult(usuario); }
        public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken token = default) => Task.FromResult<Usuario?>(null);
        public Task<bool> ExisteEmailAsync(string email, Guid? ignorarId, CancellationToken token = default) { IdIgnoradoNaConsultaEmail = ignorarId; return Task.FromResult(EmailExiste); }
        public Task<bool> ExisteCpfAsync(string cpf, Guid? ignorarId, CancellationToken token = default) => Task.FromResult(false);
        public Task<bool> PerfilExisteAsync(Guid perfilId, CancellationToken token = default) => Task.FromResult(PerfilExiste);
        public Task<bool> TentarAdicionarAsync(Usuario item, CancellationToken token = default) => Task.FromResult(true);
        public Task AtualizarAsync(Usuario item, CancellationToken token = default) { Atualizado = true; return Task.CompletedTask; }
    }

    private sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora;
    }
}
