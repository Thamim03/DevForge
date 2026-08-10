# DevForge

DevForge is a clean, extensible platform designed to centralize core developer utilities, HTTP playgrounds, and advanced .NET engineering challenges into a single cohesive workspace.

---

## Overview
DevForge is built to provide practical developer tools and sandboxed diagnostic suites running under a production-oriented architectural foundation. It is structured to help developers validate payloads, inspect HTTP requests, and analyze advanced C# coding constructs inside a unified workspace.

---

## Architecture
The platform is designed following **Clean Architecture** patterns to enforce strict separation of concerns and maintain a clean inward dependency flow:

```
DevForge.Web (MVC Layout) & DevForge.API (REST Controller endpoints)
    │
    ▼
DevForge.Application (Logical Use Cases / FluentValidation / Custom Result Wrappers)
    │
    ▼
DevForge.Domain (Core Entities / Auditable Models / Base Contracts)
    ▲
    │
DevForge.Infrastructure (Entity Framework Core / SQL Server persistence)
```
* **Separation of Presentation**: The MVC project serving Razor views is kept thin and delegates execution to the Application layer.
* **API Independence**: A separate REST Web API layer is maintained to support future third-party, client-side, or mobile integrations.

---

## Technology Stack

### Backend
* **Language/Runtime**: C# / .NET 10.0
* **API Framework**: ASP.NET Core Web API (Controllers)
* **Data Access**: Entity Framework Core 10.0 (SQL Server)
* **API Documentation**: Swagger / Swashbuckle OpenAPI
* **Structured Logging**: Serilog (Console & File rolling sinks)
* **Validation**: FluentValidation
* **Diagnostics**: ASP.NET Core Health Checks

### Frontend
* **Server Rendering**: ASP.NET Core MVC (Model-View-Controller) & Razor Views
* **Interactions**: HTML5, Vanilla CSS, Modular JavaScript, and native Fetch API

### Testing & DevOps
* **Test Engines**: xUnit, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`
* **Containerization**: Docker & Docker Compose

---

## Features
The current Week 1 release establishes the complete engineering foundation of the platform:
* **Active Status Health Checks**: A background system health monitor endpoint (`/health`) verifying SQL Server connectivity.
* **Dynamic Connection Monitoring**: A status diagnostics client (`/api/v1/system/status`) recording transaction counts and latency.
* **Self-Healing Environment Resolution**: The frontend script automatically detects the host port (IIS Express vs Kestrel profiles) to configure communication parameters seamlessly.
* **Unified Diagnostics Exceptions**: Global ProblemDetails mapping yielding structured JSON payloads on all pipeline failures.

---

## Getting Started

### Prerequisites
* .NET 10.0 SDK
* SQL Server instance or Docker Desktop

### Running the API
1. Navigate to the API project folder:
   ```bash
   cd src/DevForge.API
   ```
2. Start the Web API:
   ```bash
   dotnet run
   ```
3. Access the API Swagger UI at: `https://localhost:7172/swagger/index.html` or `http://localhost:5057/swagger/index.html`.

### Running the MVC Frontend
1. Navigate to the Web app folder:
   ```bash
   cd src/DevForge.Web
   ```
2. Start the MVC Web application:
   ```bash
   dotnet run
   ```
3. Open your browser to `http://localhost:5251` or `https://localhost:7246`.

---

## Configuration
To protect credentials, connection settings should not be committed to source control with real passwords.

### 1. Default Connection String
Default development settings are defined in the project configuration files (`appsettings.Development.json`):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=DevForge;User Id=sa;Password=YourLocalPassword123!;TrustServerCertificate=True;"
}
```

### 2. Managing Secrets Safely
To override the default password on your local development machine without editing repository files, use the **.NET User Secrets Tool**:
1. Initialize secrets in the startup projects:
   ```bash
   dotnet user-secrets init --project src/DevForge.API
   dotnet user-secrets init --project src/DevForge.Web
   ```
2. Set your actual SQL Server connection string:
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=YOUR_SERVER;Database=DevForge;Persist Security Info=True;User ID=sa;Password=YOUR_ACTUAL_PASSWORD;Encrypt=False;TrustServerCertificate=True;Command Timeout=0;" --project src/DevForge.API
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=YOUR_SERVER;Database=DevForge;Persist Security Info=True;User ID=sa;Password=YOUR_ACTUAL_PASSWORD;Encrypt=False;TrustServerCertificate=True;Command Timeout=0;" --project src/DevForge.Web
   ```
This stores your credentials in your local user profile directory, keeping them completely out of Git history.

---

## Running Tests
Run all unit and integration test suites using the .NET CLI:
```bash
dotnet test
```

---

## Running with Docker
Run the entire platform inside Docker containers:
1. Start Orchestration:
   ```bash
   docker-compose up --build
   ```
2. Open:
   * **DevForge MVC Web App**: `http://localhost:8080`
   * **Web API Swagger**: `http://localhost:5000/swagger/index.html`

---

## Development Roadmap
The following milestones outline the incremental implementation plan:
* **Week 1 - Foundation** *(Completed)*: Clean Architecture solution, EF Core SQL Server mapping, logging, error filters, Swagger versioning, Dockerization, and CI.
* **Week 2 - Authentication & RBAC**: Account registration, JWT logins, Refresh Token rotations, roles and permissions.
* **Week 3 - Developer Tools**: JSON, SQL, JWT, and Base64 formatters and parsers.
* **Week 4 - API Playground**: HTTP requesting compose sandbox, history logs, and query folder collections.
* **Week 5 - .NET Engineering Challenge**: Sandboxed C# coding tasks and EF Core index tuning exercises.
* **Week 6 - Admin & Observer Logs**: Moderation screens, analytics trackers, and event telemetry databases.
* **Week 7 - Performance & Caching**: Redis caching configurations and database query tuning.
* **Week 8 - Release Compilation**: Cloud deployments configurations and monitoring systems.
