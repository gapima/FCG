# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS restore

WORKDIR /src

COPY [".config/dotnet-tools.json", ".config/dotnet-tools.json"]
COPY [".editorconfig", ".editorconfig"]
COPY ["Directory.Build.props", "Directory.Build.props"]
COPY ["Directory.Packages.props", "Directory.Packages.props"]
COPY ["src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj", "src/FIAP.CloudGames.Api/"]
COPY ["src/FIAP.CloudGames.Application/FIAP.CloudGames.Application.csproj", "src/FIAP.CloudGames.Application/"]
COPY ["src/FIAP.CloudGames.Domain/FIAP.CloudGames.Domain.csproj", "src/FIAP.CloudGames.Domain/"]
COPY ["src/FIAP.CloudGames.Infrastructure/FIAP.CloudGames.Infrastructure.csproj", "src/FIAP.CloudGames.Infrastructure/"]

RUN dotnet tool restore \
    && dotnet restore "src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj"

FROM restore AS build

COPY src/ src/

RUN dotnet build "src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj" \
    --configuration Release \
    --no-restore

FROM build AS migrations

ENTRYPOINT ["dotnet", "ef", "database", "update", "--project", "src/FIAP.CloudGames.Infrastructure/FIAP.CloudGames.Infrastructure.csproj", "--startup-project", "src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj", "--context", "PostgresqlDbContext", "--configuration", "Release", "--no-build"]

FROM build AS publish

RUN dotnet publish "src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.Presentation.csproj" \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER app

ENTRYPOINT ["dotnet", "FIAP.CloudGames.Api.Presentation.dll"]
