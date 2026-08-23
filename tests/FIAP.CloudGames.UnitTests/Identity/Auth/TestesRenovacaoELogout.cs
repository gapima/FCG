using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Application.Abstractions.Security;
using FIAP.CloudGames.Application.Identity.Auth;
using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.UnitTests.Identity.Auth;

public sealed class TestesRenovacaoELogout
{
    private static readonly DateTimeOffset Agora =
        new(2026, 8, 16, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Renovar_ComTokenAtivo_RotacionaRefreshTokenEGeraNovoJwt()
    {
        var usuario = CriarUsuario();
        var tokenAtual = CriarToken(usuario.Id);
        var repositorioTokens = new RepositorioTokensStub(tokenAtual);
        var manipulador = CriarManipulador(usuario, repositorioTokens);

        var resultado = await manipulador.ProcessarAsync(
            new ComandoRenovarToken("refresh-atual"),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusRenovacaoToken.Sucesso, resultado.Status);
        Assert.Equal("novo-access-token", resultado.Login!.AccessToken);
        Assert.Equal("novo-refresh-token", resultado.Login.RefreshToken);
        Assert.Equal("HASH_ATUAL", repositorioTokens.HashRotacionado);
        Assert.NotNull(repositorioTokens.NovoToken);
        Assert.Equal(usuario.Id, repositorioTokens.NovoToken.UsuarioId);
    }

    [Fact]
    public async Task Renovar_ComTokenReutilizado_RetornaNaoAutorizadoLogico()
    {
        var usuario = CriarUsuario();
        var repositorioTokens = new RepositorioTokensStub(CriarToken(usuario.Id))
        {
            DeveRotacionar = false
        };
        var manipulador = CriarManipulador(usuario, repositorioTokens);

        var resultado = await manipulador.ProcessarAsync(
            new ComandoRenovarToken("refresh-atual"),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusRenovacaoToken.TokenInvalido, resultado.Status);
        Assert.Null(resultado.Login);
    }

    [Fact]
    public async Task Renovar_SemToken_RetornaValidacaoSemConsultarRepositorio()
    {
        var repositorioTokens = new RepositorioTokensStub(null);
        var manipulador = CriarManipulador(null, repositorioTokens);

        var resultado = await manipulador.ProcessarAsync(
            new ComandoRenovarToken(""),
            TestContext.Current.CancellationToken);

        Assert.Equal(StatusRenovacaoToken.DadosInvalidos, resultado.Status);
        Assert.Contains("refreshToken", resultado.Erros);
        Assert.False(repositorioTokens.Consultado);
    }

    [Fact]
    public async Task Logout_RevogaTokensAtivosDoUsuarioNaDataAtual()
    {
        var repositorioTokens = new RepositorioTokensStub(null);
        var manipulador = new ManipuladorLogout(repositorioTokens, new RelogioFixo(Agora));
        var usuarioId = Guid.NewGuid();

        await manipulador.ProcessarAsync(usuarioId, TestContext.Current.CancellationToken);

        Assert.Equal(usuarioId, repositorioTokens.UsuarioRevogadoId);
        Assert.Equal(Agora, repositorioTokens.DataRevogacao);
    }

    private static ManipuladorRenovarToken CriarManipulador(
        Usuario? usuario,
        RepositorioTokensStub repositorioTokens) =>
        new(
            new RepositorioUsuariosStub(usuario),
            repositorioTokens,
            new ServicoTokenJwtStub(),
            new ServicoRefreshTokenStub(),
            new RelogioFixo(Agora));

    private static Usuario CriarUsuario() => new(
        Guid.NewGuid(),
        "Usuário",
        "12345678900",
        Agora.AddYears(-20),
        "usuario@exemplo.com",
        "hash",
        PerfisSistema.UsuarioId,
        Agora.AddDays(-1));

    private static Token CriarToken(Guid usuarioId) => new(
        Guid.NewGuid(),
        usuarioId,
        "HASH_ATUAL",
        Agora.AddDays(-1),
        Agora.AddDays(1));

    private sealed class RepositorioUsuariosStub(Usuario? usuario) : IRepositoryUsuarios
    {
        public Task<UsuarioAutenticacao?> ObterAutenticacaoPorIdAsync(
            Guid id,
            CancellationToken token = default) =>
            Task.FromResult(
                usuario is null
                    ? null
                    : new UsuarioAutenticacao(usuario, PerfisSistema.Usuario));

        public Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken token = default) => Task.FromResult<Usuario?>(null);
        public Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken token = default) => Task.FromResult<Usuario?>(null);
        public Task<UsuarioAutenticacao?> ObterAutenticacaoPorEmailAsync(string email, CancellationToken token = default) => Task.FromResult<UsuarioAutenticacao?>(null);
        public Task<bool> ExisteEmailAsync(string email, Guid? ignorarId, CancellationToken token = default) => Task.FromResult(false);
        public Task<bool> ExisteCpfAsync(string cpf, Guid? ignorarId, CancellationToken token = default) => Task.FromResult(false);
        public Task<bool> PerfilExisteAsync(Guid perfilId, CancellationToken token = default) => Task.FromResult(true);
        public Task<bool> TentarAdicionarAsync(Usuario item, CancellationToken token = default) => Task.FromResult(true);
        public Task AtualizarAsync(Usuario item, CancellationToken token = default) => Task.CompletedTask;
    }

    private sealed class RepositorioTokensStub(Token? token) : IRepositorioTokens
    {
        public bool DeveRotacionar { get; init; } = true;
        public bool Consultado { get; private set; }
        public string? HashRotacionado { get; private set; }
        public Token? NovoToken { get; private set; }
        public Guid? UsuarioRevogadoId { get; private set; }
        public DateTimeOffset? DataRevogacao { get; private set; }

        public Task<Token?> ObterPorHashAsync(string tokenHash, CancellationToken tokenCancelamento = default)
        {
            Consultado = true;
            return Task.FromResult(token);
        }

        public Task<bool> TentarRotacionarAsync(
            string tokenHashAtual,
            Token novoToken,
            DateTimeOffset dataRevogacao,
            CancellationToken tokenCancelamento = default)
        {
            HashRotacionado = tokenHashAtual;
            NovoToken = novoToken;
            return Task.FromResult(DeveRotacionar);
        }

        public Task RevogarTokensAtivosDoUsuarioAsync(
            Guid usuarioId,
            DateTimeOffset dataRevogacao,
            CancellationToken tokenCancelamento = default)
        {
            UsuarioRevogadoId = usuarioId;
            DataRevogacao = dataRevogacao;
            return Task.CompletedTask;
        }

        public Task AdicionarAsync(Token novoToken, CancellationToken tokenCancelamento = default) =>
            Task.CompletedTask;
    }

    private sealed class ServicoTokenJwtStub : IServicoTokenJwt
    {
        public TokenJwtGerado GerarToken(Usuario usuario, string perfil) =>
            new("novo-access-token", "Bearer", 1200, Agora.AddMinutes(20));
    }

    private sealed class ServicoRefreshTokenStub : IServicoRefreshToken
    {
        public RefreshTokenGerado GerarToken() =>
            new("novo-refresh-token", "NOVO_HASH", Agora, Agora.AddDays(7));

        public string CalcularHash(string token) => "HASH_ATUAL";
    }

    private sealed class RelogioFixo(DateTimeOffset agora) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => agora;
    }
}
