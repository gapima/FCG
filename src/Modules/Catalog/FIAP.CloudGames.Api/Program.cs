using FIAP.CloudGames.Api.Configuration;
using FIAP.CloudGames.Application.IoC;
using FIAP.CloudGames.Infrastructure.IoC;

var builder = WebApplication.CreateBuilder(args);

// Serviços próprios da API e do pipeline HTTP.
builder.Services.AddControllers();
builder.Services.AddProblemDetailsApi();
builder.Services.AddHealthChecks();
builder.Services.AdicionarDocumentacaoSwagger();
builder.Services.AdicionarAutenticacaoJwt(builder.Configuration);

// Serviços organizados pelas respectivas camadas.
builder.Services.RegistrarApplicationDependency();
builder.Services.RegistrarInfrastructureDependency(builder.Configuration);

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
