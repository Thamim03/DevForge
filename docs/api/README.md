# DevForge API Documentation

This document describes the foundational endpoints implemented for testing, health monitoring, and system checks in **DevForge**.

---

## Base URLs

* **Development (Direct)**:
  * HTTPS: `https://localhost:7172`
  * HTTP: `http://localhost:5057`

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
Returns metadata about the application host state.

* **URL**: `/api/system/status`
* **Method**: `GET`
* **Auth Required**: No
* **Headers**: None

#### Responses

##### Success
* **Status**: `200 OK`
* **Content-Type**: `application/json`
* **Response Body**:
  ```json
  {
    "application": "DevForge",
    "status": "Healthy"
  }
  ```

---

## Global Error Responses

DevForge adheres to the **RFC 7807 Problem Details** standard for consistent API error responses.

### Server Exception Example
If an unhandled exception occurs on the server, the exception is intercepted by the global error middleware, structured, logged, and formatted without leaking sensitive code stack traces or database errors.

* **Status**: `500 Internal Server Error`
* **Content-Type**: `application/problem+json`
* **Response Body**:
  ```json
  {
    "type": "https://tools.ietf.org/html/rfc7231#section-6.6.1",
    "title": "Internal Server Error",
    "status": 500,
    "detail": "An unexpected error occurred on the server. Please contact support.",
    "instance": "/api/system/status",
    "traceId": "0HN18A5G4B211:00000001"
  }
  ```
