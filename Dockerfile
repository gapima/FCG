FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.csproj", "src/FIAP.CloudGames.Api/"]
COPY ["src/FIAP.CloudGames.Application/FIAP.CloudGames.Application.csproj", "src/FIAP.CloudGames.Application/"]
COPY ["src/FIAP.CloudGames.Domain/FIAP.CloudGames.Domain.csproj", "src/FIAP.CloudGames.Domain/"]
COPY ["src/FIAP.CloudGames.Infrastructure/FIAP.CloudGames.Infrastructure.csproj", "src/FIAP.CloudGames.Infrastructure/"]

RUN dotnet restore "src/FIAP.CloudGames.Api/FIAP.CloudGames.Api.csproj"

COPY . .

WORKDIR "/src/src/FIAP.CloudGames.Api"
RUN dotnet build "FIAP.CloudGames.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FIAP.CloudGames.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "FIAP.CloudGames.Api.dll"]
