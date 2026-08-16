using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using FIAP.CloudGames.Domain.Identity.Entities;
using FIAP.CloudGames.Infrastructure.Security;
using Microsoft.IdentityModel.Tokens;

namespace FIAP.CloudGames.UnitTests.Identity.Auth;

public sealed class TestesServicosSeguranca
{
    private static readonly DateTimeOffset Agora =
        new(2026, 8, 1, 12, 0, 0, TimeSpan.Zero);

    private static readonly ConfiguracaoJwt Configuracao = new(
        "FIAP.CloudGames.Tests",
        "FIAP.CloudGames.Api.Tests",
        "CHAVE-DE-TESTE-COM-PELO-MENOS-32-BYTES",
        20,
        7);

    [Fact]
    public void Hash_GeraValorDiferenteEValidaSomenteSenhaCorreta()
    {
        var servico = new ServicoHashSenha();

        var hash = servico.GerarHash("Senha@123");

        Assert.NotEqual("Senha@123", hash);
        Assert.True(servico.Verificar("Senha@123", hash));
        Assert.False(servico.Verificar("Senha@Errada", hash));
        Assert.False(servico.Verificar("Senha@123", "hash-invalido"));
    }

    [Fact]
    public void Hash_ValidaFormatoPbkdf2GeradoPeloCrudDeUsuarios()
    {
        const string senha = "Senha@123";
        const int iteracoes = 100_000;
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            senha,
            salt,
            iteracoes,
            HashAlgorithmName.SHA256,
            32);
        var hashPersistido = $"PBKDF2-SHA256${iteracoes}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        var servico = new ServicoHashSenha();

        Assert.True(servico.Verificar(senha, hashPersistido));
        Assert.False(servico.Verificar("Senha@Errada", hashPersistido));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Hash_NaoGeraHashParaSenhaAusente(string? senha)
    {
        var servico = new ServicoHashSenha();

        Assert.ThrowsAny<ArgumentException>(() => servico.GerarHash(senha!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Hash_NaoValidaSenhaAusente(string? senha)
    {
        var servico = new ServicoHashSenha();
        var hash = servico.GerarHash("Senha@123");

        Assert.False(servico.Verificar(senha!, hash));
    }

    [Fact]
    public void RefreshToken_GeraValoresUnicosEPersisteHashRecalculavel()
    {
        var servico = new ServicoRefreshToken(Configuracao, new RelogioFixo(Agora));

        var primeiro = servico.GerarToken();
        var segundo = servico.GerarToken();
        var hashCalculado = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(primeiro.Valor)));

        Assert.NotEqual(primeiro.Valor, segundo.Valor);
        Assert.False(string.IsNullOrWhiteSpace(primeiro.Valor));
        Assert.True(primeiro.Valor.Length >= 80);
        Assert.All(
            primeiro.Valor,
            caractere => Assert.True(char.IsLetterOrDigit(caractere) || caractere is '-' or '_'));
        Assert.Equal(hashCalculado, primeiro.Hash);
        Assert.NotEqual(primeiro.Valor, primeiro.Hash);
        Assert.Equal(Agora.AddDays(7), primeiro.ExpiraEm);
    }

    [Fact]
    public void Jwt_GeraClaimsMinimasSemDadosSensiveis()
    {
        var usuario = new Usuario(
            Guid.NewGuid(),
            "Usuário de Teste",
            "12345678900",
            Agora.AddYears(-20),
            "usuario@exemplo.com",
            "hash-secreto",
            PerfisSistema.UsuarioId,
            Agora);
        var servico = new ServicoTokenJwt(Configuracao, new RelogioFixo(Agora));

        var resultado = servico.GerarToken(usuario, PerfisSistema.Usuario);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(resultado.AccessToken);
        var validador = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = validador.ValidateToken(
            resultado.AccessToken,
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(Configuracao.SigningKey)),
                ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                ValidateIssuer = true,
                ValidIssuer = Configuracao.Issuer,
                ValidateAudience = true,
                ValidAudience = Configuracao.Audience,
                ValidateLifetime = false,
                RoleClaimType = "role"
            },
            out _);

        Assert.False(string.IsNullOrWhiteSpace(resultado.AccessToken));
        Assert.NotNull(jwt.Payload.Expiration);
        Assert.Equal(usuario.Id.ToString(), jwt.Subject);
        Assert.Equal(usuario.Email, jwt.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Email).Value);
        Assert.Equal(usuario.Nome, jwt.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Name).Value);
        Assert.Equal(PerfisSistema.Usuario, jwt.Claims.Single(x => x.Type == "role").Value);
        Assert.False(string.IsNullOrWhiteSpace(
            jwt.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value));
        Assert.DoesNotContain(jwt.Claims, x => x.Value == usuario.SenhaHash);
        Assert.DoesNotContain(
            jwt.Claims,
            claim => claim.Type.Contains("senha", StringComparison.OrdinalIgnoreCase)
                || claim.Type.Contains("cpf", StringComparison.OrdinalIgnoreCase)
                || claim.Type.Contains("nascimento", StringComparison.OrdinalIgnoreCase)
                || claim.Type.Contains("refresh", StringComparison.OrdinalIgnoreCase));
        Assert.True(principal.IsInRole(PerfisSistema.Usuario));
        Assert.Equal(1200, resultado.ExpiresIn);
        Assert.Equal(Agora.AddMinutes(20), resultado.ExpiresAt);
    }

    [Fact]
    public void ConfiguracaoJwt_RejeitaRefreshComDuracaoMenorOuIgualAoAccessToken()
    {
        var excecao = Assert.Throws<InvalidOperationException>(() => new ConfiguracaoJwt(
            "FIAP.CloudGames.Tests",
            "FIAP.CloudGames.Api.Tests",
            "CHAVE-DE-TESTE-COM-PELO-MENOS-32-BYTES",
            1440,
            1));

        Assert.Contains("RefreshTokenExpirationDays", excecao.Message, StringComparison.Ordinal);
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
