using FIAP.CloudGames.Application.Abstractions.Security;
using Microsoft.AspNetCore.Identity;

namespace FIAP.CloudGames.Infrastructure.Security;

internal sealed class ServicoHashSenha : IServicoHashSenha
{
    private static readonly object UsuarioHash = new();
    private readonly PasswordHasher<object> _passwordHasher = new();

    public string GerarHash(string senha)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(senha);
        return _passwordHasher.HashPassword(UsuarioHash, senha);
    }

    public bool Verificar(string senha, string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senha) || string.IsNullOrWhiteSpace(senhaHash))
            return false;

        try
        {
            return _passwordHasher.VerifyHashedPassword(UsuarioHash, senhaHash, senha)
                is PasswordVerificationResult.Success
                    or PasswordVerificationResult.SuccessRehashNeeded;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
