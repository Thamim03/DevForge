# DevForge - Internal Development Roadmap

This document outlines the 8-week milestone schedule for the development of the **DevForge** developer platform.

---

## Weekly Milestones

### Week 1 - Architecture & Foundation (Current Milestone)
* **Goal**: Setup clean solution architecture, logging, database integration, global error handling, and container configurations.
* **Deliverables**:
  * 5-project Clean Architecture solution.
  * EF Core configuration supporting SQL Server and in-memory SQLite.
  * Global exception handling middleware (`ProblemDetails`).
  * Swashbuckle dynamic Swagger UI document generator.
  * Serilog file and console logging.
  * xUnit unit/integration testing framework.
  * Dockerfile configurations for API and Web projects, and `docker-compose` files.
  * GitHub Actions CI pipelines.

### Week 2 - Authentication & RBAC
* **Goal**: Establish secure user accounts and role-based permissions filtering.
* **Deliverables**:
  * User, Role, and Permission tables schema.
  * Registration and JWT-based Login endpoints.
  * Refresh Token rotation and session revocations.
  * ASP.NET Core Policy-based authorization middleware.

### Week 3 - Developer Tools Module
* **Goal**: Implement client-side developer utility pages.
* **Deliverables**:
  * JSON Formatter & Schema Validator.
  * JWT Decoder, Header, and Payload Inspector.
  * SQL Beautifier & Query Formatter.
  * Base64 Encoder / Decoder and HL7 Parser.

### Week 4 - API Playground
* **Goal**: Build an interactive HTTP client sandbox.
* **Deliverables**:
  * REST request composer (HTTP method verbs, headers, body formatting).
  * Request History log (database-backed tracking).
  * Query Collection Folders structure.

### Week 5 - .NET Engineering Challenges
* **Goal**: Design sandboxed programming exercises.
* **Deliverables**:
  * Sandboxed compilation runner for C# / LINQ code blocks.
  * Performance tuning assignments (EF Core query optimization, SQL indexing).
  * Automated testing and scoring metrics.

### Week 6 - Admin & Telemetry Analytics
* **Goal**: Create moderate logs and dashboard analytics.
* **Deliverables**:
  * Platform moderation views (user control, challenge management).
  * System event tracking (logging api requests and platform usage statistics).

### Week 7 - Caching, Hardening & Tuning
* **Goal**: Perform security audits and caching optimizations.
* **Deliverables**:
  * Redis distributed caching integration.
  * SQL Indexing and stored procedures tuning.
  * Security scanners audits.

### Week 8 - Cloud Deployment & Final Release
* **Goal**: Prepare production deployment configurations.
* **Deliverables**:
  * Multi-container setup ready for cloud provider deployment.
  * Telemetry monitoring setup.
  * Production build compilation and release tag.
