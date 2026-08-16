# FIAP Cloud Games

API REST acadêmica em .NET 8 com arquitetura em camadas, organização vertical por negócio e persistência PostgreSQL por meio do Entity Framework Core.

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
- Docker Desktop com Docker Compose para o ambiente local recomendado;
- ou uma instância PostgreSQL acessível, caso a API seja executada sem Docker;
- ferramenta local do Entity Framework Core restaurada pelo manifesto do repositório.

Na raiz da solução:

```powershell
dotnet tool restore
dotnet restore
```

## Executar com Docker — recomendado

O Compose da raiz inicia PostgreSQL, aplica as migrations em um serviço de execução única e inicia a API:

```powershell
Copy-Item .env.example .env
# Edite POSTGRES_PASSWORD e JWT_SIGNING_KEY antes da primeira subida.
docker compose up -d --build
docker compose ps -a
```

Endereços padrão:

- Swagger: `http://localhost:5080/swagger`;
- health check: `http://localhost:5080/health`;
- PostgreSQL: `localhost:5432`.

O serviço `migrations` deve terminar como `Exited (0)`. O PostgreSQL usa o volume persistente `fiap-cloud-games-postgres-data`.

Consulte [o guia Docker completo](docs/GUIA-DOCKER.md) para entender onde o login é persistido, testar pelo Postman e inspecionar `tb_Tokens` no Docker Desktop.

## Executar API fora do Docker

A origem pretendida para a conexão é `ConnectionStrings:PostgreSql`. Armazene credenciais locais em User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:PostgreSql" "Host=localhost;Port=5432;Database=fiap_cloud_games;Username=postgres;Password=SUA_SENHA" --project src/FIAP.CloudGames.Api
```

Também é possível usar a variável de ambiente `ConnectionStrings__PostgreSql`.

Configure também uma chave JWT com pelo menos 32 bytes:

```powershell
$chaveJwtLocal = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets set "Jwt:SigningKey" $chaveJwtLocal --project src/FIAP.CloudGames.Api
```

Também é possível usar `Jwt__SigningKey`. Não versione credenciais ou chaves reais.

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
  "email": "usuario@exemplo.com",
  "senha": "Senha@123"
}
```

Respostas principais:

| Situação | Código HTTP |
|---|---:|
| Usuário criado | 201 |
| Dados inválidos | 400 |
| E-mail já cadastrado | 409 |

O nome e o e-mail são normalizados antes da criação da entidade. A senha deve ter pelo menos oito caracteres, letra, número e caractere especial, e somente seu hash é persistido. O e-mail possui índice único no modelo relacional, e tentativas duplicadas resultam em `409 Conflict`.

## Login

`POST /api/v1/auth/login`

```json
{
  "email": "usuario@exemplo.com",
  "senha": "Senha@123"
}
```

O login retorna access token JWT de curta duração e refresh token. O access token não é persistido, e somente o hash do refresh token é salvo em `tb_Tokens`. Consulte [o guia completo da implementação](docs/GUIA-IMPLEMENTACAO-LOGIN-JWT-REFRESH-TOKEN.md) e [o guia de execução no Docker](docs/GUIA-DOCKER.md).

## IoC e injeção de dependência

O `Program.cs` é a raiz de composição: registra os serviços próprios da API e delega os registros de cada camada a duas classes:

- `ApplicationDependency`, em `Application/IoC`, registra casos de uso e serviços da aplicação;
- `InfrastructureDependency`, em `Infrastructure/IoC`, valida a connection string e registra Entity Framework Core, PostgreSQL e repositórios.

Nenhuma dessas classes resolve serviços manualmente ou guarda um `IServiceProvider`; portanto, não funcionam como service locator. Controllers e casos de uso continuam recebendo dependências pelo contêiner.

## Migrations

O projeto contém `InitialCreate`, `AdicionarDemaisEntidades`, `CorrigirPerfilIdUsuario` e `ImplementarLoginJwtRefreshToken` em `Infrastructure/Data/EF/Migrations`. A aplicação não chama `Database.Migrate()` automaticamente. No Compose, o serviço isolado `migrations` executa `dotnet ef database update` antes da API.

Para consultar as migrations com a ferramenta local:

```powershell
dotnet tool restore
dotnet ef migrations list --project src/FIAP.CloudGames.Infrastructure --startup-project src/FIAP.CloudGames.Api --context PostgresqlDbContext
```

Para verificar se o modelo compilado divergiu do snapshot, sem gerar nem aplicar migration:

```powershell
dotnet ef migrations has-pending-model-changes --project src/FIAP.CloudGames.Infrastructure --startup-project src/FIAP.CloudGames.Api --context PostgresqlDbContext
```

Não recrie ou altere migrations sem coordenar com o grupo. O Compose aplica as migrations existentes somente no PostgreSQL apontado pela configuração local.

## Validação do projeto

```powershell
dotnet restore FIAP.CloudGames.sln
dotnet build FIAP.CloudGames.sln --no-restore --configuration Release
dotnet test FIAP.CloudGames.sln --no-build --no-restore --configuration Release
dotnet format FIAP.CloudGames.sln --no-restore --verify-no-changes
```

Os testes HTTP substituem os repositórios por implementações em memória. Assim, permanecem determinísticos sem alterar a configuração PostgreSQL usada pela aplicação.

## Limites atuais

- Fora do Compose, nenhuma migration é aplicada automaticamente; no Docker, o serviço `migrations` é responsável por essa etapa.
- O health check comprova a disponibilidade do processo, não a conexão com o banco.
- Os módulos possuem organização inicial, mas ainda não têm fronteiras arquiteturais protegidas.
- O endpoint de renovação do refresh token ainda não está implementado.
- Policies devem ser aplicadas quando forem criados endpoints protegidos de usuário e administrador.
- Antes de produção, devem ser definidos observabilidade, readiness, estratégia de migrations e proteção contra abuso.
