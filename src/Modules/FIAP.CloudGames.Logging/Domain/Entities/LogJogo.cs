namespace FIAP.CloudGames.Domain.Entities;

public class LogJogo
{
    public Guid Id { get; private set; }
    public Guid JogoId { get; private set; }
    public string Descricao { get; private set; }
    public DateTime DataCriacao { get; private set; }

    protected LogJogo()
    {
        Descricao = string.Empty;
    }

    public LogJogo(Guid jogoId, string descricao)
    {
        Id = Guid.NewGuid();
        JogoId = jogoId;
        Descricao = descricao;
        DataCriacao = DateTime.UtcNow;
    }
}
