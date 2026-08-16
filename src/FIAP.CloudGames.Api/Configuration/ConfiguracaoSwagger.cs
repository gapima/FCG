using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

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

            opcoes.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Informe o access token JWT. O prefixo Bearer é aplicado pelo Swagger."
            });

            opcoes.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", documento)] = []
            });
            opcoes.OperationFilter<FiltroSegurancaSwagger>();
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

/// <summary>
/// Mantém endpoints públicos sem requisito de autenticação no documento OpenAPI.
/// Endpoints que receberem <see cref="AuthorizeAttribute"/> herdam o Bearer global.
/// </summary>
internal sealed class FiltroSegurancaSwagger : IOperationFilter
{
    public void Apply(OpenApiOperation operacao, OperationFilterContext contexto)
    {
        var metadados = contexto.ApiDescription.ActionDescriptor.EndpointMetadata;
        var permiteAnonimo = metadados.OfType<IAllowAnonymous>().Any();
        var exigeAutorizacao = metadados.OfType<IAuthorizeData>().Any();

        if (permiteAnonimo || !exigeAutorizacao)
            operacao.Security = [];
    }
}
