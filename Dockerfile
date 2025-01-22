# Base image para execução da aplicação
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Imagem para construção da aplicação
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copia o arquivo de projeto e restaura as dependências
COPY ["api.posto/api.posto.csproj", "api.posto/"]
RUN dotnet restore "./api.posto/api.posto.csproj"

# Copia o restante do código para o container e compila
COPY . .
WORKDIR "/src/api.posto"
RUN dotnet build "./api.posto.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publicação da aplicação
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./api.posto.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Configuração final para execução
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "api.posto.dll"]
