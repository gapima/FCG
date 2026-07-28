# FIAP Cloud Games

API REST acadêmica em .NET 8 com arquitetura em camadas e persistência preparada para PostgreSQL por meio do Entity Framework Core.

## Estrutura da solução

```text
src/
|-- FIAP.CloudGames.Api
|-- FIAP.CloudGames.Application
|-- FIAP.CloudGames.Domain
`-- FIAP.CloudGames.Infrastructure
tests/
|-- FIAP.CloudGames.UnitTests
`-- FIAP.CloudGames.IntegrationTests
```

- `Domain`: entidades e invariantes de negócio, sem dependências técnicas.
- `Application`: casos de uso e contratos necessários para executá-los.
- `Infrastructure`: Entity Framework Core, PostgreSQL e implementações dos contratos.
- `Api`: controllers, contratos HTTP, configuração e ponto de composição.

## Pré-requisitos

- .NET SDK 8;
- uma instância PostgreSQL acessível para operações persistidas;
- ferramenta local do Entity Framework Core restaurada pelo manifesto do repositório.

Na raiz da solução:

```powershell
dotnet tool restore
dotnet restore
```

## Configuração local

A API exige `ConnectionStrings:PostgreSql` e falha imediatamente na inicialização quando ela não está configurada. Armazene credenciais locais em User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:PostgreSql" "Host=localhost;Port=5432;Database=fiap_cloud_games;Username=postgres;Password=SUA_SENHA" --project src/FIAP.CloudGames.Api
```

Também é possível usar a variável de ambiente `ConnectionStrings__PostgreSql`.

## Executar localmente

```powershell
dotnet run --project src/FIAP.CloudGames.Api
```

Endereços padrão do perfil HTTP:

- Swagger: `http://localhost:5080/swagger`
- Health check: `http://localhost:5080/health`

O Swagger é habilitado apenas em `Development` quando `Swagger:Enabled` for `true`.

## Endpoint-guia de criação de usuário

`POST /api/v1/usuarios`

```json
{
  "nome": "Usuário de exemplo",
  "email": "usuario@exemplo.com"
}
```

Respostas principais:

| Situação | Código HTTP |
|---|---:|
| Usuário criado | 201 |
| Dados inválidos | 400 |
| E-mail já cadastrado | 409 |

O nome e o e-mail são normalizados antes da criação da entidade. O e-mail possui índice único no modelo relacional, e tentativas duplicadas resultam em `409 Conflict`.

## IoC e injeção de dependência

O `Program.cs` é a raiz de composição: registra os serviços próprios da API e delega os registros de cada camada a duas classes:

- `ApplicationDependency`, em `Application/IoC`, registra casos de uso e serviços da aplicação;
- `InfrastructureDependency`, em `Infrastructure/IoC`, valida a connection string e registra Entity Framework Core, PostgreSQL e repositórios.

Nenhuma dessas classes resolve serviços manualmente ou guarda um `IServiceProvider`; portanto, não funcionam como service locator. Controllers e casos de uso continuam recebendo dependências pelo contêiner.

## Prontidão para migrations

O projeto está preparado para migrations, mas nenhuma foi criada. Quando o grupo definir o primeiro schema:

```powershell
dotnet ef migrations add NomeDaMigration --project src/FIAP.CloudGames.Infrastructure --startup-project src/FIAP.CloudGames.Api --context ContextoBancoDadosCloudGames --output-dir Persistence/Migrations
```

Para aplicar migrations revisadas:

```powershell
dotnet ef database update --project src/FIAP.CloudGames.Infrastructure --startup-project src/FIAP.CloudGames.Api --context ContextoBancoDadosCloudGames
```

As migrations pertencem ao projeto `Infrastructure`. A API não chama `Database.Migrate()` automaticamente.

## Validação do projeto

```powershell
dotnet restore FIAP.CloudGames.sln
dotnet build FIAP.CloudGames.sln --no-restore --configuration Release
dotnet test FIAP.CloudGames.sln --no-build --no-restore --configuration Release
dotnet format FIAP.CloudGames.sln --no-restore --verify-no-changes
```

Os testes HTTP substituem somente o repositório por uma implementação em memória. Assim, permanecem determinísticos sem alterar a configuração PostgreSQL usada pela aplicação.

## Limites atuais

- Nenhuma migration ou tabela é criada automaticamente.
- O health check comprova a disponibilidade do processo, não a conexão com o banco.
- Os demais módulos de negócio ainda devem ser definidos pelo grupo.
- Antes de produção, devem ser definidos observabilidade, readiness, estratégia de migrations e proteção contra abuso.
