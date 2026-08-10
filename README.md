# DevForge

DevForge is a serious, production-grade developer platform designed to centralize core tools, request playarounds, and advanced .NET engineering challenges into a single extensible ecosystem. This repository serves as an open-source, Clean Architecture-aligned workspace demonstrating professional C#, ASP.NET Core 10, React, and Docker deployments.

---

## Vision
DevForge aims to bridge the gap between developer productivity utilities and technical skills growth:
1. **Developer Tools**: Fast, client-side developer tools (JSON Formatter, JWT Decoder, SQL Formatter, Base64 Encoder, HL7 Validator, GUID Generator).
2. **API Playground**: A complete HTTP request builder (supporting GET/POST/PUT/PATCH/DELETE, headers, params, and collections) acting as a lightweight, browser-based alternative to Postman.
3. **.NET Interview Challenges**: Comprehensive evaluations covering C#, ASP.NET Core, EF Core, LINQ, SQL Server, security patterns, Azure integrations, and system design.
4. **User Platform**: Secure account registration, JWT session management, Refresh Token rotation, and role-based preferences.

---

## Current Status
* **Week 1 Foundation** has been successfully established and verified. 
* All core system structures, global exception logging, versioned API pipelines, CORS layers, SQL Server EF Core configurations, automated test suites, Docker container orchestrations, and GitHub Actions pipelines are active.
* Business features (Authentication, Dev Tools, API Playground, Challenges) are scheduled for subsequent milestones.

---

## Technology Stack

### Backend
* **Language/Runtime**: C# 10 / .NET 10.0
* **API Framework**: ASP.NET Core Web API (Controllers)
* **Data Access**: Entity Framework Core 10.0 (SQL Server)
* **API Documentation**: Swagger / Swashbuckle OpenAPI
* **Structured Logging**: Serilog (Console & File rolling sinks)
* **Validation**: FluentValidation
* **Diagnostics**: ASP.NET Core Health Checks

### Frontend
* **Runtime/Bundler**: Node.js & Vite
* **Library/Language**: React 19, TypeScript
* **Styling**: Tailwind CSS (v4)
* **Icons**: Lucide React

### Testing & DevOps
* **Test Engines**: xUnit, FluentAssertions, `Microsoft.AspNetCore.Mvc.Testing`
* **Containerization**: Docker, Docker Compose (SQL Server 2022, Web API, React Nginx)
* **CI/CD**: GitHub Actions

---

## Project Structure

```
DevForge/
├── src/
│   ├── DevForge.API/            # Presentation Layer (Controllers, Middlewares, Program)
│   ├── DevForge.Application/    # Application Logic (Interfaces, Result models, Exceptions, DI)
│   ├── DevForge.Domain/         # Core Domain Layer (Base Entity, AuditableEntity, System Models)
│   ├── DevForge.Infrastructure/ # Data Layer (ApplicationDbContext, Configurations, DI, Migrations)
│   └── DevForge.Web/            # React & TS Vite SPA (Tailwind CSS v4)
├── tests/
│   ├── DevForge.UnitTests/      # xUnit Unit tests for Domain/Application helpers
│   └── DevForge.IntegrationTests/ # xUnit WebApplicationFactory Integration tests
├── docs/
│   ├── architecture/            # Architectural flow documentation & Mermaid diagrams
│   └── api/                     # Endpoint query lists & mock response templates
├── docker-compose.yml           # Orchestration for SQL Server, Web API, and Nginx Web
└── DevForge.slnx                 # .NET Solution File
```

---

## Local Development

### Prerequisites
* .NET 10.0 SDK
* Node.js v20+ & npm
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

### Running the Frontend
1. Navigate to the Web app folder:
   ```bash
   cd src/DevForge.Web
   ```
2. Install npm packages:
   ```bash
   npm install
   ```
3. Copy environment configuration:
   ```bash
   cp .env.example .env
   ```
4. Start the local Vite server:
   ```bash
   npm run dev
   ```
5. Open your browser to `http://localhost:5173`. The application shell will automatically try to connect to the backend status API.

---

## Running with Docker
You can run the entire platform (SQL Server database, backend API, and React frontend) inside Docker containers:
1. Run Docker Compose build and start:
   ```bash
   docker-compose up --build
   ```
2. Open:
   * **React Frontend**: `http://localhost:8080` (Served via Nginx)
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

* **Week 1 - Foundation** *(Completed)*: System architecture, EF Core configuration, global filters, Swagger versioning, Dockerization, and CI.
* **Week 2 - Authentication & RBAC**: User registrations, JWT logins, Refresh Token rotations, roles and permissions.
* **Week 3 - Developer Tools**: Release client-side formats (JSON, SQL, JWT, Base64).
* **Week 4 - API Playground**: HTTP requesting dashboard with header inputs, parameter variables, and saved histories.
* **Week 5 - .NET Interview Challenge**: C# compiler playgrounds, LINQ diagnostics, and MVC architecture challenges.
* **Week 6 - Admin & Analytics**: Moderation views, analytics tracking, and telemetry database logs.
* **Week 7 - Performance, Security & Testing**: Redis caching, SQL indexing, load tests, and security scanning.
* **Week 8 - Docker, CI/CD & Production Release**: Final cloud deployments, telemetry dashboards, and release compilation.
