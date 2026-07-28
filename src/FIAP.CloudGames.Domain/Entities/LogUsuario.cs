namespace FIAP.CloudGames.Domain.Entities;

public class LogUsuario
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public string Descricao { get; private set; }
    public DateTime DataCriacao { get; private set; }

    protected LogUsuario()
    {
        Descricao = string.Empty;
    }

    public LogUsuario(Guid usuarioId, string descricao)
    {
        Id = Guid.NewGuid();
        UsuarioId = usuarioId;
        Descricao = descricao;
        DataCriacao = DateTime.UtcNow;
    }
}
