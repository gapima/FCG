using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Domain.Identity.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace FIAP.CloudGames.IntegrationTests.Support;

internal static class AutenticacaoTeste
{
    internal const string SenhaValida = "Senha@123";

    public static async Task<UsuarioCriadoTeste> CadastrarAsync(
        HttpClient cliente,
        string? email = null,
        object? dadosAdicionais = null)
    {
        var identificador = Guid.NewGuid().ToString("N");
        email ??= $"usuario-{identificador}@exemplo.com";
        var requisicao = new Dictionary<string, object?>
        {
            ["nome"] = "Usuário Teste",
            ["cpf"] = "1" + new string(identificador.Where(char.IsDigit).ToArray()),
            ["dataNascimento"] = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ["email"] = email,
            ["senha"] = SenhaValida
        };

        if (dadosAdicionais is not null)
        {
            foreach (var propriedade in dadosAdicionais.GetType().GetProperties())
                requisicao[propriedade.Name] = propriedade.GetValue(dadosAdicionais);
        }

        var resposta = await cliente.PostAsJsonAsync(
            "/api/v1/usuarios",
            requisicao,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<UsuarioCriadoTeste>(
            TestContext.Current.CancellationToken))!;
    }

    public static async Task<LoginTeste> LoginAsync(
        HttpClient cliente,
        string email,
        string senha = SenhaValida)
    {
        var resposta = await cliente.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email, senha },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<LoginTeste>(
            TestContext.Current.CancellationToken))!;
    }

    public static async Task<(UsuarioCriadoTeste Usuario, LoginTeste Login)> CadastrarEAutenticarAsync(
        HttpClient cliente)
    {
        var email = $"usuario-{Guid.NewGuid():N}@exemplo.com";
        var usuario = await CadastrarAsync(cliente, email);
        var login = await LoginAsync(cliente, email);
        Autenticar(cliente, login.AccessToken);
        return (usuario, login);
    }

    public static async Task<(UsuarioCriadoTeste Usuario, LoginTeste Login)> CriarAdministradorBootstrapAsync(
        HttpClient cliente,
        FabricaApiCloudGames fabrica)
    {
        var email = $"admin-{Guid.NewGuid():N}@exemplo.com";
        var usuario = await CadastrarAsync(cliente, email);
        var repositorio = Assert.IsType<RepositorioUsuariosMemoria>(
            fabrica.Services.GetRequiredService<IRepositoryUsuarios>());
        Assert.True(repositorio.TentarAlterarPerfil(email, PerfisSistema.AdministradorId));
        var login = await LoginAsync(cliente, email);
        Autenticar(cliente, login.AccessToken);
        return (usuario, login);
    }

    public static void Autenticar(HttpClient cliente, string accessToken)
    {
        cliente.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }
}

internal sealed record UsuarioCriadoTeste(
    Guid Id,
    string Nome,
    string Email,
    Guid PerfilId,
    bool Ativo,
    DateTimeOffset CriadoEmUtc,
    DateTimeOffset? DataInativacao);

internal sealed record LoginTeste(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    long ExpiresIn,
    DateTimeOffset ExpiresAt,
    UsuarioLogadoTeste Usuario);

internal sealed record UsuarioLogadoTeste(
    Guid Id,
    string Nome,
    string Email,
    Guid PerfilId,
    string Perfil);
