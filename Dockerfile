# Base image para execução da aplicação
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Imagem para construção da aplicação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia apenas o arquivo de projeto e restaura as dependências para aproveitar o cache
COPY ["api.coleta.csproj", "./"]
RUN dotnet restore "./api.coleta.csproj"

# Copia o restante do código somente após o restore
COPY . . 

# Build do projeto
RUN dotnet build "./api.coleta.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicação da aplicação
RUN dotnet publish "./api.coleta.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Configuração final para execução
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# ENTRYPOINT para executar o projeto
ENTRYPOINT ["dotnet", "api.coleta.dll"]
