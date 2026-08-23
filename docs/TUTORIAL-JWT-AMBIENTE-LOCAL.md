# Tutorial JWT no ambiente local

Este tutorial mostra como executar e testar o fluxo de autenticação usando:

- PostgreSQL no Docker;
- API em debug pelo Visual Studio;
- Postman para as requisições HTTP.

## 1. Pré-requisitos

- Docker Desktop em execução;
- Visual Studio 2022 com suporte ao .NET 8;
- Postman;
- repositório atualizado na branch de autenticação.

Na raiz do repositório:

```powershell
git fetch origin
git checkout feat/criar-autenticacao-fix-conflito
git pull
```

## 2. Criar a configuração do Docker

Copie o arquivo de exemplo apenas se ainda não existir um `.env`:

```powershell
Copy-Item .env.example .env
```

Gere uma chave JWT local:

```powershell
$jwtSigningKey = [Convert]::ToBase64String(
    [Security.Cryptography.RandomNumberGenerator]::GetBytes(48)
)
```

Abra o `.env` e configure:

```dotenv
POSTGRES_USER=postgres
POSTGRES_PASSWORD=SUA_SENHA_LOCAL
POSTGRES_DB=fiap_cloud_games
POSTGRES_PORT=5432

API_PORT=5080
ASPNETCORE_ENVIRONMENT=Development
SWAGGER_ENABLED=true

JWT_SIGNING_KEY=COLE_A_CHAVE_GERADA_AQUI
```

O `.env` contém segredos locais e não deve ser commitado ou compartilhado.

## 3. Subir o PostgreSQL

Na raiz do projeto:

```powershell
docker compose stop api
docker compose up -d postgres
docker compose run --rm migrations
docker compose ps -a
```

Resultado esperado:

- `postgres`: `Up (healthy)`;
- `api`: parada, pois será executada pelo Visual Studio;
- `migrations`: finalizada com código `0`.

## 4. Configurar o Visual Studio

O Visual Studio não lê automaticamente o `.env`. Configure a conexão e a chave JWT usando User Secrets.

Na mesma janela do PowerShell em que a chave foi gerada:

```powershell
dotnet user-secrets set "ConnectionStrings:PostgreSql" "Host=localhost;Port=5432;Database=fiap_cloud_games;Username=postgres;Password=SUA_SENHA_LOCAL" --project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj

dotnet user-secrets set "Jwt:SigningKey" $jwtSigningKey --project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj
```

Use em `SUA_SENHA_LOCAL` o mesmo valor definido em `POSTGRES_PASSWORD`.

Esses comandos são necessários somente na primeira configuração da máquina ou quando os segredos forem substituídos.

## 5. Executar a API

No Visual Studio:

1. Defina `FIAP.CloudGames.Api.Presentation` como projeto de inicialização.
2. Selecione o perfil `FIAP.CloudGames.Api`.
3. Pressione `F5` para executar com debug.

Endereços locais:

| Recurso | Endereço |
|---|---|
| Swagger | `http://localhost:5080/swagger` |
| Health check | `http://localhost:5080/health` |

## 6. Criar o ambiente no Postman

Crie um ambiente com estas variáveis:

| Variável | Valor inicial |
|---|---|
| `baseUrl` | `http://localhost:5080` |
| `usuarioId` | vazio |
| `accessToken` | vazio |
| `refreshToken` | vazio |

## 7. Cadastrar um usuário

```http
POST {{baseUrl}}/api/v1/usuarios
```

Body em formato JSON:

```json
{
  "nome": "Usuário Local",
  "cpf": "52998224725",
  "dataNascimento": "1990-01-01T00:00:00Z",
  "email": "usuario.local@exemplo.com",
  "senha": "Senha@123"
}
```

Resposta esperada: `201 Created`.

Na aba **Scripts > Post-response**, salve o ID retornado:

```javascript
const resposta = pm.response.json();
pm.environment.set("usuarioId", resposta.id);
```

O cadastro público sempre cria o perfil `Usuario`. Use CPF e e-mail diferentes ao repetir o teste.

## 8. Fazer login

