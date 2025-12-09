# LoginApi - Secure Authentication API

A secure ASP.NET Core Web API implementing JWT-based authentication with comprehensive security features.

## Features

- **Secure Login API** with email and password authentication
- **JWT Token Generation** with configurable expiration
- **Password Hashing** using BCrypt
- **Login Attempt Logging** with IP address and user agent tracking
- **Brute Force Protection** with rate limiting (5 failed attempts per 15 minutes)
- **Account Lockout** for inactive users
- **Input Validation** to prevent injection attacks
- **HTTPS Enforcement** for secure communication

## Architecture

- **Backend**: ASP.NET Core 10.0 Web API
- **Database**: SQL Server via Entity Framework Core
- **Authentication**: JWT Bearer tokens
- **Password Hashing**: BCrypt.Net-Next

## Prerequisites

- .NET 10.0 SDK
- SQL Server or SQL Server LocalDB

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/tahir-ansari-nv/app-repo.git
cd app-repo
```

### 2. Configure Database

Update the connection string in `src/LoginApi/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=LoginApiDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

### 3. Apply Migrations

```bash
cd src/LoginApi
dotnet ef database update
```

### 4. Configure JWT Settings

Update JWT settings in `src/LoginApi/appsettings.json` (use secure secrets in production):

```json
"JwtSettings": {
  "Secret": "YOUR_SECURE_SECRET_KEY_AT_LEAST_32_CHARACTERS",
  "Issuer": "LoginApi",
  "Audience": "LoginApiUsers",
  "ExpirationMinutes": 60
}
```

**Important**: In production, store the JWT secret in a secure location like Azure Key Vault or Secret Manager.

#### Using Azure Key Vault (Production)

```bash
# Add Azure Key Vault configuration package
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets

# Configure in Program.cs
var keyVaultUri = new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/");
builder.Configuration.AddAzureKeyVault(keyVaultUri, new DefaultAzureCredential());
```

#### Using User Secrets (Development)

```bash
# Initialize user secrets for the project
cd src/LoginApi
dotnet user-secrets init

# Set the JWT secret
dotnet user-secrets set "JwtSettings:Secret" "YOUR_DEVELOPMENT_SECRET_KEY"
```

#### Using Environment Variables

```bash
# Set environment variable (Linux/macOS)
export JwtSettings__Secret="YOUR_SECRET_KEY"

# Set environment variable (Windows)
set JwtSettings__Secret=YOUR_SECRET_KEY
```

### 5. Run the Application

```bash
dotnet run
```

The API will be available at `https://localhost:7074` (or the port specified in `launchSettings.json`).

## API Endpoints

### POST /api/auth/login

Authenticates a user and returns a JWT token.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-12-09T05:50:00Z"
}
```

**Error Responses:**
- `400 Bad Request`: Invalid input (email format, password length)
- `401 Unauthorized`: Invalid credentials or account locked
- `429 Too Many Requests`: Rate limit exceeded

## Security Features

### Password Security
- Passwords are hashed using BCrypt with automatic salt generation
- Password verification uses constant-time comparison
- Minimum password length: 6 characters
- Maximum password length: 100 characters

### Login Attempt Tracking
- All login attempts are logged with:
  - User ID (if found)
  - Email attempted
  - IP address
  - User agent
  - Timestamp
  - Success/failure status

### Rate Limiting
- Maximum 5 failed login attempts per 15 minutes per IP/email combination
- Generic error messages to prevent account enumeration

### JWT Tokens
- Tokens expire after 60 minutes (configurable)
- Signed with HMAC-SHA256
- Include claims: user ID, email, issue time, expiration

## Project Structure

```
src/LoginApi/
├── Controllers/
│   └── AuthController.cs          # Login endpoint
├── Data/
│   └── ApplicationDbContext.cs    # EF Core DbContext
├── Models/
│   ├── User.cs                    # User entity
│   ├── LoginAttempt.cs            # Login attempt entity
│   ├── LoginRequest.cs            # Login request DTO
│   └── LoginResponse.cs           # Login response DTO
├── Repositories/
│   ├── IUserRepository.cs         # User repository interface
│   ├── UserRepository.cs          # User repository implementation
│   ├── ILoginAttemptRepository.cs # Login attempt repository interface
│   └── LoginAttemptRepository.cs  # Login attempt repository implementation
├── Services/
│   ├── IPasswordHasher.cs         # Password hasher interface
│   ├── BcryptPasswordHasher.cs    # BCrypt password hasher
│   ├── ITokenService.cs           # Token service interface
│   ├── JwtTokenService.cs         # JWT token service
│   ├── JwtSettings.cs             # JWT configuration
│   └── AuthenticationService.cs   # Authentication logic
└── Program.cs                      # Application entry point
```

## Development

### Build the Project

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

### Create a New Migration

```bash
dotnet ef migrations add MigrationName
```

## Production Considerations

1. **Secrets Management**: Use Azure Key Vault or similar for JWT secrets
2. **Database**: Use a production SQL Server instance with proper backups
3. **HTTPS**: Ensure SSL/TLS certificates are properly configured
4. **Logging**: Integrate with Application Insights or similar for centralized logging
5. **Rate Limiting**: Consider implementing distributed rate limiting with Redis
6. **Monitoring**: Set up alerts for failed login attempts and security events

## License

This project is licensed under the MIT License.