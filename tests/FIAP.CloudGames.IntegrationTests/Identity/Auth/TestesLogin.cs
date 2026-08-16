using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.IntegrationTests.Support;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.IntegrationTests.Identity.Auth;

public sealed class TestesLogin : IClassFixture<FabricaApiCloudGames>
{
    private static readonly JsonSerializerOptions OpcoesJson =
        new(JsonSerializerDefaults.Web);

    private readonly FabricaApiCloudGames _fabrica;

    public TestesLogin(FabricaApiCloudGames fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Login_ComCredenciaisValidas_RetornaJwtRefreshTokenEUsuario()
    {
        using var cliente = _fabrica.CreateClient();
        var email = $"login-{Guid.NewGuid():N}@exemplo.com";
        const string senha = "Senha@123";
        await CadastrarAsync(cliente, email, senha);

        var resposta = await cliente.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, senha },
            TestContext.Current.CancellationToken);
        var conteudo = await resposta.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);
        var login = JsonSerializer.Deserialize<RespostaLoginTeste>(
            conteudo,
            OpcoesJson);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(login.RefreshToken));
        Assert.Equal("Bearer", login.TokenType);
        Assert.Equal(1200, login.ExpiresIn);
        Assert.Equal(email, login.Usuario.Email);
        Assert.Equal("Usuario", login.Usuario.Perfil);
        Assert.DoesNotContain("senha", conteudo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("cpf", conteudo, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dataNascimento", conteudo, StringComparison.OrdinalIgnoreCase);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(login.AccessToken);
        Assert.Equal(email, jwt.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal("Usuario", jwt.Claims.Single(claim => claim.Type == "role").Value);
    }

    [Fact]
    public async Task Login_ComSenhaIncorreta_RetornaNaoAutorizado()
    {
        using var cliente = _fabrica.CreateClient();
        var email = $"senha-incorreta-{Guid.NewGuid():N}@exemplo.com";
        await CadastrarAsync(cliente, email, "Senha@123");

        var resposta = await cliente.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, senha = "Senha@Errada" },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_ComRequestInvalida_RetornaDetalhesValidacao()
    {
        using var cliente = _fabrica.CreateClient();

        var resposta = await cliente.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = "invalido", senha = "" },
            TestContext.Current.CancellationToken);
        var conteudo = await resposta.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("email", conteudo, StringComparison.Ordinal);
        Assert.Contains("senha", conteudo, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Login_ComUsuarioInativo_RetornaNaoAutorizado()
    {
        using var cliente = _fabrica.CreateClient();
        var email = $"inativo-{Guid.NewGuid():N}@exemplo.com";
        const string senha = "Senha@123";
        await CadastrarAsync(cliente, email, senha);
        var repositorio = Assert.IsType<RepositorioUsuariosMemoria>(
            _fabrica.Services.GetRequiredService<IRepositoryUsuarios>());
        Assert.True(repositorio.TentarInativar(email, DateTimeOffset.UtcNow));

        var resposta = await cliente.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, senha },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Login_ComCorpoVazio_RetornaRequisicaoInvalida()
    {
        using var cliente = _fabrica.CreateClient();
        using var conteudo = new StringContent(string.Empty, Encoding.UTF8, "application/json");

        var resposta = await cliente.PostAsync(
            "/api/v1/auth/login",
            conteudo,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    private static async Task CadastrarAsync(HttpClient cliente, string email, string senha)
    {
        var cpf = "1" + new string(Guid.NewGuid().ToString("N").Where(char.IsDigit).ToArray());
        var resposta = await cliente.PostAsJsonAsync(
            "/api/v1/usuarios",
            new
            {
                nome = "Usuário Login",
                cpf,
                dataNascimento = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
                email,
                senha,
                perfilId = RepositorioUsuariosMemoria.PerfilId
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    private sealed record RespostaLoginTeste(
        string AccessToken,
        string RefreshToken,
        string TokenType,
        long ExpiresIn,
        DateTimeOffset ExpiresAt,
        RespostaUsuarioTeste Usuario);

    private sealed record RespostaUsuarioTeste(
        Guid Id,
        string Nome,
        string Email,
        Guid PerfilId,
        string Perfil);
}
