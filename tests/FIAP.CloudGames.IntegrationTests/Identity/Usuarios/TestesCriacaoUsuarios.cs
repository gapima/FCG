using System.Net;
using System.Net.Http.Json;
using FIAP.CloudGames.IntegrationTests.Support;

namespace FIAP.CloudGames.IntegrationTests.Identity.Usuarios;

public sealed class TestesCriacaoUsuarios : IClassFixture<FabricaApiCloudGames>
{
    private static readonly Guid PerfilId =
        Guid.Parse("4f642cbc-3720-4bb2-b456-15a97049da5c");
    private readonly FabricaApiCloudGames _fabrica;

    public TestesCriacaoUsuarios(FabricaApiCloudGames fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Criar_ComDadosValidos_RetornaCriado()
    {
        using var cliente = _fabrica.CreateClient();
        var email = $"usuario-{Guid.NewGuid():N}@exemplo.com";

        var resposta = await cliente.PostAsJsonAsync(
            "/api/v1/usuarios",
            new
            {
                nome = "  Usuário   de Teste  ",
                email = email.ToUpperInvariant(),
                perfilId = PerfilId
            },
            TestContext.Current.CancellationToken);

        var usuario = await resposta.Content.ReadFromJsonAsync<RespostaUsuarioTeste>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(usuario);
        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal("Usuário de Teste", usuario.Nome);
        Assert.Equal(email, usuario.Email);
        Assert.NotEqual(default, usuario.DataCriacao);
        Assert.Equal($"/api/v1/usuarios/{usuario.Id}", resposta.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Criar_ComEmailRepetido_RetornaConflito()
    {
        using var cliente = _fabrica.CreateClient();
        var requisicao = new
        {
            nome = "Usuário Duplicado",
            email = $"duplicado-{Guid.NewGuid():N}@exemplo.com",
            perfilId = PerfilId
        };

        var primeiraResposta = await cliente.PostAsJsonAsync(
            "/api/v1/usuarios",
            requisicao,
            TestContext.Current.CancellationToken);
        var segundaResposta = await cliente.PostAsJsonAsync(
            "/api/v1/usuarios",
            requisicao,
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, primeiraResposta.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, segundaResposta.StatusCode);
    }

    [Fact]
    public async Task Criar_ComDadosInvalidos_RetornaDetalhesValidacao()
    {
        using var cliente = _fabrica.CreateClient();

        var resposta = await cliente.PostAsJsonAsync(
            "/api/v1/usuarios",
            new { nome = "A", email = "invalido" },
            TestContext.Current.CancellationToken);
        var conteudo = await resposta.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Contains("nome", conteudo, StringComparison.Ordinal);
        Assert.Contains("email", conteudo, StringComparison.Ordinal);
    }

    private sealed record RespostaUsuarioTeste(
        Guid Id,
        string Nome,
        string Email,
        DateTimeOffset DataCriacao);
}
