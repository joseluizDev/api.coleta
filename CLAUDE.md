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

### Layered Structure

- **Controllers/** - API endpoints inheriting from `BaseController`
  - `CustomResponse()` returns 400 with errors if `INotificador` has notifications, else 200
  - `ObterIDDoToken()` extracts JWT from Authorization header
- **Controllers/Mobile/** - Mobile-specific endpoints with simplified DTOs
- **Services/** - Business logic layer; all inherit from `ServiceBase` which provides `IUnitOfWork`
  - **Services/Relatorio/** - Specialized services for nutrient classification and soil indicators
- **Repositories/** - Data access layer inheriting from `GenericRepository<T>`
  - Core methods: `Adicionar`, `Atualizar`, `ObterPorId`, `Deletar`, `BuscaPaginada`
- **Models/Entidades/** - Entity classes inheriting from `Entity` (provides `Id` as GUID, `DataInclusao` timestamp)
- **Models/DTOs/** - Data transfer objects
- **Data/** - EF Core `ApplicationDbContext` and Unit of Work (`IUnitOfWork.Commit()`)
- **Utils/** - Utilities including `NutrienteConfig.cs` (nutrient classification with CTC/Argila-dependent intervals)

### Error Handling Pattern

Controllers use `INotificador` to collect validation errors:
```csharp
Notificador.Notificar(new Notificacao("Error message"));
return CustomResponse();  // Returns 400 with errors if any exist
```

### Nutrient Classification System

`Utils/NutrienteConfig.cs` contains the core nutrient classification logic:
- `DefaultNutrienteConfig` - Static intervals for nutrients (pH, V%, m%, micronutrients)
- `config_dependentes` - Dynamic intervals based on CTC or Argila values (Ca, Mg, K, P, Al)
- `NutrientKeyMapping` - Maps short keys (e.g., "Ca") to full names (e.g., "Cálcio - Ca (cmolc/dm³)")
- `GetNutrientClassification()` - Main method that returns classification, color, and intervals
- Classification colors: Muito Baixo (red) → Baixo (orange) → Médio (yellow) → Adequado (green) → Alto (light green) → Muito Alto (dark green)

## Key Domain Entities

- **Cliente** → **Fazenda** → **Talhao** (Client → Farm → Field/Plot)
- **Safra** - Harvest/crop season
- **Coleta** / **MColeta** - Sample collection with GeoJSON
- **PontoColetado** - Individual collected point data
- **Relatorio** - Analysis report with nutrient data
- **Recomendacao** - Fertilizer/nutrient recommendation
- **ConfiguracaoPadrao** / **ConfiguracaoPersonalizada** - Default and user-specific nutrient configs

## Database

- MySQL database via Pomelo.EntityFrameworkCore.MySql
- Connection string in `appsettings.json` under `ConnectionStrings:DefaultConnection`
- All decimal columns use `decimal(12,4)` precision
- Geospatial support via NetTopologySuite

## Testing

Tests use xUnit with EF Core InMemory provider:

```csharp
// Unit tests - use TestHelper for isolated contexts
var context = TestHelper.CreateInMemoryContext();

// Integration tests - use TestApplicationFactory
public class MyTests : IClassFixture<TestApplicationFactory>
{
    // Factory provides FakeMinioStorage and FakeJwtToken
    // Set factory.TestUserId to control authenticated user
}
```

Test files are organized:
- `Tests/` - Unit tests for services and repositories
- `Tests/Integration/` - Integration tests with `TestApplicationFactory`
- `Tests/Fakes/` - Fake implementations (`FakeMinioStorage`, `FakeJwtToken`)
- `Tests/Helpers/` - Test utilities (`TestHelper.cs`, `RelatorioTestData.cs`)

## External Services

- **MinIO** - Object storage for files/images (configured in `appsettings.json`)
- **OneSignal** - Push notifications via `IOneSignalService`
- **Google API** - API key loaded from `GOOGLE_API_KEY` environment variable

## Authentication

JWT Bearer authentication. Configuration in `appsettings.json` under `Jwt` section. Token service implemented in `Utils/JwtTokenService.cs`.

## API Endpoints Structure

| Controller | Route | Purpose |
|------------|-------|---------|
| `RelatorioController` | `/api/relatorio` | Report upload, retrieval, nutrient indicators |
| `VisualizarMapaController` | `/api/visualizar-mapa` | GeoJSON/hexagon map visualization |
| `ColetaController` | `/api/coleta` | Sample collection CRUD |
| `ClienteController` | `/api/cliente` | Client management |
| `FazendaController` | `/api/fazenda` | Farm management |
| `TalhaoController` | `/api/talhao` | Field/plot management |
| `SafraController` | `/api/safra` | Harvest season management |
| `RecomendacaoController` | `/api/recomendacao` | Fertilizer recommendations |
| `NdviController` | `/api/ndvi` | NDVI satellite imagery |

## Environment Variables

Required: `GOOGLE_API_KEY` (see `.env.example`)

Environment is loaded via `DotNetEnv.Env.Load()` at startup.
