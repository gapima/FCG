using FIAP.CloudGames.Api.Contracts.Identity.Usuarios;
using FIAP.CloudGames.Application.Identity.Usuarios;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.CloudGames.Api.Controllers.Identity;

[ApiController]
[Route("api/v1/usuarios")]
[Tags("Usuários")]
public sealed class UsuariosController : ControllerBase
{
    [HttpGet("{id:guid}")]
    [ProducesResponseType<RespostaUsuario>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RespostaUsuario>> ObterAsync(
        Guid id,
        ManipuladorObterUsuario manipulador,
        CancellationToken tokenCancelamento)
    {
        var resultado = await manipulador.ProcessarAsync(
            new ConsultaObterUsuario(id),
            tokenCancelamento);

        if (resultado.Status == StatusObtencaoUsuario.IdInvalido)
            return BadRequest(CriarProblemaValidacao(new Dictionary<string, string[]>
            {
                ["id"] = ["Informe um identificador de usuário válido."]
            }));

        if (resultado.Status == StatusObtencaoUsuario.NaoEncontrado)
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Usuário não encontrado.",
                Detail = "Não existe usuário com o identificador informado."
            });

        return Ok(CriarResposta(resultado.Usuario!));
    }

    [HttpPost]
    [ProducesResponseType<RespostaUsuario>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespostaUsuario>> CriarAsync(
        RequisicaoCriarUsuario requisicao,
        ManipuladorCriarUsuario manipulador,
        CancellationToken tokenCancelamento)
    {
        var resultado = await manipulador.ProcessarAsync(
            new ComandoCriarUsuario(
                requisicao.Nome,
                requisicao.CPF,
                requisicao.DataNascimento,
                requisicao.Email,
                requisicao.Senha,
                requisicao.PerfilId),
            tokenCancelamento);

        if (resultado.Status == StatusCriacaoUsuario.DadosInvalidos)
            return BadRequest(CriarProblemaValidacao(resultado.Erros));

        if (resultado.Status == StatusCriacaoUsuario.PerfilNaoEncontrado)
            return BadRequest(CriarProblemaValidacao(new Dictionary<string, string[]>
            {
                ["perfilId"] = ["O perfil informado não existe."]
            }));

        if (resultado.Status is StatusCriacaoUsuario.EmailJaCadastrado or StatusCriacaoUsuario.CpfJaCadastrado)
        {
            var campo = resultado.Status == StatusCriacaoUsuario.EmailJaCadastrado ? "e-mail" : "CPF";
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = $"{campo} já cadastrado.",
                Detail = $"Já existe um usuário cadastrado com o {campo} informado."
            });
        }

        var resposta = CriarResposta(resultado.Usuario!);
        return Created($"/api/v1/usuarios/{resposta.Id}", resposta);
    }

    private static ValidationProblemDetails CriarProblemaValidacao(
        IReadOnlyDictionary<string, string[]> erros) =>
        new(erros.ToDictionary())
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Um ou mais dados informados são inválidos."
        };

    private static RespostaUsuario CriarResposta(DadosUsuario usuario) => new(
        usuario.Id,
        usuario.Nome,
        usuario.Email,
        usuario.PerfilId,
        usuario.Ativo,
        usuario.CriadoEmUtc,
        usuario.DataInativacao);
}
