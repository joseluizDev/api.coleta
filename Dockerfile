# Base para execução
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Fase de construção
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Passo 1: Copiar o arquivo de projeto
COPY ["api.coleta.csproj", "./"]

# Passo 2: Restaurar dependências
RUN ls -la api.coleta/ && dotnet restore "api.coleta.csproj"

# Passo 3: Copiar o restante do código
COPY . .

# Passo 4: Compilar o projeto
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
