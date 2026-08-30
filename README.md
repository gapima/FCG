# FIAP Cloud Games

API REST acadêmica em .NET 8, organizada como monólito modular e persistida em PostgreSQL com Entity Framework Core.

## Arquitetura

`FIAP.CloudGames.Api` é o host e composition root. Cada domínio de negócio é compilado em um assembly próprio:

```text
src/
├── FIAP.CloudGames.Api/
│   └── FIAP.CloudGames.Api.Presentation.csproj
└── Modules/
    ├── Identity/
    │   └── FIAP.CloudGames.Modules.Identity.csproj
    ├── Catalog/
    │   └── FIAP.CloudGames.Modules.Catalog.csproj
    ├── Acquisition/
    │   └── FIAP.CloudGames.Modules.Acquisition.csproj
    └── Logging/
        └── FIAP.CloudGames.Modules.Logging.csproj
```

A API referencia diretamente os quatro módulos. Os módulos não possuem `ProjectReference` entre si e são publicados juntos em um único deploy. Consulte [Architure.md](Architure.md) e [docs/GUIA-DE-ARQUITETURA.md](docs/GUIA-DE-ARQUITETURA.md) para detalhes.

## Pré-requisitos

- SDK do .NET 8;
- PostgreSQL, caso a execução seja local;
- Docker e Docker Compose, caso seja usado o ambiente conteinerizado.

A ferramenta `dotnet-ef` 8.0.29 é restaurada pelo manifesto local do repositório.

## Execução local

Clone o projeto e restaure as ferramentas e dependências:

```bash
git clone https://github.com/gapima/FCG.git
cd FCG
dotnet tool restore
dotnet restore FIAP.CloudGames.sln
```

Configure `ConnectionStrings:PostgreSql` em `src/FIAP.CloudGames.Api/appsettings.Development.json`, com User Secrets ou pela variável `ConnectionStrings__PostgreSql`. A chave JWT deve ser fornecida por `Jwt:SigningKey` ou `Jwt__SigningKey`.

Exemplo com User Secrets:

```bash
dotnet user-secrets set "ConnectionStrings:PostgreSql" "Host=localhost;Port=5432;Database=fiap_cloud_games;Username=postgres;Password=SUA_SENHA" --project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj
dotnet user-secrets set "Jwt:SigningKey" "SUA_CHAVE_LOCAL_COM_PELO_MENOS_32_BYTES" --project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj
```

Execute as migrations de cada módulo:

```bash
dotnet ef database update --project src/Modules/Identity/FIAP.CloudGames.Modules.Identity.csproj --startup-project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --context IdentityDbContext
dotnet ef database update --project src/Modules/Catalog/FIAP.CloudGames.Modules.Catalog.csproj --startup-project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --context CatalogDbContext
dotnet ef database update --project src/Modules/Acquisition/FIAP.CloudGames.Modules.Acquisition.csproj --startup-project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --context AcquisitionDbContext
dotnet ef database update --project src/Modules/Logging/FIAP.CloudGames.Modules.Logging.csproj --startup-project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --context LoggingDbContext
```

Inicie a API:

```bash
dotnet run --project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --urls "http://localhost:5080"
```

Com `Swagger:Enabled` habilitado, a documentação estará em `http://localhost:5080/swagger`. O health check está em `http://localhost:5080/health`.

## Execução com Docker

Crie o arquivo de configuração local e ajuste os valores:

```bash
cp .env.example .env
docker compose up -d --build
```

Por padrão, a API fica disponível em `http://localhost:5080`. O Compose inicia um PostgreSQL, executa as migrations dos quatro módulos e então inicia uma única instância da API.

Para executar novamente apenas a tarefa operacional de migrations:

```bash
docker compose run --rm migrations
```

Os containers de API e migrations incluem os assemblies de Identity, Catalog, Acquisition e Logging. Os módulos não são microserviços nem possuem deploy independente.

## Persistência

Os módulos compartilham a mesma instância PostgreSQL, mas cada um possui contexto, schema, migrations, snapshot e tabela de histórico próprios:

| Módulo | DbContext | Schema |
|---|---|---|
| Identity | `IdentityDbContext` | `identity` |
| Catalog | `CatalogDbContext` | `catalog` |
| Acquisition | `AcquisitionDbContext` | `acquisition` |
| Logging | `LoggingDbContext` | `logging` |

Não existe DbContext global nem FK física entre módulos. Referências cross-module são armazenadas como IDs escalares.

## Validação

```bash
dotnet build FIAP.CloudGames.sln -m:1
dotnet test FIAP.CloudGames.sln -m:1
git diff --check
```
