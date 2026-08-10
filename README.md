# DevForge

DevForge is a developer-focused platform built with .NET 10 using Clean Architecture principles. It provides a server-rendered ASP.NET Core MVC web interface backed by a decoupled REST API service.

---

## Vision
DevForge is designed to bring practical developer utilities and engineering workflows into one focused workspace:
1. **Developer Tools**: Practical client-side utility applications.
2. **API Testing**: Decoupled backend REST API integration.
3. **Robust Engineering**: Built on top of clean architecture, dependency injection, structured logging, health checks, and global exception handling.

---

## Project Status
DevForge Week 1 foundation is complete, establishing a clean ASP.NET Core MVC + Web API application with:
* Clean Architecture
* EF Core
* SQL Server
* Dependency Injection
* Global Exception Handling
* Structured Logging
* Health Checks
* Swagger / OpenAPI
* Unit & Integration Testing
* Clean Product UI

---

## Technology Stack

### Backend & API
* **Language/Runtime**: C# / .NET 10.0
* **API Framework**: ASP.NET Core Web API (Controllers)
* **Data Access**: Entity Framework Core 10.0 (SQL Server)
* **API Documentation**: Swagger / Swashbuckle OpenAPI
* **Structured Logging**: Serilog (Console & File rolling sinks)
* **Diagnostics**: ASP.NET Core Health Checks

### Frontend
* **Server Rendering**: ASP.NET Core MVC & Razor Views
* **Interactions**: HTML5, Vanilla CSS, Modular JavaScript, and Fetch API
* **Typography**: Google Fonts "Inter"

### Testing & DevOps
* **Test Engines**: xUnit, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`
* **CI/CD**: GitHub Actions

---

## Project Structure

```
DevForge/
├── src/
│   ├── DevForge.Web/            # Frontend Presentation Layer (MVC, Razor Views, CSS, JS)
│   ├── DevForge.API/            # API Presentation Layer (REST API Controllers, Middlewares)
│   ├── DevForge.Application/    # Application Layer (Interfaces, DI, etc.)
│   ├── DevForge.Domain/         # Core Domain Layer (Base Entity, AuditableEntity, System Models)
│   └── DevForge.Infrastructure/ # Data Layer (ApplicationDbContext, Configurations, DI, Migrations)
├── tests/
│   ├── DevForge.UnitTests/      # xUnit Unit tests
│   └── DevForge.IntegrationTests/ # xUnit WebApplicationFactory Integration tests
├── docs/
│   ├── architecture/            # Architectural flow documentation & Mermaid diagrams
│   └── api/                     # Endpoint query lists & mock response templates
└── DevForge.slnx                 # Solution File
```

---

## Local Development

### Prerequisites
* .NET 10.0 SDK
* Local SQL Server instance

### Running the API
1. Navigate to the API project folder:
   ```bash
   cd src/DevForge.API
   ```
2. Configure your SQL Server database connection string in `appsettings.Development.json` under `DefaultConnection`.
3. Apply initial migrations to create the database:
   ```bash
   dotnet ef database update
   ```
4. Start the Web API:
   ```bash
   dotnet run
   ```
5. Access the API Swagger UI at: `https://localhost:7172/swagger/index.html` or `http://localhost:5057/swagger/index.html`.

### Running the MVC Frontend
1. Navigate to the Web app folder:
   ```bash
   cd src/DevForge.Web
   ```
2. Start the MVC Web application:
   ```bash
   dotnet run
   ```
3. Open your browser to `http://localhost:5251` or `https://localhost:7246`. The Razor views will render the landing page, and the modular JavaScript will query the backend API.

---

## Testing
To run the automated xUnit test suites (including unit tests and integration tests targeting host health and system status endpoints):
```bash
dotnet test
```
