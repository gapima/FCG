namespace FIAP.CloudGames.Application.Abstractions.Security;

public interface IServicoHashSenha
{
    string GerarHash(string senha);

    bool Verificar(string senha, string senhaHash);
}
