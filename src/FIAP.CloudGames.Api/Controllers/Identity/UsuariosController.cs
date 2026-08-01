using FIAP.CloudGames.Api.Contracts.Identity.Usuarios;
using FIAP.CloudGames.Application.Identity.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.CloudGames.Api.Controllers.Identity;

/// <summary>
/// Classe referência.
/// </summary>
[ApiController]
[Route("api/v1/usuarios")]
[Tags("Usuários")]
public sealed class UsuariosController : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<RespostaCriarUsuario>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespostaCriarUsuario>> CriarAsync(
        RequisicaoCriarUsuario requisicao,
        ManipuladorCriarUsuario manipulador,
        CancellationToken tokenCancelamento)
    {
        var resultado = await manipulador.ProcessarAsync(
            new ComandoCriarUsuario(requisicao.Nome, requisicao.Email, requisicao.PerfilId),
            tokenCancelamento);

        if (resultado.Status == StatusCriacaoUsuario.DadosInvalidos)
        {
            return BadRequest(new ValidationProblemDetails(resultado.Erros.ToDictionary())
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Um ou mais dados informados são inválidos."
            });
        }

        if (resultado.Status == StatusCriacaoUsuario.EmailJaCadastrado)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "E-mail já cadastrado.",
                Detail = "Já existe um usuário cadastrado com o e-mail informado."
            });
        }

        var usuario = resultado.Usuario!;
        var resposta = new RespostaCriarUsuario(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.CriadoEmUtc);

        return Created($"/api/v1/usuarios/{usuario.Id}", resposta);
    }
}
