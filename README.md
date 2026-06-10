# Motsos Car Rentals

## Live Deployments

| | Link |
|---|---|
| **Frontend** | https://motsos-car-rentals.vercel.app/ |
| **Backend API** | https://car-rental-api-elm2.onrender.com |

---

A RESTful Car Rental backend built with **ASP.NET Core 10** and **PostgreSQL**. It covers the full rental lifecycle — from customer self-registration through vehicle browsing, rental requests, employee approval/rejection, and return — with role-based access control, structured logging, and Docker deployment.

---

## Tech Stack

| Concern | Library / Tool |
|---|---|
| Framework | ASP.NET Core 10 (Minimal hosting model) |
| ORM | Entity Framework Core 10 — PostgreSQL provider (Npgsql) |
| Authentication | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer 10`) |
| Password hashing | BCrypt.Net-Next 4 |
| Object mapping | AutoMapper 16 |
| Logging | Serilog — Console (compact JSON) + File sinks |
| API documentation | Swashbuckle / Swagger UI with XML comments |
| Containerisation | Docker (multi-stage build) + Docker Compose |
| Testing | xUnit v3 + NSubstitute + EF Core InMemory |

---

## Architecture

The solution follows a **layered architecture**:

```
Controllers  →  IApplicationService (facade)  →  Individual Services  →  Unit of Work  →  Repositories  →  EF Core
```

- **`IApplicationService`** is a single injectable facade that groups all domain services (`VehicleService`, `RentalService`, `CustomerService`, `EmployeeService`, `UserService`, `LookupService`), keeping controller constructors clean.
- **Unit of Work** wraps all repositories and a single `SaveChanges()` call, ensuring that multi-step operations (e.g. create rental + update vehicle status) are committed atomically.
- **UUID vs. internal ID** — all entities carry both a surrogate `int Id` (used internally and in FK joins) and a `Guid Uuid` (exposed in the API). External callers always use UUIDs, never integer IDs.
- **Soft deletes** — `BaseEntity` includes `IsDeleted`, `DeletedAt`, `InsertedAt`, `ModifiedAt`. Deleting a resource sets `IsDeleted = true`; hard deletes do not happen.

---

## Roles

| Role | How obtained | Key permissions |
|---|---|---|
| `ADMIN` | Seeded / provisioned out-of-band | Full CRUD on vehicles, employees, customers; approve/reject rentals |
| `EMPLOYEE` | Registered by an Admin (`POST /auth/register/employee`) | Add vehicles, upload photos, approve/reject rentals, view all data |
| `CUSTOMER` | Self-registers (`POST /auth/register`) | Browse available vehicles, create rental requests, view own history |

---

## Rental Workflow

A rental moves through the following states:

```
[Customer] POST /rentals  →  Pending
                                 │
              ┌──────────────────┴──────────────────┐
              ▼                                      ▼
[Employee] Approved                          [Employee] Rejected
   (cost calculated)                      (vehicle → Available)
              │
              ▼
[Employee] Returned  →  vehicle → Available
```

**Business rules enforced by the service layer:**

- The vehicle must have `VehicleStatus.Available` and `IsDeleted = false` to accept a new rental.
- Dates are validated: `StartDate` must be ≥ today (UTC) and `EndDate` must be > `StartDate`.
- Overlapping rentals for the same vehicle are rejected.
- On **Approval**, `TotalCost` is calculated as `(EndDate − StartDate) days × DailyRate`.
- On **Rejection or Return**, the vehicle status reverts to `Available`.
- Employees must be identified by their UUID when approving/rejecting.
- A vehicle with active (non-rejected, non-returned) rentals **cannot be soft-deleted**.

---

## API Reference

All routes are prefixed with `/api/v1`. All list endpoints return a `PaginatedResult<T>` (see [Pagination](#pagination)). All error responses conform to [RFC 7807](#error-responses).

### Auth — `/auth`

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/auth/register` | Public | Self-register a Customer account |
| `POST` | `/auth/register/employee` | ADMIN | Create an Employee account |
| `POST` | `/auth/login` | Public | Authenticate — returns a signed JWT |

