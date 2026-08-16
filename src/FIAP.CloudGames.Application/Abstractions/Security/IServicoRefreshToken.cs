namespace FIAP.CloudGames.Application.Abstractions.Security;

public interface IServicoRefreshToken
{
    RefreshTokenGerado GerarToken();
}

public sealed record RefreshTokenGerado(
    string Valor,
    string Hash,
    DateTimeOffset CriadoEm,
    DateTimeOffset ExpiraEm);
