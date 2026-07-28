using System;

namespace FIAP.CloudGames.Domain.Entities;

public sealed class Aquisicao
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid JogoId { get; private set; }
    public DateTimeOffset DataAquisicao { get; private set; }

    private Aquisicao() { }

    public Aquisicao(Guid id, Guid usuarioId, Guid jogoId)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("O identificador da aquisição não pode ser vazio.");

        if (usuarioId == Guid.Empty)
            throw new ArgumentException("O identificador do usuário é obrigatório.");

        if (jogoId == Guid.Empty)
            throw new ArgumentException("O identificador do jogo é obrigatório.");

        Id = id;
        UsuarioId = usuarioId;
        JogoId = jogoId;
        DataAquisicao = DateTimeOffset.UtcNow;
    }
}
