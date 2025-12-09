# Login API - Secure Authentication Service

A secure RESTful API for user authentication built with ASP.NET Core, featuring JWT token-based authentication, password hashing, and comprehensive audit logging.

## Features

- **Secure Authentication**: JWT token-based authentication with configurable expiration
- **Password Security**: Uses ASP.NET Core Identity's PasswordHasher (PBKDF2 with salt)
- **Audit Logging**: All login attempts are logged with timestamps and outcomes
- **Input Validation**: Comprehensive request validation with proper error responses
- **Clean Architecture**: Layered design with Controllers, Services, and Repositories

## Architecture

### Components

- **Controllers**: `AuthController` - handles HTTP POST /api/auth/login
- **Services**:
  - `AuthService` - authentication logic, password verification, JWT generation
  - `JwtTokenGenerator` - creates JWT tokens with configured claims and expiry
- **Repositories**:
  - `UserRepository` - user data access
  - `LoginAttemptRepository` - login attempt logging
- **Domain Services**:
  - `PasswordHasher<User>` - ASP.NET Core Identity's password hasher

### Data Models

#### User
```csharp
{
    "Id": "guid",
    "Email": "string (unique, max 256 chars)",
    "PasswordHash": "string",
    "IsActive": "bool (default true)",
    "CreatedAt": "DateTimeOffset"
}
```

#### LoginAttempt
```csharp
{
    "Id": "guid",
    "Email": "string",
    "Timestamp": "DateTimeOffset",
    "Success": "bool",
    "FailureReason": "string? (max 512 chars)"
}
```

## API Endpoints

### POST /api/auth/login

Authenticates a user with email and password.

**Request:**
```json
{
    "email": "user@example.com",
    "password": "SecurePassword123"
}
```

**Success Response (200 OK):**
```json
{
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "expiration": "2025-12-09T06:08:00.000Z"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid or missing input (e.g., malformed email, missing fields)
- `401 Unauthorized`: Invalid email or password
- `500 Internal Server Error`: Unexpected server errors

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- SQL Server (or LocalDB for development)

### Configuration

1. **Database Connection**: Update `appsettings.json` with your SQL Server connection string:
```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=LoginApiDb;Trusted_Connection=true;MultipleActiveResultSets=true"
    }
}
```

2. **JWT Configuration**: Configure JWT settings in `appsettings.json`:
```json
{
    "Jwt": {
        "Secret": "YOUR-SECRET-KEY-AT-LEAST-32-CHARACTERS-LONG",
        "Issuer": "LoginApi",
        "Audience": "LoginApiClient",
        "ExpirationMinutes": "60"
    }
}
```

⚠️ **Security Warning**: Never commit production secrets to source control. Use:
- **Development**: User Secrets (`dotnet user-secrets`)
- **Production**: Azure Key Vault or environment variables

### Database Setup

1. Navigate to the project directory:
```bash
cd src/LoginApi
```

2. Apply migrations to create the database:
```bash
dotnet ef database update
```

### Running the Application

```bash
cd src/LoginApi
dotnet run
```

The API will be available at `http://localhost:5000` (or the port configured in `launchSettings.json`).

### Creating Test Users

You can create test users by inserting records into the database. Here's an example using C#:

```csharp
var passwordHasher = new PasswordHasher<User>();
var user = new User
{
    Id = Guid.NewGuid(),
    Email = "test@example.com",
    PasswordHash = passwordHasher.HashPassword(null, "TestPassword123"),
    IsActive = true,
    CreatedAt = DateTimeOffset.UtcNow
};
// Add to database using your preferred method
```

## Testing

### Run Unit Tests

```bash
cd tests/LoginApi.Tests
dotnet test
```

The test suite includes:
- **AuthServiceTests**: 6 tests covering authentication logic
- **AuthControllerTests**: 3 tests covering API endpoints
- **JwtTokenGeneratorTests**: 2 tests covering token generation

All tests use Moq for mocking dependencies.

## Security Features

### Authentication
- JWT tokens with configurable expiration (default 60 minutes)
- Tokens signed with symmetric key (HS256 algorithm)
- Claims include user ID, email, and unique JWT ID

### Password Security
- Passwords hashed using ASP.NET Core's `PasswordHasher<User>`
- Uses PBKDF2 with salt by default
- Constant-time verification to mitigate timing attacks

### Input Validation
- Email format validation using `[EmailAddress]` attribute
- Required field validation using `[Required]` attribute
- Model state validation before processing requests

### Audit Logging
- All login attempts logged with:
  - Email address
  - Timestamp
  - Success/failure status
  - Failure reason (if applicable)
- Passwords never logged
- Logging failures don't block authentication flow

### Deployment Considerations
- HTTPS should be enforced at the infrastructure level
- JWT secrets must be stored securely (Key Vault, environment variables)
- Consider implementing rate limiting to prevent brute force attacks
- Monitor failed login attempts for security threats

## Project Structure

```
src/LoginApi/
├── Controllers/        # API controllers
├── Services/          # Business logic services
├── Repositories/      # Data access layer
├── Models/           # Domain entities
├── DTOs/             # Data transfer objects
├── Data/             # EF Core DbContext
└── Migrations/       # EF Core migrations

tests/LoginApi.Tests/
├── AuthServiceTests.cs
├── AuthControllerTests.cs
└── JwtTokenGeneratorTests.cs
```

## Building

```bash
dotnet build
```

## License

This project is part of a demonstration for secure authentication practices.