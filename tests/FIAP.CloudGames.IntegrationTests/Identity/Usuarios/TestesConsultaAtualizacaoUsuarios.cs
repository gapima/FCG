using System.Net;
using System.Net.Http.Json;
using FIAP.CloudGames.IntegrationTests.Support;

namespace FIAP.CloudGames.IntegrationTests.Identity.Usuarios;

public sealed class TestesConsultaAtualizacaoUsuarios : IClassFixture<FabricaApiCloudGames>
{
    private readonly FabricaApiCloudGames _fabrica;

    public TestesConsultaAtualizacaoUsuarios(FabricaApiCloudGames fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task Obter_EAtualizarUsuario_RetornamRespostaSemDadosSensiveis()
    {
        using var cliente = _fabrica.CreateClient();
        var criado = await CriarUsuarioAsync(cliente);

        var respostaGet = await cliente.GetAsync($"/api/v1/usuarios/{criado.Id}", TestContext.Current.CancellationToken);
        var conteudoGet = await respostaGet.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, respostaGet.StatusCode);
        Assert.DoesNotContain("cpf", conteudoGet, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("senha", conteudoGet, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dataNascimento", conteudoGet, StringComparison.OrdinalIgnoreCase);

        var respostaPut = await cliente.PutAsJsonAsync(
            $"/api/v1/usuarios/{criado.Id}",
            new { nome = "Nome Atualizado", dataNascimento = new DateTimeOffset(1991, 1, 1, 0, 0, 0, TimeSpan.Zero), email = $"atualizado-{Guid.NewGuid():N}@exemplo.com", perfilId = RepositorioUsuariosMemoria.PerfilId },
            TestContext.Current.CancellationToken);
        var atualizado = await respostaPut.Content.ReadFromJsonAsync<RespostaUsuarioTeste>(TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respostaPut.StatusCode);
        Assert.Equal("Nome Atualizado", atualizado!.Nome);
    }

    [Fact]
    public async Task Obter_ComIdInexistente_RetornaNaoEncontrado()
    {
        using var cliente = _fabrica.CreateClient();
        var resposta = await cliente.GetAsync($"/api/v1/usuarios/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Atualizar_ComEmailDeOutroUsuario_RetornaConflito()
    {
        using var cliente = _fabrica.CreateClient();
        var primeiro = await CriarUsuarioAsync(cliente);
        var segundo = await CriarUsuarioAsync(cliente);
        var resposta = await cliente.PutAsJsonAsync(
            $"/api/v1/usuarios/{segundo.Id}",
            new { nome = "Outro Nome", dataNascimento = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), email = primeiro.Email, perfilId = RepositorioUsuariosMemoria.PerfilId },
            TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }

    private static async Task<RespostaUsuarioTeste> CriarUsuarioAsync(HttpClient cliente)
    {
        var identificador = Guid.NewGuid().ToString("N");
        var resposta = await cliente.PostAsJsonAsync(
            "/api/v1/usuarios",
            new { nome = "Usuário Teste", cpf = identificador[..11], dataNascimento = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero), email = $"usuario-{identificador}@exemplo.com", senha = "Senha@123", perfilId = RepositorioUsuariosMemoria.PerfilId },
            TestContext.Current.CancellationToken);
        resposta.EnsureSuccessStatusCode();
        return (await resposta.Content.ReadFromJsonAsync<RespostaUsuarioTeste>(TestContext.Current.CancellationToken))!;
    }

    private sealed record RespostaUsuarioTeste(Guid Id, string Nome, string Email);
}
