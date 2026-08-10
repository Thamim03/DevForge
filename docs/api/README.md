# DevForge API Documentation

This document describes the foundational endpoints implemented for testing, health monitoring, and system checks in **DevForge**.

---

## Base URLs

* **Development (Direct)**:
  * HTTPS: `https://localhost:7172`
  * HTTP: `http://localhost:5057`
* **Docker Compose**:
  * API Endpoint: `http://localhost:5000`

---

## Endpoints

### 1. API Health Check
Verifies that the API application is running and can successfully ping its underlying database.

* **URL**: `/health`
* **Method**: `GET`
* **Auth Required**: No
* **Headers**: None

#### Responses

##### Success (Healthy)
* **Status**: `200 OK`
* **Content-Type**: `text/plain`
* **Response Body**:
  ```text
  Healthy
  ```

##### Failure (Unhealthy)
If the database or core system services fail, this endpoint returns an unhealthy status code.
* **Status**: `503 Service Unavailable`
* **Content-Type**: `text/plain`
* **Response Body**:
  ```text
  Unhealthy
  ```

---

### 2. System Status
Returns metadata about the application host state and performs a test query to verify database read/write permissions.

* **URL**: `/api/v1/system/status`
* **Method**: `GET`
* **Auth Required**: No
* **Headers**: None

#### Responses

##### Success (Database Online)
* **Status**: `200 OK`
* **Content-Type**: `application/json`
* **Response Body**:
  ```json
  {
    "status": "ok",
    "application": "DevForge",
    "version": "1.0.0",
    "databaseConnection": "Connected",
    "totalStatusChecks": 42
  }
  ```

##### Partial Success (Database Offline)
If the API host is running but the SQL Server database cannot be accessed (e.g., connection timed out or database not migrated yet), the API handles this exception and reports the error gracefully.
* **Status**: `200 OK`
* **Content-Type**: `application/json`
* **Response Body**:
  ```json
  {
    "status": "ok",
    "application": "DevForge",
    "version": "1.0.0",
    "databaseConnection": "Offline",
    "databaseError": "A network-related or instance-specific error occurred while establishing a connection..."
  }
  ```

---

## Global Error Responses

DevForge adheres to the **RFC 7807 Problem Details** standard for consistent API error responses.

### Server Exception Example
If an unhandled exception occurs on the server, the exception is intercepted by the global error middleware, structured, logged, and formatted without leaking sensitive code stack traces.

* **Status**: `500 Internal Server Error`
* **Content-Type**: `application/problem+json`
* **Response Body**:
  ```json
  {
    "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    "title": "Internal Server Error",
    "status": 500,
    "detail": "An unexpected error occurred on the server. Please contact support.",
    "instance": "/api/v1/system/status",
    "traceId": "0HN18A5G4B211:00000001"
  }
  ```

### Validation Error Example
When input parameters fail validation, the system returns a standard validation problem layout containing the specific property-level error logs.

* **Status**: `400 Bad Request`
* **Content-Type**: `application/problem+json`
* **Response Body**:
  ```json
  {
    "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
    "title": "Validation Error",
    "status": 400,
    "detail": "One or more validation failures occurred.",
    "instance": "/api/v1/user/register",
    "traceId": "0HN18A5G4B211:00000002",
    "errors": {
      "Email": [
        "'Email' is not a valid email address.",
        "'Email' must not be empty."
      ],
      "Password": [
        "The password must contain at least one uppercase letter."
      ]
    }
  }
  ```
