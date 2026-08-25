using FIAP.CloudGames.Api.Configuration;
using FIAP.CloudGames.Modules.Acquisition;
using FIAP.CloudGames.Modules.Catalog;
using FIAP.CloudGames.Modules.Identity;
using FIAP.CloudGames.Modules.Logging;

var builder = WebApplication.CreateBuilder(args);

// Serviços próprios da API e do pipeline HTTP.
builder.Services.AddControllers();
builder.Services.AddProblemDetailsApi();
builder.Services.AddHealthChecks();
builder.Services.AdicionarDocumentacaoSwagger();
builder.Services.AdicionarAutenticacaoJwt(builder.Configuration);

// Módulos funcionais e suas persistências.
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddAcquisitionModule(builder.Configuration);
builder.Services.AddLoggingModule(builder.Configuration);

var app = builder.Build();

// O manipulador nativo de exceções converte falhas inesperadas em respostas Problem Details.
app.UseExceptionHandler();
app.UsarDocumentacaoSwagger();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.MapFallback("/{**path}", () => Results.NotFound());

app.Run();

// WebApplicationFactory usa este ponto de entrada no projeto de testes de integração.
public partial class Program
{
}
