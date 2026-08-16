using System.IdentityModel.Tokens.Jwt;
using FIAP.CloudGames.Api.Contracts.Identity.Usuarios;
using FIAP.CloudGames.Application.Identity.Usuarios;
using FIAP.CloudGames.Domain.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FIAP.CloudGames.Api.Controllers.Identity;

[ApiController]
[Authorize]
[Route("api/v1/usuarios")]
[Tags("Usuários")]
public sealed class UsuariosController : ControllerBase
{
    [HttpPut("{id:guid}")]
    [ProducesResponseType<RespostaUsuario>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespostaUsuario>> AtualizarAsync(
        Guid id,
        RequisicaoAtualizarUsuario requisicao,
        ManipuladorAtualizarUsuario manipulador,
        CancellationToken tokenCancelamento)
    {
        if (!PodeAcessarUsuario(id))
            return Forbid();

        var resultado = await manipulador.ProcessarAsync(
            new ComandoAtualizarUsuario(
                id,
                requisicao.Nome,
                requisicao.DataNascimento,
                requisicao.Email),
            tokenCancelamento);

        if (resultado.Status == StatusAtualizacaoUsuario.DadosInvalidos)
            return BadRequest(CriarProblemaValidacao(resultado.Erros));

        if (resultado.Status == StatusAtualizacaoUsuario.NaoEncontrado)
            return NotFound(CriarProblemaUsuarioNaoEncontrado());

        if (resultado.Status == StatusAtualizacaoUsuario.EmailJaCadastrado)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "E-mail já cadastrado.",
                Detail = "Já existe outro usuário cadastrado com o e-mail informado."
            });
        }

        return Ok(CriarResposta(resultado.Usuario!));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<RespostaUsuario>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RespostaUsuario>> ObterAsync(
        Guid id,
        ManipuladorObterUsuario manipulador,
        CancellationToken tokenCancelamento)
    {
        if (!PodeAcessarUsuario(id))
            return Forbid();

        var resultado = await manipulador.ProcessarAsync(
            new ConsultaObterUsuario(id),
            tokenCancelamento);

        if (resultado.Status == StatusObtencaoUsuario.IdInvalido)
        {
            return BadRequest(CriarProblemaValidacao(new Dictionary<string, string[]>
            {
                ["id"] = ["Informe um identificador de usuário válido."]
            }));
        }

        if (resultado.Status == StatusObtencaoUsuario.NaoEncontrado)
            return NotFound(CriarProblemaUsuarioNaoEncontrado());

        return Ok(CriarResposta(resultado.Usuario!));
    }

    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType<RespostaUsuario>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<ActionResult<RespostaUsuario>> CriarAsync(
        RequisicaoCriarUsuario requisicao,
        ManipuladorCriarUsuario manipulador,
        CancellationToken tokenCancelamento) =>
        CriarComPerfilAsync(
            requisicao,
            PerfisSistema.UsuarioId,
            manipulador,
            tokenCancelamento);

    [Authorize(Roles = PerfisSistema.Administrador)]
    [HttpPost("administradores")]
    [ProducesResponseType<RespostaUsuario>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public Task<ActionResult<RespostaUsuario>> CriarAdministradorAsync(
        RequisicaoCriarUsuario requisicao,
        ManipuladorCriarUsuario manipulador,
        CancellationToken tokenCancelamento) =>
        CriarComPerfilAsync(
            requisicao,
            PerfisSistema.AdministradorId,
            manipulador,
            tokenCancelamento);

    [Authorize(Roles = PerfisSistema.Administrador)]
    [HttpPut("{id:guid}/perfil")]
    [ProducesResponseType<RespostaUsuario>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RespostaUsuario>> AlterarPerfilAsync(
        Guid id,
        RequisicaoAlterarPerfilUsuario requisicao,
        ManipuladorAlterarPerfilUsuario manipulador,
        CancellationToken tokenCancelamento)
    {
        var resultado = await manipulador.ProcessarAsync(
            new ComandoAlterarPerfilUsuario(id, requisicao.PerfilId),
            tokenCancelamento);

        if (resultado.Status == StatusAlteracaoPerfilUsuario.DadosInvalidos)
            return BadRequest(CriarProblemaValidacao(resultado.Erros));

        if (resultado.Status == StatusAlteracaoPerfilUsuario.PerfilNaoEncontrado)
        {
            return BadRequest(CriarProblemaValidacao(new Dictionary<string, string[]>
            {
                ["perfilId"] = ["O perfil informado não existe."]
            }));
        }

        if (resultado.Status == StatusAlteracaoPerfilUsuario.NaoEncontrado)
            return NotFound(CriarProblemaUsuarioNaoEncontrado());

        return Ok(CriarResposta(resultado.Usuario!));
    }

    private async Task<ActionResult<RespostaUsuario>> CriarComPerfilAsync(
        RequisicaoCriarUsuario requisicao,
        Guid perfilId,
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
                perfilId),
            tokenCancelamento);

        if (resultado.Status == StatusCriacaoUsuario.DadosInvalidos)
            return BadRequest(CriarProblemaValidacao(resultado.Erros));

        if (resultado.Status == StatusCriacaoUsuario.PerfilNaoEncontrado)
        {
            return BadRequest(CriarProblemaValidacao(new Dictionary<string, string[]>
            {
                ["perfilId"] = ["O perfil necessário para o cadastro não está configurado."]
            }));
        }

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

    private bool PodeAcessarUsuario(Guid usuarioId) =>
        User.IsInRole(PerfisSistema.Administrador)
        || Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var usuarioAutenticadoId)
            && usuarioAutenticadoId == usuarioId;

    private static ValidationProblemDetails CriarProblemaValidacao(
        IReadOnlyDictionary<string, string[]> erros) =>
        new(erros.ToDictionary())
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Um ou mais dados informados são inválidos."
        };

    private static ProblemDetails CriarProblemaUsuarioNaoEncontrado() =>
        new()
        {
            Status = StatusCodes.Status404NotFound,
            Title = "Usuário não encontrado.",
            Detail = "Não existe usuário com o identificador informado."
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
