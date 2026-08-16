# Guia rápido de arquitetura e desenvolvimento

Este documento é uma consulta rápida para quem vai desenvolver na FIAP Cloud Games. A solução é uma API .NET 8 organizada em camadas e executada como um único monólito.

## Visão das camadas

```text
API --------------> Application -------------> Domain
 |                       ^                        ^
 `--> Infrastructure ---'------------------------'
```

### `FIAP.CloudGames.Api`

É a entrada da aplicação. Contém controllers, contratos HTTP, Swagger, tratamento de erros e o `Program.cs`.

- Recebe e responde HTTP.
- Converte requests em comandos da Application.
- Converte resultados dos casos de uso em códigos como `201`, `400` e `409`.
- Não deve conter regra de negócio nem acesso direto ao banco.

### `FIAP.CloudGames.Application`

Orquestra os casos de uso. O exemplo atual é `ManipuladorCriarUsuario`.

- Contém comandos, resultados e manipuladores.
- Declara interfaces necessárias, como `IRepositoryUsuarios`.
- Valida e coordena o fluxo da funcionalidade.
- Não conhece controllers, Entity Framework ou PostgreSQL.

Seus registros ficam em `IoC/ApplicationDependency.cs`.

### `FIAP.CloudGames.Domain`

Representa o negócio. Contém entidades, propriedades e regras que devem valer independentemente da API ou do banco.

- Não referencia as outras camadas.
- Não utiliza atributos do Entity Framework.
- Deve permanecer simples e independente de tecnologia.

### `FIAP.CloudGames.Infrastructure`

Implementa os detalhes técnicos definidos por interfaces da Application.

- Contém `DbContext`, mapeamentos e repositórios do Entity Framework.
- Configura Npgsql e PostgreSQL.
- Implementa `IRepositoryUsuarios` com `RepositorioUsuarios`.

Seus registros ficam em `IoC/InfrastructureDependency.cs`.

## Fluxo do endpoint de exemplo

```text
POST /api/v1/usuarios
  -> UsuariosController
  -> ManipuladorCriarUsuario
  -> IRepositoryUsuarios
  -> RepositorioUsuarios
  -> ContextoBancoDadosCloudGames
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

O projeto está configurado para PostgreSQL e Entity Framework Core. A connection string esperada é `ConnectionStrings:PostgreSql`.

As migrations serão criadas pelo grupo e devem permanecer em `Infrastructure/Data/EF/Migrations`. A aplicação não executa migrations automaticamente ao iniciar.

## Antes de entregar

```powershell
dotnet restore FIAP.CloudGames.sln
dotnet build FIAP.CloudGames.sln --no-restore --configuration Release
dotnet test FIAP.CloudGames.sln --no-build --no-restore --configuration Release
dotnet format FIAP.CloudGames.sln --no-restore --verify-no-changes
```

Confirme também que não foram adicionados segredos, arquivos locais ou mudanças fora da funcionalidade trabalhada.

## Para compilar

Rode no terminal:

dotnet user-secrets set "ConnectionStrings:PostgreSql" "Host=localhost;Port=5432;Database=fiap_cloud_games;Username=postgres;Password=@Testesenha123456" --project src/FIAP.CloudGames.Api
