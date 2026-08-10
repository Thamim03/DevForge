# DevForge

DevForge is an open-source developer platform that centralizes essential productivity tools, an interactive API request playground, and C#/.NET coding challenges into a single, cohesive workspace. Built with .NET 10 using Clean Architecture principles, it provides a server-rendered ASP.NET Core MVC web interface backed by a decoupled REST API service.

---

## Vision
DevForge bridges the gap between everyday utility tools and skills growth:
1. **Developer Tools**: Fast, client-side utility applications (JSON Formatter, JWT Decoder, SQL Formatter, Base64 Encoder, HL7 Validator, and GUID Generator).
2. **API Playground**: An interactive HTTP client allowing developers to compose requests, modify headers, and debug payloads directly in the browser.
3. **.NET Engineering Challenges**: Practical C# and system design puzzles covering EF Core, LINQ diagnostics, database query tuning, and security patterns.
4. **User Platform**: Secure user accounts, session authorization, and preference tracking.

---

## Project Status
DevForge is under active development. The current implementation establishes the foundational system architecture, API endpoints, persistence layer, structured logging, containerization configurations, and a minimal web interface. Core features are being built incrementally.

---

## Technology Stack

### Backend & API
* **Language/Runtime**: C# / .NET 10.0
* **API Framework**: ASP.NET Core Web API (Controllers)
* **Data Access**: Entity Framework Core 10.0 (SQL Server)
* **API Documentation**: Swagger / Swashbuckle OpenAPI
* **Structured Logging**: Serilog (Console & File rolling sinks)
* **Validation**: FluentValidation
* **Diagnostics**: ASP.NET Core Health Checks

### Frontend
* **Server Rendering**: ASP.NET Core MVC & Razor Views
* **Interactions**: HTML5, Vanilla CSS, Modular JavaScript, and Fetch API
* **Typography**: Google Fonts "Inter"

### Testing & DevOps
* **Test Engines**: xUnit, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`
* **Containerization**: Docker & Docker Compose (SQL Server 2022, Web API, Web MVC)
* **CI/CD**: GitHub Actions

---

## Project Structure

```
DevForge/
├── src/
│   ├── DevForge.Web/            # Frontend Presentation Layer (MVC, Razor Views, CSS, JS)
│   ├── DevForge.API/            # API Presentation Layer (REST API Controllers, Middlewares)
│   ├── DevForge.Application/    # Application Layer (Interfaces, Result models, Exceptions, DI)
│   ├── DevForge.Domain/         # Core Domain Layer (Base Entity, AuditableEntity, System Models)
│   └── DevForge.Infrastructure/ # Data Layer (ApplicationDbContext, Configurations, DI, Migrations)
├── tests/
│   ├── DevForge.UnitTests/      # xUnit Unit tests for Domain/Application helpers
│   └── DevForge.IntegrationTests/ # xUnit WebApplicationFactory Integration tests
├── docs/
│   ├── architecture/            # Architectural flow documentation & Mermaid diagrams
│   └── api/                     # Endpoint query lists & mock response templates
├── docker-compose.yml           # Orchestration for SQL Server, Web API, and Web MVC
└── DevForge.slnx                 # Solution File
```

---

## Local Development

### Prerequisites
* .NET 10.0 SDK
* SQL Server instance or Docker Desktop

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

## Running with Docker
You can run the entire platform (SQL Server, Web API, and Web MVC) inside Docker containers:
1. Run Docker Compose build and start:
   ```bash
   docker-compose up --build
   ```
2. Open:
   * **DevForge MVC Web App**: `http://localhost:8080` (Served via Kestrel inside the Docker container)
   * **Web API Swagger**: `http://localhost:5000/swagger/index.html`
   * **Database Port**: `localhost,1433` (SQL Server)

---

## Testing
To run the automated xUnit test suites (including unit tests for Result wrappers and integration tests targeting host health / system endpoints):
```bash
dotnet test
```

---

## Roadmap

* **Milestone 1 - Foundation** *(Completed)*: System architecture, EF Core database mapping, logging, global error filters, Swagger versioning, Dockerization, and CI.
* **Milestone 2 - Authentication & RBAC**: User registration, JWT logins, Refresh Token rotations, roles and permissions.
* **Milestone 3 - Productivity Tools**: Client-side formatters and decoders (JSON, SQL, JWT, Base64).
* **Milestone 4 - API Playground**: HTTP request composer dashboard, request histories, and collections.
* **Milestone 5 - C# Engineering Challenges**: Compiler sandboxes, LINQ diagnostic challenges, and EF Core tuning puzzles.
* **Milestone 6 - Telemetry & Observability**: Moderation views, analytics tracking, and telemetry database logs.
* **Milestone 7 - Caching & Performance**: Redis integration, SQL index tuning, and load testing.
* **Milestone 8 - Production Release**: Cloud deployment configurations, SSL setups, and final build compilation.
