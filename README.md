# FIAP Cloud Games

API REST acadêmica em .NET 10 com arquitetura em camadas e persistência preparada para PostgreSQL por meio do Entity Framework Core.

## Arquitetura

Detalhes sobre a arquitetura do projeto estão descritos em [Architecture.md](https://github.com/gapima/FCG/blob/fature/proj-documentation/Architure.md).

## Executar localmente

Clonar o projeto

```bash
git clone https://github.com/gapima/FCG.git
cd FCG
```

Existem duas maneiras comuns de executar o projeto: localmente com o SDK do .NET, ou com Docker.

Opção A — Local (.NET)

- Pré-requisitos: .NET 10 SDK, ferramentas `dotnet-ef` instaladas, e uma instância PostgreSQL em execução. Configure a connection string em `src/FIAP.CloudGames.Api/appsettings.Development.json` ou via variável de ambiente `ConnectionStrings__DefaultConnection`.

Executar migrations:

```bash
dotnet tool restore
dotnet ef database update --project src/Modules/Identity/FIAP.CloudGames.Modules.Identity.csproj --startup-project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --context IdentityDbContext
dotnet ef database update --project src/Modules/Catalog/FIAP.CloudGames.Modules.Catalog.csproj --startup-project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --context CatalogDbContext
dotnet ef database update --project src/Modules/Acquisition/FIAP.CloudGames.Modules.Acquisition.csproj --startup-project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --context AcquisitionDbContext
dotnet ef database update --project src/Modules/Logging/FIAP.CloudGames.Modules.Logging.csproj --startup-project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --context LoggingDbContext
```

Os quatro módulos usam o mesmo PostgreSQL, com schemas e históricos de migrations independentes: `identity`, `catalog`, `acquisition` e `logging`. Não existem FKs físicas entre módulos.

Iniciar o servidor:

```bash
dotnet run --project FIAP.CloudGames.Api.Presentation.csproj --urls "http://localhost:5000"
```

Opção B — Docker

Construir a imagem Docker usando o `Dockerfile` fornecido:

```bash
docker build -t fiap-cloudgames .
```

Executar o container (exemplo mapeando a porta 80 do container para a porta 5000 do host):

```bash
docker run -e ASPNETCORE_ENVIRONMENT=Development -p 5000:80 --name fiap-cloudgames fiap-cloudgames
```

O target `migrations` da imagem aplica os quatro contextos explicitamente e interrompe na primeira falha. Com as variáveis do arquivo `.env` configuradas, execute:

```bash
docker compose run --rm migrations
```
