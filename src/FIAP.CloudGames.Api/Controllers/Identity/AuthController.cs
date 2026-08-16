using System.IdentityModel.Tokens.Jwt;
using FIAP.CloudGames.Api.Contracts.Identity.Auth;
using FIAP.CloudGames.Application.Identity.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.CloudGames.Api.Controllers.Identity;

[ApiController]
[Route("api/v1/auth")]
[Tags("Autenticação")]
public sealed class AuthController : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType<RespostaLogin>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RespostaLogin>> LoginAsync(
        RequisicaoLogin requisicao,
        ManipuladorLogin manipulador,
        CancellationToken tokenCancelamento)
    {
        var resultado = await manipulador.ProcessarAsync(
            new ComandoLogin(requisicao.Email, requisicao.Senha),
            tokenCancelamento);

        if (resultado.Status == StatusLogin.DadosInvalidos)
        {
            return BadRequest(new ValidationProblemDetails(resultado.Erros.ToDictionary())
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Um ou mais dados informados são inválidos."
            });
        }

        if (resultado.Status == StatusLogin.CredenciaisInvalidas)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Credenciais inválidas.",
                Detail = "E-mail ou senha inválidos."
            });
        }

        return Ok(CriarResposta(resultado.Login!));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType<RespostaLogin>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RespostaLogin>> RenovarAsync(
        RequisicaoRefreshToken requisicao,
        ManipuladorRenovarToken manipulador,
        CancellationToken tokenCancelamento)
    {
        var resultado = await manipulador.ProcessarAsync(
            new ComandoRenovarToken(requisicao.RefreshToken),
            tokenCancelamento);

        if (resultado.Status == StatusRenovacaoToken.DadosInvalidos)
        {
            return BadRequest(new ValidationProblemDetails(resultado.Erros.ToDictionary())
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Um ou mais dados informados são inválidos."
            });
        }

        if (resultado.Status == StatusRenovacaoToken.TokenInvalido)
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Refresh token inválido.",
                Detail = "O refresh token é inválido, expirou ou já foi utilizado."
            });
        }

        return Ok(CriarResposta(resultado.Login!));
    }

    [Authorize]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAsync(
        ManipuladorLogout manipulador,
        CancellationToken tokenCancelamento)
    {
        if (!Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var usuarioId))
        {
            return Unauthorized(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Token de acesso inválido.",
                Detail = "O token não identifica um usuário."
            });
        }

        await manipulador.ProcessarAsync(usuarioId, tokenCancelamento);
        return NoContent();
    }

    private static RespostaLogin CriarResposta(LoginRealizado login) =>
        new(
            login.AccessToken,
            login.RefreshToken,
            login.TokenType,
            login.ExpiresIn,
            login.ExpiresAt,
            new RespostaUsuarioLogado(
                login.Usuario.Id,
                login.Usuario.Nome,
                login.Usuario.Email,
                login.Usuario.PerfilId,
                login.Usuario.Perfil));
}
