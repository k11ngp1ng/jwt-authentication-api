# JWT Authentication API

A RESTful authentication and authorization API built with **C#, ASP.NET Core, and .NET 10**, using **JSON Web Tokens (JWT)**.

This project demonstrates secure authentication practices, separation of concerns, role-based authorization, SQLite persistence, password hashing, automated testing, and a clean layered architecture.

## Features

- User registration with input validation
- Secure password hashing using BCrypt Enhanced with SHA-384 and work factor 12
- User authentication with JWT access tokens
- JWT claims for user ID, username, email, and role
- Authentication using `[Authorize]`
- Role-based authorization using `[Authorize(Roles = "Admin")]`
- Entity Framework Core with SQLite persistence
- Automatic database migration on application startup
- Standardized error responses using RFC 7807 `ProblemDetails`
- OpenAPI/Swagger documentation with Bearer authentication
- Unit and HTTP integration tests

## Architecture

The project follows a layered architecture with clear separation of responsibilities:

```text
src/
├── JwtAuthenticationApi.Api
│   ├── Controllers
│   └── Middleware
├── JwtAuthenticationApi.Application
│   ├── DTOs
│   ├── Exceptions
│   ├── Interfaces
│   └── Services
├── JwtAuthenticationApi.Domain
│   ├── Entities
│   └── Enums
└── JwtAuthenticationApi.Infrastructure
    ├── Authentication
    ├── Persistence
    └── Repositories

tests/
└── JwtAuthenticationApi.Tests
    ├── Unit
    └── Integration
```

### Layer Dependencies

```text
Api ───────────► Application
 │                    │
 └──► Infrastructure  └──► Domain
          │
          └───────────────► Domain
```

The **Domain** layer contains the core business entities and enums.

The **Application** layer contains application logic, DTOs, interfaces, services, and application-specific exceptions.

The **Infrastructure** layer implements persistence, repositories, and authentication-related services.

The **API** layer exposes HTTP endpoints and handles incoming requests, authentication, authorization, and middleware.

## Tech Stack

- .NET 10
- ASP.NET Core
- Entity Framework Core 10
- SQLite
- JWT Bearer Authentication
- BCrypt.Net-Next
- Swashbuckle / OpenAPI
- xUnit

## Prerequisites

Make sure the following tools are installed:

- .NET 10 SDK
- Git

Trust the ASP.NET Core HTTPS development certificate:

```powershell
dotnet dev-certs https --trust
```

## Configuration

Restore the project dependencies and local tools:

```powershell
dotnet restore
dotnet tool restore
```

### JWT Secret

The JWT signing key should **never be stored in the repository**.

For local development, generate a cryptographically secure random key and store it using .NET User Secrets:

```powershell
$jwtSecret = [Convert]::ToBase64String(
    [Security.Cryptography.RandomNumberGenerator]::GetBytes(64)
)

dotnet user-secrets set "Jwt:SecretKey" $jwtSecret `
    --project .\src\JwtAuthenticationApi.Api
```

The remaining configuration is defined in:

```text
src/JwtAuthenticationApi.Api/appsettings.json
```

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=jwt-auth.db"
  },
  "Jwt": {
    "Issuer": "JwtAuthenticationApi",
    "Audience": "JwtAuthenticationApi.Client",
    "SecretKey": "",
    "ExpirationMinutes": 60
  }
}
```

For production environments, provide `Jwt__SecretKey` through an environment variable or a secure secrets management service.

## Running the Application

Start the API using:

```powershell
dotnet run --project .\src\JwtAuthenticationApi.Api --launch-profile https
```

Swagger UI will be available at:

```text
https://localhost:7035/swagger
```

The SQLite database and its tables are automatically created when the application starts.

To apply Entity Framework Core migrations manually:

```powershell
dotnet ef database update `
    --project .\src\JwtAuthenticationApi.Infrastructure `
    --startup-project .\src\JwtAuthenticationApi.Api
```

## API Endpoints

| Method | Endpoint | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Registers a new user and returns a JWT |
| `POST` | `/api/auth/login` | Public | Authenticates a user and returns a JWT |
| `GET` | `/api/protected/user` | Bearer Token | Accessible to authenticated users |
| `GET` | `/api/protected/admin` | Admin Role | Accessible only to administrators |

Newly registered users are always assigned the `User` role.

Promotion to the `Admin` role should only be performed through a trusted administrative process and must never be exposed through the public registration endpoint.

## Usage Examples

### Register a User

```http
POST /api/auth/register
Content-Type: application/json

{
  "username": "demo_user",
  "email": "demo@example.com",
  "password": "StrongPassword@123"
}
```

### Login

```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "demo@example.com",
  "password": "StrongPassword@123"
}
```

Example response:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresAtUtc": "2026-09-13T19:00:00+00:00"
}
```

Use the returned access token when accessing protected endpoints:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

A ready-to-use HTTP request collection is available at:

```text
requests/JwtAuthenticationApi.http
```

## Input Validation

User registration requires:

- Username between 3 and 50 characters
- Username containing only letters, numbers, or underscores (`_`)
- Valid email address with a maximum length of 254 characters
- Password between 8 and 128 characters
- Password containing at least:
  - one uppercase letter
  - one lowercase letter
  - one number
  - one special character

Invalid credentials return a generic authentication error to prevent revealing whether a specific email address is registered.

## Testing

Run the complete test suite with:

```powershell
dotnet test --configuration Release
```

The test suite covers:

- DTO validation
- JWT generation and claims
- BCrypt password hashing and verification
- Registration and authentication services
- Repository behavior using a real in-memory SQLite database
- Controllers and authorization metadata
- Complete HTTP authentication flows
- HTTP `401 Unauthorized` responses
- HTTP `403 Forbidden` responses
- HTTP `409 Conflict` responses

## Security

The project implements several security practices:

- Passwords are never stored in plain text
- Passwords are hashed using BCrypt Enhanced with SHA-384
- JWT signing keys are kept outside the source code
- Tokens validate signature, issuer, audience, and expiration
- JWT expiration tolerance (`ClockSkew`) is set to zero
- Email addresses and usernames have unique database indexes
- Public registration cannot assign privileged roles
- Authentication errors do not reveal whether an account exists
- Internal errors do not expose stack traces to API clients
- Administrative access is protected through role-based authorization

## Project Goals

This project was created to demonstrate practical implementation of:

- RESTful API design
- JWT authentication
- Role-based authorization
- Secure password storage
- Layered application architecture
- Dependency injection
- Entity Framework Core
- Database persistence
- API documentation
- Automated testing
- Security-oriented backend development

## License

This project currently does not include a license.

Unless a license is added, the source code should not be assumed to be available for unrestricted reuse or redistribution.
