# Manual da API — FIAP Cloud Games

## Visão geral 

API REST para gerenciamento de usuários e do catálogo de jogos. Os endpoints
de negócio são versionados em `/api/v1` (`/health` e o Swagger ficam fora
desse prefixo). A API aceita e retorna JSON e usa PostgreSQL. A autenticação é
feita com JWT e refresh tokens rotativos.

| Serviço | Endereço local |
| --- | --- |
| API | `http://localhost:5080` |
| Health check | `GET /health` |
| Swagger (Development) | `http://localhost:5080/swagger` |

Datas usam ISO 8601 em UTC, como `1990-01-01T00:00:00Z`. Valores monetários
são números JSON, sem símbolo de moeda.

## Executar localmente

Pré-requisitos: .NET SDK 8, PostgreSQL e uma chave JWT de pelo menos 32 bytes.
Configure a connection string `ConnectionStrings__PostgreSql` e
`Jwt__SigningKey`:

```bash
export ConnectionStrings__PostgreSql='Host=localhost;Port=5432;Database=fiap_cloud_games;Username=postgres;Password=sua-senha'
export Jwt__SigningKey='uma-chave-secreta-com-pelo-menos-32-bytes'
export Swagger__Enabled=true
```

Execute as migrations de cada módulo e a API. Após a modularização, as
migrations ficam separadas por `DbContext` (`Identity`, `Catalog`,
`Acquisition` e `Logging`) e o projeto de startup passou a ser
`FIAP.CloudGames.Api.Presentation`:

```bash
dotnet ef database update --context IdentityDbContext --project src/FIAP.CloudGames.Infrastructure --startup-project src/FIAP.CloudGames.Api.Presentation
dotnet ef database update --context CatalogDbContext --project src/FIAP.CloudGames.Infrastructure --startup-project src/FIAP.CloudGames.Api.Presentation
dotnet ef database update --context AcquisitionDbContext --project src/FIAP.CloudGames.Infrastructure --startup-project src/FIAP.CloudGames.Api.Presentation
dotnet ef database update --context LoggingDbContext --project src/FIAP.CloudGames.Infrastructure --startup-project src/FIAP.CloudGames.Api.Presentation

dotnet run --project src/FIAP.CloudGames.Api.Presentation --urls http://localhost:5080
```

Para Docker, copie `.env.example` para `.env`, preencha `JWT_SIGNING_KEY` e
execute `docker compose -f compose.yml up --build`.

Swagger só fica disponível em ambiente `Development` com `Swagger:Enabled=true`.

## Respostas e erros

| Status | Uso |
| --- | --- |
| `200` | Operação concluída com resposta. |
| `201` | Recurso criado; inclui `Location`. |
| `204` | Operação concluída sem corpo. |
| `400` | Dados de entrada ou paginação inválidos. |
| `401` | Credenciais ou tokens inválidos. |
| `403` | Usuário sem permissão. |
| `404` | Recurso inexistente. |
| `409` | E-mail ou CPF já cadastrado. |

Erros de validação seguem Problem Details:

```json
{
  "title": "Um ou mais dados informados são inválidos.",
  "status": 400,
  "errors": { "email": ["Informe um e-mail válido."] }
}
```

## Autenticação e perfis

Para endpoints protegidos, envie:

```http
Authorization: Bearer <accessToken>
```

| Perfil | ID |
| --- | --- |
| `Usuario` | `11111111-1111-1111-1111-111111111111` |
| `Administrador` | `22222222-2222-2222-2222-222222222222` |

O access token expira em 480 minutos e o refresh token em 7 dias, conforme a
configuração padrão. Ao renovar, o refresh token anterior é invalidado. Logout
revoga todos os refresh tokens ativos do usuário; o access token continua
válido até expirar.

> As migrations criam os perfis, mas não uma conta administradora. O primeiro
> administrador precisa ser provisionado por procedimento seguro externo; depois
> disso, administradores podem criar outros pela API.

## Saúde

### `GET /health`

Verifica se a aplicação está ativa. Retorna `200 OK`.

## Autenticação

### `POST /api/v1/auth/login`

**Acesso:** público.

```json
{ "email": "usuario@exemplo.com", "senha": "Senha@123" }
```

**Resposta `200 OK`:**

```json
{
  "accessToken": "eyJ...",
  "refreshToken": "token-opaco",
  "tokenType": "Bearer",
  "expiresIn": 28800,
  "expiresAt": "2026-08-26T20:00:00+00:00",
  "usuario": {
    "id": "00000000-0000-0000-0000-000000000000",
    "nome": "Usuário de exemplo",
    "email": "usuario@exemplo.com",
    "perfilId": "11111111-1111-1111-1111-111111111111",
    "perfil": "Usuario"
  }
}
```

Retorna `400` para e-mail/senha inválidos e `401` para credenciais inválidas ou
usuário inativo.

### `POST /api/v1/auth/refresh`

**Acesso:** público.

```json
{ "refreshToken": "token-opaco" }
```

Retorna `200` com o mesmo contrato do login, `400` se o token não for informado
e `401` se estiver inválido, expirado, revogado ou já utilizado.

### `POST /api/v1/auth/logout`

