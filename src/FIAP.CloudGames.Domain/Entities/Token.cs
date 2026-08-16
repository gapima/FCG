namespace FIAP.CloudGames.Domain.Entities;

public class Token
{
    public Guid Id { get; set; }

    public string TokenValue { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; }

    public DateTime DataExpiracao { get; set; }

    public DateTime? DataRevogacao { get; set; }
}
