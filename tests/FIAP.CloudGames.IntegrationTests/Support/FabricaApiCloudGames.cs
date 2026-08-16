using FIAP.CloudGames.Application.Abstractions.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FIAP.CloudGames.IntegrationTests.Support;

/// <summary>
/// Inicia o pipeline HTTP real com uma configuração isolada exclusiva para os testes.
/// </summary>
public sealed class FabricaApiCloudGames : WebApplicationFactory<Program>
{
    private readonly string _ambiente;

    public FabricaApiCloudGames()
        : this("Development")
    {
    }

    internal FabricaApiCloudGames(string ambiente)
    {
        _ambiente = ambiente;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(_ambiente);

        foreach (var configuracao in ConfiguracaoTeste.CriarValores())
        {
            builder.UseSetting(configuracao.Key, configuracao.Value ?? string.Empty);
        }

        builder.ConfigureTestServices(servicos =>
        {
            servicos.RemoveAll<IRepositoryUsuarios>();
            servicos.RemoveAll<IRepositorioTokens>();
            servicos.AddSingleton<IRepositoryUsuarios, RepositorioUsuariosMemoria>();
            servicos.AddSingleton<IRepositorioTokens, RepositorioTokensMemoria>();
        });
    }
}