### Vehicles — `/vehicles`

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/vehicles` | All | Paginated, filtered list. Customers see only `Available` vehicles; Admins/Employees see all statuses |
| `GET` | `/vehicles/{uuid}` | All | Single vehicle by UUID |
| `POST` | `/vehicles` | ADMIN, EMPLOYEE | Add a vehicle (must provide a valid `categoryId`) |
| `POST` | `/vehicles/{uuid}/photo` | ADMIN, EMPLOYEE | Upload or replace the vehicle photo (`multipart/form-data`) |
| `PUT` | `/vehicles/{uuid}` | ADMIN | Full update — duplicate license plate check is enforced |
| `DELETE` | `/vehicles/{uuid}` | ADMIN | Soft-delete — blocked if any active rental exists |

**Vehicle filters (query string)**

| Parameter | Type | Description |
|---|---|---|
| `search` | `string` | Free-text match on Make or Model |
| `licensePlate` | `string` | Partial match |
| `make` | `string` | Exact match |
| `model` | `string` | Exact match |
| `minYear` / `maxYear` | `short` | Year range |
| `minDailyRate` / `maxDailyRate` | `decimal` | Daily rate range |
| `status` | `Available \| Rented \| Maintenance` | Current status (Admin/Employee only) |
| `tierType` | `Economy \| Standard \| Luxury \| VIP` | Tier filter |
| `categoryId` | `int` | Filter by category |

### Rentals — `/rentals`

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/rentals` | CUSTOMER | Create a rental request (status starts as `Pending`) |
| `PATCH` | `/rentals/{uuid}` | ADMIN, EMPLOYEE | Approve, reject, or mark as returned |
| `GET` | `/rentals` | ADMIN, EMPLOYEE | Paginated list of all rentals |
| `GET` | `/rentals/rental-history` | CUSTOMER | Caller's own rental history |

**Rental filters (query string)**

| Parameter | Type | Description |
|---|---|---|
| `status` | `Pending \| Approved \| Rejected \| Returned` | Current rental status |
| `vehicleUuid` | `Guid` | Filter by vehicle |
| `customerUuid` | `Guid` | Filter by customer (Admin/Employee endpoint) |
| `employeeUuid` | `Guid` | Filter by handling employee |
| `startDateFrom` / `startDateTo` | `DateOnly` | Rental start date range |
| `minTotalCost` / `maxTotalCost` | `decimal` | Approved cost range |

### Customers — `/customers`

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/customers` | ADMIN, EMPLOYEE | Paginated, filtered customer list |
| `GET` | `/customers/{uuid}` | ADMIN, EMPLOYEE; CUSTOMER (own) | Single customer by UUID |
| `PUT` | `/customers/{uuid}` | ADMIN; CUSTOMER (own) | Update customer details |
| `DELETE` | `/customers/{uuid}` | ADMIN | Soft-delete customer |

**Customer filters:** `username`, `firstname`, `lastname`, `email`, `driverLicenceNumber`

### Employees — `/employees`

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/employees` | ADMIN | Paginated employee list |
| `GET` | `/employees/{uuid}` | ADMIN; EMPLOYEE (own) | Single employee by UUID |
| `PUT` | `/employees/{uuid}` | ADMIN; EMPLOYEE (own) | Update employee details |
| `DELETE` | `/employees/{uuid}` | ADMIN | Soft-delete employee |

**Employee filters:** `username`, `firstname`, `lastname`, `email`

### Lookup — `/locations`

| Method | Path | Auth | Description |
|---|---|---|---|
| `GET` | `/locations` | All | List all pick-up / drop-off locations |

---

## Pagination

Every list endpoint accepts `pageNumber` (default `1`) and `pageSize` (default `10`) as query parameters and returns:

