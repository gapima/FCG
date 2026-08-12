using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Application.Identity.Usuarios;
using FIAP.CloudGames.Application.Abstractions.Security;
using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.UnitTests.Identity.Usuarios;

public sealed class TestesManipuladorCriarUsuario
{
    private static readonly DateTimeOffset Agora =
        new(2026, 7, 18, 12, 0, 0, TimeSpan.Zero);
    private static readonly Guid PerfilId = Guid.Parse("4f642cbc-3720-4bb2-b456-15a97049da5c");

    [Fact]
    public async Task Processar_ComDadosValidos_NormalizaDadosAntesDePersistir()
    {
        var repositorio = new RepositorioUsuariosStub();
        var manipulador = CriarManipulador(repositorio);

        var resultado = await manipulador.ProcessarAsync(
            new ComandoCriarUsuario(
                "  Maria   da Silva  ",
                "123.456.789-00",
                new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
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
    }

    [Fact]
    public async Task Processar_ComDadosInvalidos_NaoPersisteUsuario()
    {
        var repositorio = new RepositorioUsuariosStub();
        var manipulador = CriarManipulador(repositorio);

        var resultado = await manipulador.ProcessarAsync(
            new ComandoCriarUsuario("A", "", Agora.AddDays(1), "email-invalido", "fraca", PerfilId),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCriacaoUsuario.DadosInvalidos, resultado.Status);
        Assert.Null(repositorio.UsuarioAdicionado);
        Assert.Equal(5, resultado.Erros.Count);
    }

    [Fact]
    public async Task Processar_ComPerfilVazio_NaoPersisteUsuario()
    {
        var repositorio = new RepositorioUsuariosStub();
        var manipulador = CriarManipulador(repositorio);

        var resultado = await manipulador.ProcessarAsync(
            new ComandoCriarUsuario("Maria da Silva", "12345678900", Agora.AddYears(-20), "maria@exemplo.com", "Senha@123", Guid.Empty),
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
                "12345678900",
                Agora.AddYears(-20),
                "existente@exemplo.com",
                "Senha@123",
                PerfilId),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusCriacaoUsuario.EmailJaCadastrado, resultado.Status);
        Assert.Null(resultado.Usuario);
    }

    private static ManipuladorCriarUsuario CriarManipulador(
        RepositorioUsuariosStub repositorio) =>
        new(repositorio, new HashSenhaStub(), new RelogioFixo(Agora));

    private sealed class RepositorioUsuariosStub : IRepositoryUsuarios
    {
        public bool DeveAdicionar { get; init; } = true;

        public Usuario? UsuarioAdicionado { get; private set; }

        public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken tokenCancelamento = default) => Task.FromResult<Usuario?>(null);
        public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken tokenCancelamento = default) => Task.FromResult<Usuario?>(null);
        public Task<bool> ExisteEmailAsync(string email, Guid? ignorarUsuarioId, CancellationToken tokenCancelamento = default) => Task.FromResult(false);
        public Task<bool> ExisteCpfAsync(string cpf, Guid? ignorarUsuarioId, CancellationToken tokenCancelamento = default) => Task.FromResult(false);
        public Task<bool> PerfilExisteAsync(Guid perfilId, CancellationToken tokenCancelamento = default) => Task.FromResult(true);

        public Task<bool> TentarAdicionarAsync(
            Usuario usuario,
            CancellationToken tokenCancelamento = default)
        {
            tokenCancelamento.ThrowIfCancellationRequested();
            UsuarioAdicionado = usuario;

            return Task.FromResult(DeveAdicionar);
        }

        public Task AtualizarAsync(Usuario usuario, CancellationToken tokenCancelamento = default) => Task.CompletedTask;
    }

    private sealed class HashSenhaStub : IHashSenha
    {
        public string Criar(string senha) => $"hash:{senha}";
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
