using System.Text;
using Microsoft.Extensions.Configuration;

namespace FIAP.CloudGames.Infrastructure.Security;

public sealed class ConfiguracaoJwt
{
    public const string Secao = "Jwt";

    internal ConfiguracaoJwt(
        string issuer,
        string audience,
        string signingKey,
        int accessTokenExpirationMinutes,
        int refreshTokenExpirationDays)
    {
        if (TimeSpan.FromDays(refreshTokenExpirationDays)
            <= TimeSpan.FromMinutes(accessTokenExpirationMinutes))
        {
            throw new InvalidOperationException(
                $"{Secao}:RefreshTokenExpirationDays deve representar um período maior "
                + $"que {Secao}:AccessTokenExpirationMinutes.");
        }

        Issuer = issuer;
        Audience = audience;
        SigningKey = signingKey;
        AccessTokenExpirationMinutes = accessTokenExpirationMinutes;
        RefreshTokenExpirationDays = refreshTokenExpirationDays;
    }

    public string Issuer { get; }
    public string Audience { get; }
    public string SigningKey { get; }
    public int AccessTokenExpirationMinutes { get; }
    public int RefreshTokenExpirationDays { get; }

    public static ConfiguracaoJwt Criar(IConfiguration configuracao)
    {
        ArgumentNullException.ThrowIfNull(configuracao);

        var issuer = Exigir(configuracao, $"{Secao}:Issuer");
        var audience = Exigir(configuracao, $"{Secao}:Audience");
        var signingKey = Exigir(configuracao, $"{Secao}:SigningKey");
        var accessTokenExpirationMinutes = ExigirInteiroPositivo(
            configuracao,
            $"{Secao}:AccessTokenExpirationMinutes");
        var refreshTokenExpirationDays = ExigirInteiroPositivo(
            configuracao,
            $"{Secao}:RefreshTokenExpirationDays");

        if (Encoding.UTF8.GetByteCount(signingKey) < 32)
        {
            throw new InvalidOperationException(
                $"{Secao}:SigningKey deve possuir pelo menos 256 bits (32 bytes).");
        }

        return new ConfiguracaoJwt(
            issuer,
            audience,
            signingKey,
            accessTokenExpirationMinutes,
            refreshTokenExpirationDays);
    }

    private static string Exigir(IConfiguration configuracao, string chave) =>
        configuracao[chave] is { } valor && !string.IsNullOrWhiteSpace(valor)
            ? valor
            : throw new InvalidOperationException(
                $"{chave} deve ser configurada por User Secrets ou variável de ambiente.");

    private static int ExigirInteiroPositivo(IConfiguration configuracao, string chave) =>
        int.TryParse(configuracao[chave], out var valor) && valor > 0
            ? valor
            : throw new InvalidOperationException($"{chave} deve ser um inteiro positivo.");
}
