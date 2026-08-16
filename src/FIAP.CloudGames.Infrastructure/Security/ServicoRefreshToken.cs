using System.Security.Cryptography;
using System.Text;
using FIAP.CloudGames.Application.Abstractions.Security;

namespace FIAP.CloudGames.Infrastructure.Security;

internal sealed class ServicoRefreshToken : IServicoRefreshToken
{
    private const int QuantidadeBytes = 64;
    private readonly ConfiguracaoJwt _configuracao;
    private readonly TimeProvider _relogio;

    public ServicoRefreshToken(ConfiguracaoJwt configuracao, TimeProvider relogio)
    {
        _configuracao = configuracao;
        _relogio = relogio;
    }

    public RefreshTokenGerado GerarToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(QuantidadeBytes);
        var valor = ConverterBase64Url(bytes);
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(valor)));
        var criadoEm = _relogio.GetUtcNow();

        return new RefreshTokenGerado(
            valor,
            hash,
            criadoEm,
            criadoEm.AddDays(_configuracao.RefreshTokenExpirationDays));
    }

    private static string ConverterBase64Url(byte[] bytes) =>
        Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
