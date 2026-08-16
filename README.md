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
cd src/FIAP.CloudGames.Api
dotnet tool restore
dotnet ef database update
```

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

Para executar as migrations do EF a partir de um container (execução pontual), passe a connection string e execute `dotnet ef database update` dentro da imagem. Exemplo (ajuste caminhos e connection string conforme necessário):

```bash
docker run --rm \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Username=postgres;Password=secret;Database=fiap" \
  --entrypoint dotnet fiap-cloudgames \
  /app/src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj ef database update --project /app/src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj
```

Observações:
- Substitua os valores da connection string pelos dados do seu PostgreSQL (host, porta, usuário, senha, database).
- Ao usar Docker no Windows, `host.docker.internal` aponta para a máquina host.


