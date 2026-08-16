using System.Net.Mail;
using FIAP.CloudGames.Application.Abstractions.Repositories;
using FIAP.CloudGames.Domain.Identity.Entities;

namespace FIAP.CloudGames.Application.Identity.Usuarios;

/// <summary>
/// Valida, normaliza e persiste um novo usuário.
/// </summary>
public sealed class ManipuladorCriarUsuario
{
    private const int TamanhoMinimoNome = 3;
    private readonly IRepositoryUsuarios _repositoryUsuarios;
    private readonly TimeProvider _relogio;

    public ManipuladorCriarUsuario(IRepositoryUsuarios repositoryUsuarios, TimeProvider relogio)
    {
        _repositoryUsuarios = repositoryUsuarios;
        _relogio = relogio;
    }

    public async Task<ResultadoCriarUsuario> ProcessarAsync(ComandoCriarUsuario comando,
                                                            CancellationToken tokenCancelamento = default)
    {
        ArgumentNullException.ThrowIfNull(comando);

        var nomeNormalizado = NormalizarNome(comando.Nome);
        var emailNormalizado = NormalizarEmail(comando.Email);
        var erros = Validar(comando, nomeNormalizado, emailNormalizado);

        if (erros.Count > 0)
        {
            return ResultadoCriarUsuario.DadosInvalidos(erros);
        }

        var usuario = new Usuario(
            Guid.NewGuid(),
            nomeNormalizado,
            emailNormalizado!,
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

        return erros;
    }

    private static string NormalizarNome(string? nome) =>
        string.Join(' ',
            (nome ?? string.Empty).Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static string? NormalizarEmail(string? email)
    {
        var valor = email?.Trim();

        if (string.IsNullOrWhiteSpace(valor)
            || valor.Length > Usuario.TamanhoMaximoEmail
            || !MailAddress.TryCreate(valor, out var endereco)
            || !string.Equals(endereco.Address, valor, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return endereco.Address.ToLowerInvariant();
    }
}
