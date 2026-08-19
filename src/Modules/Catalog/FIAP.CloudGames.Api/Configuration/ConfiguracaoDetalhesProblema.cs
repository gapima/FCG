namespace FIAP.CloudGames.Api.Configuration;

/// <summary>
/// Padroniza respostas de erros inesperados usando detalhes de problema da RFC 7807.
/// </summary>
internal static class ConfiguracaoDetalhesProblema
{
    public static IServiceCollection AddProblemDetailsApi(this IServiceCollection servicos)
    {
        servicos.AddProblemDetails(opcoes =>
        {
            opcoes.CustomizeProblemDetails = contexto =>
            {
                contexto.ProblemDetails.Extensions.TryAdd(
                    "traceId",
                    contexto.HttpContext.TraceIdentifier);
            };
        });

        return servicos;
    }
}
