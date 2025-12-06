# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

API Coleta is a .NET 9 REST API for agricultural data collection management (coletas agrícolas). It handles soil samples, farm plots (talhões), geospatial data, nutrient analysis, and report generation for agricultural consulting.

## Build and Run Commands

```bash
# Restore dependencies
dotnet restore

# Build
dotnet build

# Run the application
dotnet run

# Run tests
dotnet test

# Run a specific test class
dotnet test --filter "FullyQualifiedName~ClienteServiceTests"

# Run a specific test
dotnet test --filter "FullyQualifiedName~ClienteServiceTests.SalvarCliente_ShouldPersistClienteWithUsuarioId"

# Apply database migrations
dotnet ef database update

# Create a new migration
dotnet ef migrations add MigrationName
```

## Docker

```bash
# Build image
docker build -t api-coleta .

# The app exposes ports 8080/8081
```

## Architecture

The codebase follows a layered architecture:

- **Controllers/** - API endpoints, inherit from `BaseController` which provides `CustomResponse()` for standardized responses and `ObterIDDoToken()` for JWT extraction
- **Controllers/Mobile/** - Mobile-specific endpoints with simplified DTOs
- **Services/** - Business logic layer; each service has a corresponding repository
- **Repositories/** - Data access layer; all inherit from `GenericRepository<T>` which provides CRUD operations (`Adicionar`, `Atualizar`, `ObterPorId`, `Deletar`, `BuscaPaginada`)
- **Models/Entidades/** - Entity classes that inherit from `Entity` base class (provides `Id` as GUID and `DataInclusao` timestamp)
- **Models/DTOs/** - Data transfer objects for API requests/responses
- **Data/** - EF Core DbContext (`ApplicationDbContext`) and Unit of Work pattern (`IUnitOfWork.Commit()` to save changes)
- **Utils/** - Utility classes including `NutrienteConfig.cs` (nutrient mapping/configuration), `JwtTokenService`, and mapping utilities in `Utils/Maps/`
- **Jobs/** - Background jobs (e.g., `MensagemAgendadaJob` for scheduled notifications)
- **Interfaces/** - Core interfaces (`INotificador`, `IMinioStorage`, `IJwtToken`, `IOneSignalService`)

## Error Handling Pattern

Controllers use `INotificador` to collect validation errors. Call `Notificador.Notificar(new Notificacao("message"))` to add errors, then return `CustomResponse()` which automatically returns 400 with errors if any exist.

## Key Domain Entities

- **Cliente** - Customer/client
- **Fazenda** - Farm
- **Talhao/TalhaoJson** - Farm plot with GeoJSON polygon data
- **Safra** - Harvest/crop season
- **Coleta** - Sample collection
- **PontoColetado** - Collected point data
- **Relatorio** - Analysis report
- **Recomendacao** - Fertilizer/nutrient recommendation
- **NutrientConfig** - Nutrient analysis configuration
- **MensagemAgendada** - Scheduled notification (processed by `MensagemAgendadaJob`)

## Database

- MySQL database via Pomelo.EntityFrameworkCore.MySql
- Connection string in `appsettings.json` under `ConnectionStrings:DefaultConnection`
- All decimal columns use `decimal(12,4)` precision
- Geospatial support via NetTopologySuite

## Testing

Tests use xUnit with EF Core InMemory provider. Use `TestHelper.CreateInMemoryContext()` to create isolated database contexts for tests. Integration tests use `TestApplicationFactory` with `WebApplicationFactory<Program>`, which provides `FakeMinioStorage` and `FakeJwtToken` for mocking external dependencies.

## External Services

- **MinIO** - Object storage for files/images (configured in `appsettings.json`)
- **OneSignal** - Push notifications via `IOneSignalService`
- **Google API** - API key loaded from `GOOGLE_API_KEY` environment variable

## Authentication

JWT Bearer authentication. Configuration in `appsettings.json` under `Jwt` section. Token service implemented in `Utils/JwtTokenService.cs`.

## Environment Variables

Required: `GOOGLE_API_KEY` (see `.env.example`)
