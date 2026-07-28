namespace FIAP.CloudGames.Domain.Identity.Entities;

public class Perfil
{
    public Guid Id { get; private set; }
    public string Nome { get; private set; }

    protected Perfil()
    {
        Nome = string.Empty;
    }

    public Perfil(string nome)
    {
        Id = Guid.NewGuid();
        Nome = nome;
    }
}
