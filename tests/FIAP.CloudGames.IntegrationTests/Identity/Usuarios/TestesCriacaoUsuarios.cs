using System.Net;
using System.Net.Http.Json;
using FIAP.CloudGames.Domain.Identity.Entities;
using FIAP.CloudGames.IntegrationTests.Support;

namespace FIAP.CloudGames.IntegrationTests.Identity.Usuarios;

public sealed class TestesCriacaoUsuarios : IClassFixture<FabricaApiCloudGames>
{
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
                cpf = $"{Random.Shared.NextInt64(10000000000, 99999999999)}",
                dataNascimento = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
                email = email.ToUpperInvariant(),
                senha = "Senha@123",
                perfilId = PerfisSistema.AdministradorId
            },
            TestContext.Current.CancellationToken);

        var usuario = await resposta.Content.ReadFromJsonAsync<RespostaUsuarioTeste>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        Assert.NotNull(usuario);
        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal("Usuário de Teste", usuario.Nome);
        Assert.Equal(email, usuario.Email);
        Assert.Equal(PerfisSistema.UsuarioId, usuario.PerfilId);
        Assert.NotEqual(default, usuario.CriadoEmUtc);
        Assert.Equal($"/api/v1/usuarios/{usuario.Id}", resposta.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Criar_ComEmailRepetido_RetornaConflito()
    {
        using var cliente = _fabrica.CreateClient();
        var requisicao = new
        {
            nome = "Usuário Duplicado",
            cpf = $"{Random.Shared.NextInt64(10000000000, 99999999999)}",
            dataNascimento = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
            email = $"duplicado-{Guid.NewGuid():N}@exemplo.com",
            senha = "Senha@123",
            perfilId = PerfisSistema.AdministradorId
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
        Guid PerfilId,
        DateTimeOffset CriadoEmUtc);
}
