using Microsoft.OpenApi;

namespace FIAP.CloudGames.Api.Configuration;

/// <summary>
/// Configura o documento OpenAPI e a interface Swagger usados durante o desenvolvimento.
/// </summary>
internal static class ConfiguracaoSwagger
{
    private const string NomeDocumento = "v1";

    public static IServiceCollection AdicionarDocumentacaoSwagger(this IServiceCollection servicos)
    {
        servicos.AddEndpointsApiExplorer();
        servicos.AddSwaggerGen(opcoes =>
        {
            opcoes.SwaggerDoc(NomeDocumento, new OpenApiInfo
            {
                Title = "FIAP Cloud Games API",
                Version = NomeDocumento,
                Description = "Api para gestão de jogos e usuários"
            });

        });

        return servicos;
    }

    public static void UsarDocumentacaoSwagger(this WebApplication aplicacao)
    {
        var habilitado = aplicacao.Configuration.GetValue<bool>("Swagger:Enabled");

        if (!aplicacao.Environment.IsDevelopment() || !habilitado)
        {
            return;
        }

        aplicacao.UseSwagger();
        aplicacao.UseSwaggerUI(opcoes =>
        {
            opcoes.SwaggerEndpoint(
                $"/swagger/{NomeDocumento}/swagger.json",
                "FIAP Cloud Games API v1");
            opcoes.DisplayRequestDuration();
        });
    }
}
