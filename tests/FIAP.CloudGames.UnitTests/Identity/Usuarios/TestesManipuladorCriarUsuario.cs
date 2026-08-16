using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Application.Abstractions.Security;
using FIAP.CloudGames.Application.Identity.Usuarios;
using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.UnitTests.Identity.Usuarios;

public sealed class TestesManipuladorCriarUsuario
{
    private static readonly DateTimeOffset Agora =
        new(2026, 7, 18, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid PerfilId = PerfisSistema.UsuarioId;

    [Fact]
    public async Task Processar_ComDadosValidos_NormalizaDadosAntesDePersistir()
    {
        var repositorio = new RepositorioUsuariosStub();
        var manipulador = CriarManipulador(repositorio);

        var resultado = await manipulador.ProcessarAsync(
            new ComandoCriarUsuario(
                "  Maria   da Silva  ",
                "  MARIA@EXEMPLO.COM  ",
                "Senha@123",
                PerfilId),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCriacaoUsuario.Criado, resultado.Status);
        Assert.NotNull(resultado.Usuario);
        Assert.Equal("Maria da Silva", resultado.Usuario.Nome);
        Assert.Equal("maria@exemplo.com", resultado.Usuario.Email);
        Assert.Equal(Agora, resultado.Usuario.CriadoEmUtc);
        Assert.NotNull(repositorio.UsuarioAdicionado);
        Assert.Equal("hash::Senha@123", repositorio.UsuarioAdicionado.SenhaHash);
        Assert.Equal(PerfisSistema.UsuarioId, repositorio.UsuarioAdicionado.PerfilId);
    }

    [Fact]
    public async Task Processar_ComDadosInvalidos_NaoPersisteUsuario()
    {
        var repositorio = new RepositorioUsuariosStub();
        var manipulador = CriarManipulador(repositorio);

        var resultado = await manipulador.ProcessarAsync(
            new ComandoCriarUsuario("A", "email-invalido", "fraca"),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCriacaoUsuario.DadosInvalidos, resultado.Status);
        Assert.Null(repositorio.UsuarioAdicionado);
        Assert.Equal(3, resultado.Erros.Count);
    }

    [Fact]
    public async Task Processar_ComPerfilVazio_NaoPersisteUsuario()
    {
        var repositorio = new RepositorioUsuariosStub();
        var manipulador = CriarManipulador(repositorio);

        var resultado = await manipulador.ProcessarAsync(
            new ComandoCriarUsuario(
                "Maria da Silva",
                "maria@exemplo.com",
                "Senha@123",
                Guid.Empty),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCriacaoUsuario.DadosInvalidos, resultado.Status);
        Assert.Null(repositorio.UsuarioAdicionado);
        Assert.Contains("perfilId", resultado.Erros);
    }

    [Fact]
    public async Task Processar_ComEmailJaCadastrado_RetornaConflitoEsperado()
    {
        var repositorio = new RepositorioUsuariosStub { DeveAdicionar = false };
        var manipulador = CriarManipulador(repositorio);

        var resultado = await manipulador.ProcessarAsync(
            new ComandoCriarUsuario(
                "Usuário Existente",
                "existente@exemplo.com",
                "Senha@123",
                PerfilId),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCriacaoUsuario.EmailJaCadastrado, resultado.Status);
        Assert.Null(resultado.Usuario);
    }

    private static ManipuladorCriarUsuario CriarManipulador(
        RepositorioUsuariosStub repositorio) =>
        new(repositorio, new ServicoHashSenhaStub(), new RelogioFixo(Agora));

    private sealed class RepositorioUsuariosStub : IRepositoryUsuarios
    {
        public bool DeveAdicionar { get; init; } = true;

        public Usuario? UsuarioAdicionado { get; private set; }

        public Task<UsuarioAutenticacao?> ObterPorEmailAsync(
            string email,
            CancellationToken tokenCancelamento = default) =>
            Task.FromResult<UsuarioAutenticacao?>(null);

        public Task<bool> TentarAdicionarAsync(
            Usuario usuario,
            CancellationToken tokenCancelamento = default)
        {
            tokenCancelamento.ThrowIfCancellationRequested();
            UsuarioAdicionado = usuario;

            return Task.FromResult(DeveAdicionar);
        }
    }

    private sealed class ServicoHashSenhaStub : IServicoHashSenha
    {
        public string GerarHash(string senha) => $"hash::{senha}";

        public bool Verificar(string senha, string senhaHash) =>
            senhaHash == GerarHash(senha);
    }

    private sealed class RelogioFixo : TimeProvider
    {
        private readonly DateTimeOffset _agora;

        public RelogioFixo(DateTimeOffset agora)
        {
            _agora = agora;
        }

        public override DateTimeOffset GetUtcNow() => _agora;
    }
}
