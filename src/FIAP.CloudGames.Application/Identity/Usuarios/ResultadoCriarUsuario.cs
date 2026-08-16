namespace FIAP.CloudGames.Application.Identity.Usuarios;

public enum StatusCriacaoUsuario
{
    Criado,
    DadosInvalidos,
    EmailJaCadastrado
}

/// <summary>
/// Dados não sensíveis devolvidos após a criação de um usuário.
/// </summary>
public sealed record UsuarioCriado(
    Guid Id,
    string Nome,
    string Email,
    DateTimeOffset CriadoEmUtc);

/// <summary>
/// Representa os resultados esperados do caso de uso sem acoplá-los a códigos HTTP.
/// </summary>
public sealed class ResultadoCriarUsuario
{
    private ResultadoCriarUsuario(
        StatusCriacaoUsuario status,
        UsuarioCriado? usuario,
        IReadOnlyDictionary<string, string[]>? erros)
    {
        Status = status;
        Usuario = usuario;
        Erros = erros ?? new Dictionary<string, string[]>();
    }

    public StatusCriacaoUsuario Status { get; }

    public UsuarioCriado? Usuario { get; }

    public IReadOnlyDictionary<string, string[]> Erros { get; }

    public static ResultadoCriarUsuario Criado(UsuarioCriado usuario) =>
        new(StatusCriacaoUsuario.Criado, usuario, null);

    public static ResultadoCriarUsuario DadosInvalidos(
        IReadOnlyDictionary<string, string[]> erros) =>
        new(StatusCriacaoUsuario.DadosInvalidos, null, erros);

    public static ResultadoCriarUsuario EmailJaCadastrado() =>
        new(StatusCriacaoUsuario.EmailJaCadastrado, null, null);
}
