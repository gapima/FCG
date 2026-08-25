using FIAP.CloudGames.Application.Abstractions.Repositories;

namespace FIAP.CloudGames.Application.Catalog.Jogos;

/// <summary>
/// Consulta um jogo do catálogo pelo identificador.
/// </summary>
public sealed class ManipuladorObterJogoPorId
{
    private readonly IRepositorioJogos _repositorioJogos;

    public ManipuladorObterJogoPorId(IRepositorioJogos repositorioJogos)
    {
        _repositorioJogos = repositorioJogos;
    }

    public async Task<ResultadoObterJogo> ProcessarAsync(
        ConsultaObterJogoPorId consulta,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);

        if (consulta.Id == Guid.Empty)
        {
            return ResultadoObterJogo.IdInvalido();
        }

        var jogo = await _repositorioJogos.ObterPorIdAsync(consulta.Id, cancellationToken);

        if (jogo is null)
        {
            return ResultadoObterJogo.NaoEncontrado();
        }

        return ResultadoObterJogo.Encontrado(new JogoObtido(
            jogo.Id,
            jogo.Titulo,
            jogo.Descricao,
            jogo.FaixaEtaria,
            jogo.Preco,
            jogo.Ativo,
            jogo.DataCadastro));
    }
}
