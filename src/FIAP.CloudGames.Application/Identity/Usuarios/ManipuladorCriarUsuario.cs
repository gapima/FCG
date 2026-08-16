using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Application.Abstractions.Security;
using FIAP.CloudGames.Application.Identity;
using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.Application.Identity.Usuarios;

/// <summary>
/// Valida, normaliza e persiste um novo usuário.
/// </summary>
public sealed class ManipuladorCriarUsuario
{
    private const int TamanhoMinimoNome = 3;
    private readonly IRepositoryUsuarios _repositoryUsuarios;
    private readonly IServicoHashSenha _servicoHashSenha;
    private readonly TimeProvider _relogio;

    public ManipuladorCriarUsuario(
        IRepositoryUsuarios repositoryUsuarios,
        IServicoHashSenha servicoHashSenha,
        TimeProvider relogio)
    {
        _repositoryUsuarios = repositoryUsuarios;
        _servicoHashSenha = servicoHashSenha;
        _relogio = relogio;
    }

    public async Task<ResultadoCriarUsuario> ProcessarAsync(ComandoCriarUsuario comando,
                                                            CancellationToken tokenCancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var nomeNormalizado = NormalizadorIdentidade.NormalizarNome(comando.Nome);
        var emailNormalizado = NormalizadorIdentidade.NormalizarEmail(comando.Email);
        var erros = Validar(comando, nomeNormalizado, emailNormalizado);

        if (erros.Count > 0)
        {
            return ResultadoCriarUsuario.DadosInvalidos(erros);
        }

        var senhaHash = _servicoHashSenha.GerarHash(comando.Senha);
        var usuario = new Usuario(
            Guid.NewGuid(),
            nomeNormalizado,
            emailNormalizado!,
            senhaHash,
            comando.PerfilId,
            _relogio.GetUtcNow());

        var adicionado = await _repositoryUsuarios.TentarAdicionarAsync(
            usuario,
            tokenCancelamento);

        return adicionado
            ? ResultadoCriarUsuario.Criado(new UsuarioCriado(
                usuario.Id,
                usuario.Nome,
                usuario.Email,
                usuario.CriadoEmUtc))
            : ResultadoCriarUsuario.EmailJaCadastrado();
    }

    private static Dictionary<string, string[]> Validar(
        ComandoCriarUsuario comando,
        string nomeNormalizado,
        string? emailNormalizado)
    {
        var erros = new Dictionary<string, string[]>(StringComparer.Ordinal);

        if (nomeNormalizado.Length is < TamanhoMinimoNome or > Usuario.TamanhoMaximoNome)
        {
            erros["nome"] =
            [
                $"O nome deve conter entre {TamanhoMinimoNome} e {Usuario.TamanhoMaximoNome} caracteres."
            ];
        }

        if (emailNormalizado is null)
        {
            erros["email"] = ["Informe um e-mail válido."];
        }

        var errosSenha = PoliticaSenha.Validar(comando.Senha);
        if (errosSenha.Length > 0)
        {
            erros["senha"] = errosSenha;
        }

        if (comando.PerfilId == Guid.Empty)
        {
            erros["perfilId"] = ["Informe um perfil válido."];
        }

        return erros;
    }
}
