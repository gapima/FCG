using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FIAP.CloudGames.Application.Abstractions.Security;
using FIAP.CloudGames.Domain.Identity.Entities;
using Microsoft.IdentityModel.Tokens;

namespace FIAP.CloudGames.Infrastructure.Security;

internal sealed class ServicoTokenJwt : IServicoTokenJwt
{
    private readonly ConfiguracaoJwt _configuracao;
    private readonly TimeProvider _relogio;

    public ServicoTokenJwt(ConfiguracaoJwt configuracao, TimeProvider relogio)
    {
        _configuracao = configuracao;
        _relogio = relogio;
    }

    public TokenJwtGerado GerarToken(Usuario usuario, string perfil)
    {
        ArgumentNullException.ThrowIfNull(usuario);
        ArgumentException.ThrowIfNullOrWhiteSpace(perfil);

        var emitidoEm = _relogio.GetUtcNow();
        var expiraEm = emitidoEm.AddMinutes(_configuracao.AccessTokenExpirationMinutes);
        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuracao.SigningKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(JwtRegisteredClaimNames.Name, usuario.Nome),
            new Claim("role", perfil),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(
                JwtRegisteredClaimNames.Iat,
                EpochTime.GetIntDate(emitidoEm.UtcDateTime).ToString(CultureInfo.InvariantCulture),
                ClaimValueTypes.Integer64)
        };

        var jwt = new JwtSecurityToken(
            _configuracao.Issuer,
            _configuracao.Audience,
            claims,
            emitidoEm.UtcDateTime,
            expiraEm.UtcDateTime,
            credenciais);

        return new TokenJwtGerado(
            new JwtSecurityTokenHandler().WriteToken(jwt),
            "Bearer",
            (long)(expiraEm - emitidoEm).TotalSeconds,
            expiraEm);
    }
}