```json
{
  "data": [...],
  "totalRecords": 42,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

---

## Error Responses

All errors are returned as [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) `application/problem+json`. Expected application errors also include a machine-readable `code` extension.

```json
{
  "status": 404,
  "title": "Resource not found",
  "detail": "The requested Vehicle was not found.",
  "instance": "/api/v1/vehicles/some-uuid",
  "type": "https://httpstatuses.io/404",
  "traceId": "0HN4M2K...",
  "code": "NOT_FOUND"
}
```

| Exception | HTTP Status |
|---|---|
| `EntityNotFoundException` | 404 Not Found |
| `EntityAlreadyExistsException` | 409 Conflict |
| `InvalidArgumentException` | 400 Bad Request |
| `EntityNotAuthorizedException` | 401 Unauthorized |
| `EntityForbiddenException` | 403 Forbidden |
| Any unhandled exception | 500 Internal Server Error (detail hidden) |

---

## Logging

Serilog is used throughout. The **`MDCLoggingMiddleware`** enriches every log entry with the authenticated `User` and client `IP`, making per-request traces easy to follow. In Development, logs are written as compact JSON to the console. The configuration for additional sinks (e.g. file) is defined in `appsettings.json`.

---

## Testing

The `CarRentalTests` project is an xUnit v3 test suite that covers services, repositories, and controllers.

| Concern | Library |
|---|---|
| Test framework | xUnit v3 (`xunit.v3`) |
| Mocking | NSubstitute 5 |
| In-memory database | EF Core InMemory provider |
| Coverage | coverlet.collector |

**Test structure**

| Folder | What is tested |
|---|---|
| `Services/` | `VehicleService`, `RentalService`, `CustomerService` — business logic in isolation |
| `Repositories/` | `VehicleRepository` — EF Core queries against an in-memory database |
| `Controller/` | `AuthController` — request/response handling |
| `Helper/` | `TestDbContextFactory` — shared in-memory `AppDbContext` setup |

**Run the tests**

```bash
dotnet test CarRentalTests
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 17, or Docker (to run the full stack)

### Local development (no Docker)

1. Fill in `CarRentalApp/appsettings.Development.json` with your connection string, JWT secret, and CORS origin.
2. Apply EF Core migrations:
   ```bash
   dotnet ef database update --project CarRentalApp
   ```
3. Run the API:
   ```bash
   dotnet run --project CarRentalApp
   ```
4. Browse Swagger UI at `https://localhost:7220/swagger`.

### Docker Compose (full stack)

1. Copy `.env.example` to `.env` and set all required values (see the table below).
2. Start both the PostgreSQL container and the API:
   ```bash
   docker compose up --build
   ```
   The API is available on the port defined by `APP_PORT` (default **8081**).

> The `webapp` service waits for the PostgreSQL health check to pass before starting. EF Core migrations run automatically on startup.

---

## Environment Variables

| Variable | Required | Default | Description |
|---|---|---|---|
| `JWT_SECRET` | Yes | — | HS256 signing key — use a long random string |
| `JWT_ISSUER` | No | `https://localhost:8081` | `iss` claim in the token |
| `JWT_AUDIENCE` | No | `https://localhost:8081` | `aud` claim in the token |
| `CORS_ORIGIN` | No | `http://localhost:3000` | Allowed frontend origin |
| `DB_HOST` | No | `postgres` | PostgreSQL hostname |
| `DB_PORT` | No | `5432` | Host port mapped to PostgreSQL's 5432 |
| `DB_NAME` | No | `CarRentalDB` | Target database name |
| `DB_USER` | Yes | — | Application database user |
| `DB_USER_PASSWORD` | Yes | — | Application database user password |
| `APP_PORT` | No | `8081` | Host port mapped to the API container |
| `ASPNETCORE_ENVIRONMENT` | No | `Production` | ASP.NET Core environment name |

> **Never commit `.env` to version control.** The `.env` file contains credentials and is git-ignored.

---

## Localisation

Error messages support **English** (`en`) and **Greek** (`el`). The active locale is picked from the `Accept-Language` request header; the default is `en`.

---

## License

MIT