```http
POST {{baseUrl}}/api/v1/auth/login
```

Body:

```json
{
  "email": "usuario.local@exemplo.com",
  "senha": "Senha@123"
}
```

Resposta esperada: `200 OK`, contendo `accessToken`, `refreshToken` e os dados do usuário.

Na aba **Scripts > Post-response**:

```javascript
const resposta = pm.response.json();

pm.environment.set("accessToken", resposta.accessToken);
pm.environment.set("refreshToken", resposta.refreshToken);
pm.environment.set("usuarioId", resposta.usuario.id);
```

O access token tem duração padrão de 20 minutos. O refresh token tem duração padrão de 7 dias.

## 9. Acessar um endpoint protegido

```http
GET {{baseUrl}}/api/v1/usuarios/{{usuarioId}}
```

Na aba **Authorization**:

```text
Type: Bearer Token
Token: {{accessToken}}
```

Resposta esperada: `200 OK`.

Sem o token, a resposta deve ser `401 Unauthorized`. Um usuário comum tentando consultar outro usuário deve receber `403 Forbidden`.

## 10. Renovar a autenticação

```http
POST {{baseUrl}}/api/v1/auth/refresh
```

Body:

```json
{
  "refreshToken": "{{refreshToken}}"
}
```

Resposta esperada: `200 OK`, com um novo access token e um novo refresh token.

Atualize as variáveis automaticamente:

```javascript
const resposta = pm.response.json();

pm.environment.set("accessToken", resposta.accessToken);
pm.environment.set("refreshToken", resposta.refreshToken);
```

O refresh token anterior deixa de funcionar depois da renovação. A tentativa de reutilizá-lo deve retornar `401 Unauthorized`.

## 11. Encerrar a sessão

```http
POST {{baseUrl}}/api/v1/auth/logout
```

Na aba **Authorization**:

```text
Type: Bearer Token
Token: {{accessToken}}
```

Resposta esperada: `204 No Content`.

Depois do logout, o refresh token deve retornar `401 Unauthorized`. O access token já emitido pode permanecer válido até completar sua duração de 20 minutos.

## 12. Conferir os dados no PostgreSQL

Abra o terminal do PostgreSQL:

```powershell
docker compose exec postgres psql -U postgres -d fiap_cloud_games
```

Consultar usuários:

```sql
SELECT id, nome, email, perfil_id, ativo, criado_em_utc
FROM usuarios
ORDER BY criado_em_utc DESC;
```

Consultar refresh tokens:

```sql
SELECT
    "Id",
    "UsuarioId",
    "DataCriacao",
    "DataExpiracao",
    "DataRevogacao"
FROM "tb_Tokens"
ORDER BY "DataCriacao" DESC;
```

O banco armazena somente o hash do refresh token. O access token JWT não é armazenado no PostgreSQL.

Para sair do PostgreSQL:

```text
\q
```

## 13. Problemas comuns

### Porta 5080 ocupada

Pare a API do Docker antes de iniciar o Visual Studio:

```powershell
docker compose stop api
```

### Erro de conexão com o PostgreSQL

Confirme que o banco está saudável:

```powershell
docker compose ps -a
```

Na connection string do Visual Studio, use `Host=localhost`. O nome `postgres` funciona somente entre contêineres.

### Erro informando que `Jwt:SigningKey` não foi configurada

Execute novamente:

```powershell
dotnet user-secrets set "Jwt:SigningKey" $jwtSigningKey --project src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj
```

Se uma nova janela do PowerShell foi aberta, gere primeiro uma nova chave com o comando da etapa 2.

### Cadastro retorna `409 Conflict`

O CPF ou o e-mail já está cadastrado. Troque os dois valores antes de repetir o teste.

### Endpoint protegido retorna `401 Unauthorized`

Faça login novamente e confirme que o Postman atualizou `accessToken`. Depois de executar o refresh, o novo access token deve substituir o anterior.

## 14. Encerrar o ambiente

Pare o debug pelo Visual Studio e execute:

```powershell
docker compose stop postgres
```

Os dados continuam preservados no volume do Docker para o próximo uso.
