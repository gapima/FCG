# Guia rápido de arquitetura e desenvolvimento

O FIAP Cloud Games é uma API .NET 8 organizada como monólito modular. Existe um único processo de aplicação, enquanto Identity, Catalog, Acquisition e Logging são compilados em assemblies próprios.

## Grafo de projetos

```text
FIAP.CloudGames.Api.Presentation
├── FIAP.CloudGames.Modules.Identity
├── FIAP.CloudGames.Modules.Catalog
├── FIAP.CloudGames.Modules.Acquisition
└── FIAP.CloudGames.Modules.Logging
```

A API é o host e composition root. Os módulos não possuem `ProjectReference` entre si.

## Responsabilidades do host

`src/FIAP.CloudGames.Api` contém somente recursos do host, como:

- `Program.cs`;
- configuração de autenticação;
- configuração de Swagger;
- tratamento de erros e health checks;
- appsettings e bootstrap dos módulos.

O host registra os módulos por seus entry points:

```csharp
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule(builder.Configuration);
builder.Services.AddAcquisitionModule(builder.Configuration);
builder.Services.AddLoggingModule(builder.Configuration);
```

Identity e Catalog possuem controllers nos próprios assemblies. Por isso, o MVC carrega explicitamente seus Application Parts. Acquisition e Logging não possuem controllers.

## Organização interna dos módulos

Um módulo pode conter as seguintes responsabilidades, somente quando necessárias:

```text
Module/
├── Domain/
├── Application/       quando existir
├── Infrastructure/
├── Api/               quando existir
└── Module.csproj
```

Estado atual:

| Módulo | Domain | Application | Infrastructure | Api |
|---|---:|---:|---:|---:|
| Identity | Sim | Sim | Sim | Sim |
| Catalog | Sim | Sim | Sim | Sim |
| Acquisition | Sim | Não | Sim | Não |
| Logging | Sim | Não | Sim | Não |

Não crie camadas vazias apenas para uniformizar diretórios.

## Ownership funcional

### Identity

Identity é responsável por usuários, perfis, tokens, permissões e autorizações. `Permissao` e `Autorizacao` pertencem a este módulo; AccessControl não é um módulo separado.

Os controllers atuais cobrem autenticação e usuários. O fluxo de criação, por exemplo, permanece integralmente no assembly de Identity:

```text
POST /api/v1/usuarios
  → UsuariosController
  → ManipuladorCriarUsuario
  → IRepositorioUsuarios
  → RepositorioUsuarios
  → IdentityDbContext
```

### Catalog

Catalog é responsável por jogos, categorias e pelo vínculo `CategoriaJogo`. A superfície HTTP implementada atualmente contém somente operações de Jogos em `/api/v1/jogos`.

Não há controller ou endpoints HTTP de Categoria nesta versão.

### Acquisition

Acquisition contém a entidade `Aquisicao` e sua persistência. Ainda não possui casos de uso, contratos HTTP ou controllers.

### Logging

Logging contém `LogUsuario`, `LogJogo` e sua persistência. Ainda não possui casos de uso, API ou mensageria.

## Regras de dependência

- Um módulo não referencia o projeto de outro módulo.
- Um módulo não acessa o DbContext ou a Infrastructure de outro módulo.
- A API conhece somente os entry points públicos necessários para composição.
- Referências cross-module são IDs escalares.
- Não existem FKs físicas cross-module.
- A criação de uma abstração compartilhada deve responder a uma necessidade funcional real.

## Persistência

O sistema utiliza uma connection string chamada `ConnectionStrings:PostgreSql`. Cada módulo registra seu próprio contexto:

| Módulo | DbContext | Schema | Histórico |
|---|---|---|---|
| Identity | `IdentityDbContext` | `identity` | `identity.__EFMigrationsHistory` |
| Catalog | `CatalogDbContext` | `catalog` | `catalog.__EFMigrationsHistory` |
| Acquisition | `AcquisitionDbContext` | `acquisition` | `acquisition.__EFMigrationsHistory` |
| Logging | `LoggingDbContext` | `logging` | `logging.__EFMigrationsHistory` |

Cada módulo contém seus mappings, migrations e snapshot. Não existe DbContext global.

Para aplicar as migrations localmente:

```powershell
dotnet tool restore
dotnet ef database update --project src/Modules/Identity/FIAP.CloudGames.Modules.Identity.csproj --startup-project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --context IdentityDbContext
dotnet ef database update --project src/Modules/Catalog/FIAP.CloudGames.Modules.Catalog.csproj --startup-project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --context CatalogDbContext
dotnet ef database update --project src/Modules/Acquisition/FIAP.CloudGames.Modules.Acquisition.csproj --startup-project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --context AcquisitionDbContext
dotnet ef database update --project src/Modules/Logging/FIAP.CloudGames.Modules.Logging.csproj --startup-project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj --context LoggingDbContext
```

## Como implementar uma funcionalidade

Trabalhe dentro do módulo proprietário:

1. ajuste as entidades e regras em Domain;
2. se houver caso de uso, implemente comando, resultado, handler e abstrações em Application;
3. implemente persistência ou integração em Infrastructure;
4. registre a implementação no entry point `Add*Module`;
5. se houver HTTP, crie contratos e controller em Api;
6. adicione testes unitários e de integração adequados.

Não crie Application ou Api em módulos que ainda não necessitam dessas responsabilidades. Não exponha entidades diretamente como contratos HTTP.

## Configuração local

Use User Secrets ou variáveis de ambiente para dados sensíveis:

```powershell
dotnet user-secrets set "ConnectionStrings:PostgreSql" "Host=localhost;Port=5432;Database=fiap_cloud_games;Username=postgres;Password=SUA_SENHA" --project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj
$chaveJwtLocal = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets set "Jwt:SigningKey" $chaveJwtLocal --project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj
```

As variáveis equivalentes são `ConnectionStrings__PostgreSql` e `Jwt__SigningKey`.

## Docker

O Compose inicia PostgreSQL, executa uma tarefa de migrations e publica uma única API:

```text
migrations ── aplica os quatro contextos ──┐
                                           v
PostgreSQL <──────────────────── FIAP.CloudGames.Api
                                   ├── Identity.dll
                                   ├── Catalog.dll
                                   ├── Acquisition.dll
                                   └── Logging.dll
```

Os módulos não são microserviços. O container `migrations` é apenas uma etapa operacional anterior ao único processo da aplicação.

```powershell
Copy-Item .env.example .env
docker compose up -d --build
```

## Namespaces históricos

Namespaces iniciados por `FIAP.CloudGames.Domain`, `FIAP.CloudGames.Application` ou `FIAP.CloudGames.Infrastructure` foram preservados para evitar uma refatoração sem benefício funcional imediato. Eles não indicam dependência dos antigos projetos agregadores, que já foram removidos.

Ao desenvolver uma feature, não faça uma normalização geral desses namespaces como efeito colateral.

## Antes de entregar

```powershell
dotnet build FIAP.CloudGames.sln -m:1
dotnet test FIAP.CloudGames.sln -m:1
git diff --check
```

Confirme também que não foram adicionados segredos, migrations não planejadas, referências entre módulos ou alterações fora da funcionalidade trabalhada.
