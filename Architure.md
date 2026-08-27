# Arquitetura do Monólito Modular — FIAP Cloud Games

O FIAP Cloud Games é um monólito modular: existe um único host HTTP e um único deploy, enquanto cada domínio de negócio possui fronteira e assembly próprios.

## Visão geral

```text
FIAP.CloudGames.Api
├── FIAP.CloudGames.Modules.Identity
├── FIAP.CloudGames.Modules.Catalog
├── FIAP.CloudGames.Modules.Acquisition
└── FIAP.CloudGames.Modules.Logging
```

`FIAP.CloudGames.Api` é o composition root. Ele configura o pipeline HTTP, autenticação, Swagger, health checks e registra os quatro módulos. Identity e Catalog expõem controllers e são adicionados explicitamente ao MVC como Application Parts.

Os módulos não possuem `ProjectReference` entre si. A comunicação entre domínios não ocorre por acesso ao contexto ou à infraestrutura de outro módulo.

## Estrutura física

```text
src/
├── FIAP.CloudGames.Api/
│   ├── Configuration/
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── FIAP.CloudGames.Api.Presentation.csproj
└── Modules/
    ├── Identity/
    │   ├── Domain/
    │   ├── Application/
    │   ├── Infrastructure/
    │   ├── Api/
    │   └── FIAP.CloudGames.Modules.Identity.csproj
    ├── Catalog/
    │   ├── Domain/
    │   ├── Application/
    │   ├── Infrastructure/
    │   ├── Api/
    │   └── FIAP.CloudGames.Modules.Catalog.csproj
    ├── Acquisition/
    │   ├── Domain/
    │   ├── Infrastructure/
    │   └── FIAP.CloudGames.Modules.Acquisition.csproj
    └── Logging/
        ├── Domain/
        ├── Infrastructure/
        └── FIAP.CloudGames.Modules.Logging.csproj
```

Camadas internas existem somente quando há implementação real. Acquisition e Logging não possuem Application ou Api atualmente.

## Módulos

### Identity

Identity possui Domain, Application, Infrastructure e Api. É responsável por `Usuario`, `Perfil`, `Token`, `Permissao` e `Autorizacao`, além dos fluxos HTTP de usuários e autenticação. Sua persistência pertence ao `IdentityDbContext`.

`Permissao` e `Autorizacao` fazem parte de Identity; não existe um módulo separado chamado AccessControl.

### Catalog

Catalog possui Domain, Application, Infrastructure e Api. É responsável por `Jogo`, `Categoria` e `CategoriaJogo`. Os casos de uso e endpoints HTTP implementados atualmente são de Jogos. Não existem endpoints HTTP de Categoria nesta versão.

Sua persistência pertence ao `CatalogDbContext`.

### Acquisition

Acquisition possui somente Domain e Infrastructure. Atualmente contém `Aquisicao`, seu mapping, migration, entry point de DI e `AcquisitionDbContext`.

Não há Application, Api, controller, handler ou repository de Acquisition nesta versão.

### Logging

Logging possui somente Domain e Infrastructure. Atualmente contém `LogUsuario`, `LogJogo`, seus mappings, migration, entry point de DI e `LoggingDbContext`.

Não há Application, Api, controller ou mensageria de Logging nesta versão.

## Persistência modular

Não existe DbContext global. Os quatro contextos usam a mesma connection string `ConnectionStrings:PostgreSql` e podem compartilhar a mesma instância PostgreSQL, mas possuem ownership independente:

```text
IdentityDbContext    → schema identity
CatalogDbContext     → schema catalog
AcquisitionDbContext → schema acquisition
LoggingDbContext     → schema logging
```

Cada contexto mantém no próprio módulo:

- mappings;
- migrations;
- snapshot;
- schema;
- tabela `__EFMigrationsHistory` dentro do schema correspondente.

IDs que apontam para outro domínio permanecem escalares. Exemplos atuais incluem `Autorizacao.JogoId`, `Aquisicao.UsuarioId`, `Aquisicao.JogoId`, `LogUsuario.UsuarioId` e `LogJogo.JogoId`. Não existem navegações ou FKs físicas cross-module.

## Docker e deploy

O sistema continua sendo um único monólito e possui um único deploy da aplicação. A imagem publicada contém:

```text
FIAP.CloudGames.Api.Presentation.dll
FIAP.CloudGames.Modules.Identity.dll
FIAP.CloudGames.Modules.Catalog.dll
FIAP.CloudGames.Modules.Acquisition.dll
FIAP.CloudGames.Modules.Logging.dll
```

O serviço Docker `migrations` é uma tarefa operacional de inicialização, não um microserviço. Ele executa as migrations de Identity, Catalog, Acquisition e Logging pelos respectivos projetos modulares antes da inicialização da API.

## Namespaces históricos

O código ainda utiliza namespaces como:

```text
FIAP.CloudGames.Domain.*
FIAP.CloudGames.Application.*
FIAP.CloudGames.Infrastructure.*
```

Esses nomes são históricos e não representam projetos ou assemblies horizontais. Os projetos `FIAP.CloudGames.Domain`, `FIAP.CloudGames.Application` e `FIAP.CloudGames.Infrastructure` não existem mais.

Esta documentação descreve a implementação atual. Ela não pressupõe microserviços, mensageria, novas camadas ou endpoints ainda não implementados.
