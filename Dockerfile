# Base para a execução da aplicação
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Fase de construção
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar apenas o arquivo de projeto para restaurar as dependências primeiro (melhora o cache do Docker)
COPY ["api.coleta.csproj", "./"]
RUN dotnet restore "./api.coleta.csproj"

# Copiar o restante do código
COPY . .
WORKDIR "/src/api.coleta"
RUN dotnet build "api.coleta.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Fase de publicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "api.coleta.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Fase final para execução
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "api.coleta.dll"]
