using System.Net;
using System.Net.Http.Json;
using FIAP.CloudGames.Domain.Identity.Entities;
using FIAP.CloudGames.IntegrationTests.Support;

namespace FIAP.CloudGames.IntegrationTests.Identity.Usuarios;

public sealed class TestesAutorizacaoUsuarios : IClassFixture<FabricaApiCloudGames>
{
    private readonly FabricaApiCloudGames _fabrica;

    public TestesAutorizacaoUsuarios(FabricaApiCloudGames fabrica)
    {
        _fabrica = fabrica;
    }

    [Fact]
    public async Task GetPutEOperacoesAdministrativas_SemToken_RetornamNaoAutorizado()
    {
        using var cliente = _fabrica.CreateClient();
        var usuario = await AutenticacaoTeste.CadastrarAsync(cliente);

        var get = await cliente.GetAsync(
            $"/api/v1/usuarios/{usuario.Id}",
            TestContext.Current.CancellationToken);
        var put = await cliente.PutAsJsonAsync(
            $"/api/v1/usuarios/{usuario.Id}",
            CriarAtualizacao(),
            TestContext.Current.CancellationToken);
        var criarAdministrador = await cliente.PostAsJsonAsync(
            "/api/v1/usuarios/administradores",
            CriarCadastro(),
            TestContext.Current.CancellationToken);
        var promover = await cliente.PutAsJsonAsync(
            $"/api/v1/usuarios/{usuario.Id}/perfil",
            new { perfilId = PerfisSistema.AdministradorId },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Unauthorized, get.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, put.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, criarAdministrador.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, promover.StatusCode);
    }

    [Fact]
    public async Task UsuarioComum_NaoAcessaOutroUsuarioNemOperacoesAdministrativas()
    {
        using var cliente = _fabrica.CreateClient();
        var emailProprietario = $"proprietario-{Guid.NewGuid():N}@exemplo.com";
        await AutenticacaoTeste.CadastrarAsync(cliente, emailProprietario);
        var outroUsuario = await AutenticacaoTeste.CadastrarAsync(cliente);
        var login = await AutenticacaoTeste.LoginAsync(cliente, emailProprietario);
        AutenticacaoTeste.Autenticar(cliente, login.AccessToken);

        var get = await cliente.GetAsync(
            $"/api/v1/usuarios/{outroUsuario.Id}",
            TestContext.Current.CancellationToken);
        var put = await cliente.PutAsJsonAsync(
            $"/api/v1/usuarios/{outroUsuario.Id}",
            CriarAtualizacao(),
            TestContext.Current.CancellationToken);
        var criarAdministrador = await cliente.PostAsJsonAsync(
            "/api/v1/usuarios/administradores",
            CriarCadastro(),
            TestContext.Current.CancellationToken);
        var promover = await cliente.PutAsJsonAsync(
            $"/api/v1/usuarios/{outroUsuario.Id}/perfil",
            new { perfilId = PerfisSistema.AdministradorId },
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Forbidden, get.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, put.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, criarAdministrador.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, promover.StatusCode);
    }

    [Fact]
    public async Task Administrador_PodeAcessarCriarEPromoverUsuarios()
    {
        using var cliente = _fabrica.CreateClient();
        await AutenticacaoTeste.CriarAdministradorBootstrapAsync(cliente, _fabrica);
        var usuario = await AutenticacaoTeste.CadastrarAsync(cliente);

        var get = await cliente.GetAsync(
            $"/api/v1/usuarios/{usuario.Id}",
            TestContext.Current.CancellationToken);
        var put = await cliente.PutAsJsonAsync(
            $"/api/v1/usuarios/{usuario.Id}",
            CriarAtualizacao(),
            TestContext.Current.CancellationToken);
        var criarAdministrador = await cliente.PostAsJsonAsync(
            "/api/v1/usuarios/administradores",
            CriarCadastro(),
            TestContext.Current.CancellationToken);
        var administradorCriado = await criarAdministrador.Content.ReadFromJsonAsync<UsuarioCriadoTeste>(
            TestContext.Current.CancellationToken);
        var promover = await cliente.PutAsJsonAsync(
            $"/api/v1/usuarios/{usuario.Id}/perfil",
            new { perfilId = PerfisSistema.AdministradorId },
            TestContext.Current.CancellationToken);
        var promovido = await promover.Content.ReadFromJsonAsync<UsuarioCriadoTeste>(
            TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal(HttpStatusCode.OK, put.StatusCode);
        Assert.Equal(HttpStatusCode.Created, criarAdministrador.StatusCode);
        Assert.Equal(PerfisSistema.AdministradorId, administradorCriado!.PerfilId);
        Assert.Equal(HttpStatusCode.OK, promover.StatusCode);
        Assert.Equal(PerfisSistema.AdministradorId, promovido!.PerfilId);
    }

    private static object CriarAtualizacao() => new
    {
        nome = "Nome Atualizado",
        dataNascimento = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
        email = $"atualizado-{Guid.NewGuid():N}@exemplo.com",
        perfilId = PerfisSistema.AdministradorId
    };

    private static object CriarCadastro()
    {
        var identificador = Guid.NewGuid().ToString("N");
        return new
        {
            nome = "Administrador Criado",
            cpf = "1" + new string(identificador.Where(char.IsDigit).ToArray()),
            dataNascimento = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero),
            email = $"admin-criado-{identificador}@exemplo.com",
            senha = AutenticacaoTeste.SenhaValida
        };
    }
}
