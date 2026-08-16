# Guia rápido de arquitetura e desenvolvimento

Este documento é uma consulta rápida para quem vai desenvolver na FIAP Cloud Games. A solução é uma API .NET 8 organizada em camadas, com pastas verticais por negócio, e executada como um único monólito.

## Visão das camadas

```text
API --------------> Application -------------> Domain
 |                       ^                        ^
 `--> Infrastructure ---'------------------------'
```

### `FIAP.CloudGames.Api`

É a entrada da aplicação. Contém controllers, contratos HTTP, Swagger, tratamento de erros e o `Program.cs`. O endpoint atual de usuário está em `Identity`.

- Recebe e responde HTTP.
- Converte requests em comandos da Application.
- Converte resultados dos casos de uso em códigos como `201`, `400` e `409`.
- Não deve conter regra de negócio nem acesso direto ao banco.

### `FIAP.CloudGames.Application`

Orquestra os casos de uso. O exemplo atual é `Identity/Usuarios/ManipuladorCriarUsuario`.

- Contém comandos, resultados e manipuladores.
- Declara interfaces necessárias, como `IRepositoryUsuarios`.
- Valida e coordena o fluxo da funcionalidade.
- Não conhece controllers, Entity Framework ou PostgreSQL.

Seus registros ficam em `IoC/ApplicationDependency.cs`.

### `FIAP.CloudGames.Domain`

Representa o negócio. Contém entidades, propriedades e regras que devem valer independentemente da API ou do banco. As áreas atuais incluem `Identity`, `Catalog` e `AccessControl`; biblioteca e auditoria ainda precisam de organização vertical consistente.

- Não referencia as outras camadas.
- Não utiliza atributos do Entity Framework.
- Deve permanecer simples e independente de tecnologia.

### `FIAP.CloudGames.Infrastructure`

Implementa os detalhes técnicos definidos por interfaces da Application.

- Contém `DbContext`, mapeamentos e repositórios do Entity Framework.
- Configura Npgsql e PostgreSQL.
- Implementa `IRepositoryUsuarios` com `Repositories/Identity/RepositorioUsuarios`.

Seus registros ficam em `IoC/InfrastructureDependency.cs`.

## Organização por módulo

As camadas continuam sendo projetos separados, e cada área de negócio deve repetir a mesma organização vertical dentro delas:

```text
Api/Identity/...
Application/Identity/...
Domain/Identity/...
Infrastructure/.../Identity/...
tests/.../Identity/...
```

Use o mesmo padrão para `Catalog`, `Library`, `AccessControl` e `Audit` quando surgirem casos de uso dessas áreas. Pastas são a fronteira inicial; evite que um módulo modifique diretamente o estado interno de outro. O banco e o `PostgresqlDbContext` podem continuar compartilhados durante a Fase 1.

## Fluxo do endpoint de exemplo

```text
POST /api/v1/usuarios
  -> UsuariosController
  -> ManipuladorCriarUsuario
  -> IRepositoryUsuarios
  -> RepositorioUsuarios
  -> PostgresqlDbContext
  -> PostgreSQL
```

Cada parte possui uma responsabilidade. O controller trata HTTP, o manipulador executa o caso de uso, o repositório trabalha com persistência e o domínio representa o usuário.

## Injeção de dependência

O `Program.cs` registra os recursos da API e chama as extensões das outras camadas:

```csharp
builder.Services.RegistrarApplicationDependency();
builder.Services.RegistrarInfrastructureDependency(builder.Configuration);
```

Para registrar uma nova dependência:

- serviço ou caso de uso da Application: use `ApplicationDependency`;
- repositório, banco ou integração externa: use `InfrastructureDependency`;
- recurso exclusivo da API: registre no `Program.cs` ou em uma extensão da pasta `Configuration`.

Prefira receber interfaces no construtor. Um caso de uso deve depender de `IRepositoryUsuarios`, não de `RepositorioUsuarios`.

Os principais ciclos de vida são:

- `Scoped`: uma instância por requisição; indicado para `DbContext`, repositórios e casos de uso;
- `Singleton`: uma instância durante toda a aplicação; use somente em serviços seguros para compartilhamento;
- `Transient`: uma nova instância sempre que solicitada; útil para serviços leves e sem estado.

## Como criar uma funcionalidade

Siga esta ordem:

1. Crie ou ajuste entidades e regras no `Domain`.
2. Crie comando, resultado, manipulador e interfaces na `Application`.
3. Implemente persistência ou integrações na `Infrastructure`.
4. Registre as dependências na classe `Dependency` da camada correta.
5. Crie os contratos HTTP e o controller na `Api`.
6. Adicione testes unitários e de integração.

Não retorne entidades do domínio diretamente pela API. Use contratos de resposta próprios.

## Banco e migrations

O projeto usa PostgreSQL, Entity Framework Core e `PostgresqlDbContext`. A connection string é lida de `ConnectionStrings:PostgreSql`.

As migrations existentes ficam em `Infrastructure/Data/EF/Migrations`:

- `InitialCreate`;
- `AdicionarDemaisEntidades`;
- `CorrigirPerfilIdUsuario`;
- `ImplementarLoginJwtRefreshToken`.

A aplicação não executa migrations dentro do processo HTTP. No ambiente Docker, o serviço isolado `migrations` executa `dotnet ef database update` e a API só inicia depois de seu sucesso. Não recrie ou edite migrations do grupo sem confirmar o impacto e verificar se já foram usadas em algum banco compartilhado.

## Antes de entregar

```powershell
dotnet restore FIAP.CloudGames.sln
dotnet build FIAP.CloudGames.sln --no-restore --configuration Release
dotnet test FIAP.CloudGames.sln --no-build --no-restore --configuration Release
dotnet format FIAP.CloudGames.sln --no-restore --verify-no-changes
```

Confirme também que não foram adicionados segredos, arquivos locais ou mudanças fora da funcionalidade trabalhada.

## Configuração local

Use User Secrets em desenvolvimento:

```powershell
dotnet user-secrets set "ConnectionStrings:PostgreSql" "Host=localhost;Port=5432;Database=fiap_cloud_games;Username=postgres;Password=SUA_SENHA" --project src/FIAP.CloudGames.Api
```

```powershell
$chaveJwtLocal = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets set "Jwt:SigningKey" $chaveJwtLocal --project src/FIAP.CloudGames.Api
```

Também podem ser usadas `ConnectionStrings__PostgreSql` e `Jwt__SigningKey`. Em ambientes compartilhados, use variável de ambiente ou cofre de segredos.

## Infraestrutura local com Docker

O `compose.yml` da raiz mantém a API e a infraestrutura no mesmo deploy local, sem transformar os módulos em serviços separados:

```text
API .NET 8 -> rede Docker -> PostgreSQL 16 -> volume persistente
                 ^
                 `-- serviço de migrations executado antes da API
```

Isso continua sendo um monólito: existe um único processo de aplicação e um único banco. O container `migrations` é apenas uma tarefa operacional de inicialização.

```powershell
docker compose up -d --build
```

Consulte [GUIA-DOCKER.md](GUIA-DOCKER.md) para configuração, Postman e inspeção de `usuarios` e `tb_Tokens`.

O fluxo de autenticação está explicado em [GUIA-IMPLEMENTACAO-LOGIN-JWT-REFRESH-TOKEN.md](GUIA-IMPLEMENTACAO-LOGIN-JWT-REFRESH-TOKEN.md).