**Acesso:** autenticado. Revoga todos os refresh tokens ativos do usuário.

**Resposta:** `204 No Content`.

## Usuários

### `POST /api/v1/usuarios`

Cria um usuário comum. **Acesso:** público.

```json
{
  "nome": "Usuário de exemplo",
  "cpf": "123.456.789-00",
  "dataNascimento": "1990-01-01T00:00:00Z",
  "email": "usuario@exemplo.com",
  "senha": "Senha@123"
}
```

**Resposta `201 Created`:**

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "nome": "Usuário de exemplo",
  "email": "usuario@exemplo.com",
  "perfilId": "11111111-1111-1111-1111-111111111111",
  "ativo": true,
  "criadoEmUtc": "2026-08-26T12:00:00+00:00",
  "dataInativacao": null
}
```

Regras: nome entre 3 e 100 caracteres; CPF obrigatório (normalizado para
dígitos); data de nascimento não futura; e-mail válido; senha com 8 ou mais
caracteres, incluindo maiúscula, minúscula, número e caractere especial.
Retorna `409` para e-mail ou CPF já utilizado.

### `GET /api/v1/usuarios/{id}`

Obtém um usuário. **Acesso:** o próprio titular ou `Administrador`.

**Resposta `200`:** mesmo contrato do cadastro. Retorna `400` se o `id` for
inválido, `401` se não houver autenticação, `403` se o solicitante não for o
próprio titular nem `Administrador` e `404` se o usuário não existir.

### `PUT /api/v1/usuarios/{id}`

Atualiza nome, data de nascimento e e-mail. **Acesso:** o próprio titular ou
`Administrador`.

```json
{
  "nome": "Usuário atualizado",
  "dataNascimento": "1990-01-01T00:00:00Z",
  "email": "usuario.atualizado@exemplo.com"
}
```

Retorna `200` com o mesmo contrato do cadastro, `400` para dados inválidos,
`404` se o usuário não existir e `409` se o e-mail pertencer a outro usuário.

### `POST /api/v1/usuarios/administradores`

Cria um administrador. **Acesso:** `Administrador`.

Usa o mesmo corpo e contrato de resposta do cadastro público, mas retorna o
perfil de administrador. Aplicam-se as mesmas validações e conflitos.

### `PUT /api/v1/usuarios/{id}/perfil`

Altera o perfil de um usuário. **Acesso:** `Administrador`.

```json
{ "perfilId": "22222222-2222-2222-2222-222222222222" }
```

Retorna `200` com o contrato de usuário, `400` para perfil inválido/inexistente
e `404` se o usuário não existir.

## Jogos

> Atualmente todos os endpoints de jogos são públicos, inclusive criação e
> atualização: não exigem Bearer token.

### `POST /api/v1/jogos`

Cria um jogo.

```json
{
  "titulo": "Jogo de exemplo",
  "descricao": "Uma aventura cooperativa.",
  "faixaEtaria": "L",
  "preco": 59.9
}
```

**Resposta `201 Created`:**

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "titulo": "Jogo de exemplo",
  "descricao": "Uma aventura cooperativa.",
  "faixaEtaria": "L",
  "preco": 59.9,
  "ativo": true,
  "dataCadastro": "2026-08-26T12:00:00+00:00"
}
```

O título é obrigatório e possui no máximo 150 caracteres. Preço não pode ser
negativo; descrição e faixa etária são opcionais.

### `GET /api/v1/jogos/{id}`

Obtém um jogo. Retorna `200` com o mesmo contrato da criação, `400` se o `id`
for `Guid.Empty` ou `404` se não existir.

### `GET /api/v1/jogos?pagina=1&tamanhoPagina=20`

Lista jogos paginados. `pagina` padrão é `1` e deve ser maior que zero;
`tamanhoPagina` padrão é `20` e deve estar entre `1` e `100`.

**Resposta `200 OK`:**

```json
{
  "itens": [{
    "id": "00000000-0000-0000-0000-000000000000",
    "titulo": "Jogo de exemplo",
    "descricao": "Uma aventura cooperativa.",
    "faixaEtaria": "L",
    "preco": 59.9,
    "ativo": true,
    "dataCadastro": "2026-08-26T12:00:00+00:00"
  }],
  "pagina": 1,
  "tamanhoPagina": 20
}
```

### `PUT /api/v1/jogos/{id}`

Atualiza título, descrição, faixa etária e preço.

```json
{
  "titulo": "Jogo de exemplo — edição atualizada",
  "descricao": "Descrição atualizada.",
  "faixaEtaria": "12",
  "preco": 69.9
}
```

Retorna `200` com o contrato de jogo, `400` se título ou preço forem inválidos,
e `404` se o jogo não existir.

## Fluxo recomendado

1. Crie um usuário em `POST /api/v1/usuarios`.
2. Faça login em `POST /api/v1/auth/login`.
3. Envie o `accessToken` nas rotas protegidas.
4. Renove ambos os tokens por `POST /api/v1/auth/refresh` antes do vencimento.
5. Encerre a sessão por `POST /api/v1/auth/logout` e descarte os tokens.

Há exemplos de chamadas HTTP em `src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.http`.
