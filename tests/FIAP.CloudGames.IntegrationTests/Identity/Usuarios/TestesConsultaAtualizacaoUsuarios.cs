using System.Net;
using System.Net.Http.Json;
using FIAP.CloudGames.Domain.Identity.Entities;
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
    public async Task Obter_EAtualizarProprioUsuario_RetornamRespostaSemDadosSensiveis()
    {
        using var cliente = _fabrica.CreateClient();
        var (criado, _) = await AutenticacaoTeste.CadastrarEAutenticarAsync(cliente);

        var respostaGet = await cliente.GetAsync(
            $"/api/v1/usuarios/{criado.Id}",
            TestContext.Current.CancellationToken);
        var conteudoGet = await respostaGet.Content.ReadAsStringAsync(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respostaGet.StatusCode);
        Assert.DoesNotContain("cpf", conteudoGet, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("senha", conteudoGet, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dataNascimento", conteudoGet, StringComparison.OrdinalIgnoreCase);

        var respostaPut = await cliente.PutAsJsonAsync(
            $"/api/v1/usuarios/{criado.Id}",
            new
            {
                nome = "Nome Atualizado",
                dataNascimento = new DateTimeOffset(1991, 1, 1, 0, 0, 0, TimeSpan.Zero),
                email = $"atualizado-{Guid.NewGuid():N}@exemplo.com",
                perfilId = PerfisSistema.AdministradorId
            },
            TestContext.Current.CancellationToken);
        var atualizado = await respostaPut.Content.ReadFromJsonAsync<UsuarioCriadoTeste>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, respostaPut.StatusCode);
        Assert.Equal("Nome Atualizado", atualizado!.Nome);
        Assert.Equal(PerfisSistema.UsuarioId, atualizado.PerfilId);
    }

    [Fact]
    public async Task Obter_ComIdInexistente_ComoAdministrador_RetornaNaoEncontrado()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticacaoTeste.CriarAdministradorBootstrapAsync(cliente, _fabrica);

        var resposta = await cliente.GetAsync(
            $"/api/v1/usuarios/{Guid.NewGuid()}",
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task AtualizarProprioUsuario_ComEmailDeOutroUsuario_RetornaConflito()
    {
        using var cliente = _fabrica.CreateClient();
        var primeiro = await AutenticacaoTeste.CadastrarAsync(cliente);
        var emailSegundo = $"segundo-{Guid.NewGuid():N}@exemplo.com";
        var segundo = await AutenticacaoTeste.CadastrarAsync(cliente, emailSegundo);
        var loginSegundo = await AutenticacaoTeste.LoginAsync(cliente, emailSegundo);
        AutenticacaoTeste.Autenticar(cliente, loginSegundo.AccessToken);

        var resposta = await cliente.PutAsJsonAsync(
            $"/api/v1/usuarios/{segundo.Id}",
            new
            {
                nome = "Outro Nome",
                dataNascimento = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
                email = primeiro.Email
            },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);
    }
}
