namespace FIAP.CloudGames.Application.Identity.Usuarios;

public static class PoliticaSenha
{
    public const int TamanhoMinimo = 8;

    public static string[] Validar(string? senha)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(senha) || senha.Length < TamanhoMinimo)
            erros.Add($"A senha deve conter no mínimo {TamanhoMinimo} caracteres.");

        if (string.IsNullOrEmpty(senha) || !senha.Any(char.IsLetter))
            erros.Add("A senha deve conter pelo menos uma letra.");

        if (string.IsNullOrEmpty(senha) || !senha.Any(char.IsDigit))
            erros.Add("A senha deve conter pelo menos um número.");

        if (string.IsNullOrEmpty(senha) || senha.All(char.IsLetterOrDigit))
            erros.Add("A senha deve conter pelo menos um caractere especial.");

        return [.. erros];
    }
}
