# TimeSheet Management Portal - Secure Login Flow

This project implements a secure authentication and authorization system for the TimeSheet Management Portal using ASP.NET Core Web API with Entity Framework Core, JWT authentication, and comprehensive security features.

## 📚 Documentation

- **[API Documentation](docs/API_DOCUMENTATION.md)** - Complete API reference with examples
- **[Deployment Guide](docs/DEPLOYMENT_GUIDE.md)** - Deployment instructions for various platforms
- **[Security Summary](docs/SECURITY_SUMMARY.md)** - Comprehensive security analysis and recommendations

## Features

- **User Authentication**: Secure login with username and password
- **Multi-Factor Authentication (MFA)**: Optional MFA support with email-based codes
- **Password Recovery**: Secure password reset via email token
- **Account Lockout**: Protection against brute-force attacks (5 failed attempts = 15-minute lockout)
- **Rate Limiting**: IP-based rate limiting on authentication endpoints
- **JWT Token Management**: Stateless authentication with JWT tokens
- **Input Validation**: Protection against injection and XSS attacks
- **Audit Logging**: Comprehensive logging of authentication events

## Architecture

### Project Structure
```
TimeSheetPortal/
├── src/
│   ├── TimeSheetPortal.API/          # Web API layer
│   ├── TimeSheetPortal.Core/         # Domain entities, DTOs, interfaces
│   └── TimeSheetPortal.Infrastructure/ # Repositories, services, DbContext
├── tests/
│   └── TimeSheetPortal.Tests/        # Unit and integration tests
└── docs/
    ├── API_DOCUMENTATION.md          # API reference
    ├── DEPLOYMENT_GUIDE.md           # Deployment instructions
    └── SECURITY_SUMMARY.md           # Security analysis
```

### Technology Stack
- ASP.NET Core 8.0 Web API
- Entity Framework Core 8.0
- SQL Server (LocalDB for development)
- JWT Bearer Authentication
- BCrypt.Net for password hashing
- AspNetCoreRateLimit for rate limiting
- Swagger/OpenAPI for API documentation

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- SQL Server LocalDB (or SQL Server)

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd app-repo
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Update database connection string** (if needed)
   Edit `src/TimeSheetPortal.API/appsettings.json` and update the `ConnectionStrings:DefaultConnection`

4. **Apply database migrations**
   ```bash
   cd src/TimeSheetPortal.Infrastructure
   dotnet ef database update --startup-project ../TimeSheetPortal.API/TimeSheetPortal.API.csproj
   ```

5. **Run the application**
   ```bash
   cd ../TimeSheetPortal.API
   dotnet run
   ```

6. **Access Swagger UI**
   Open your browser and navigate to: `https://localhost:5001/swagger`

### Default Test Users

The application seeds two test users on first run:

1. **Admin User**
   - Username: `admin`
   - Password: `Admin@123`
   - MFA Enabled: No

2. **Test User**
   - Username: `testuser`
   - Password: `Test@123`
   - MFA Enabled: Yes

## API Endpoints

### Authentication

#### 1. Login
```
POST /api/auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "Admin@123"
}

Response:
{
  "token": "eyJhbGc...",
  "requiresMFA": false
}
```

#### 2. MFA Verification
```
POST /api/auth/mfa/verify
Content-Type: application/json

{
  "username": "testuser",
  "mfaCode": "123456"
}

Response:
{
  "token": "eyJhbGc..."
}
```

#### 3. Password Recovery Request
```
POST /api/auth/password-recovery/request
Content-Type: application/json

{
  "email": "admin@timesheetportal.com"
}

Response:
{
  "message": "If the email exists, a password recovery has been initiated"
}
```

#### 4. Password Reset
```
POST /api/auth/password-recovery/reset
Content-Type: application/json

{
  "token": "guid-token-here",
  "newPassword": "NewPassword@123"
}

Response:
{
  "message": "Password reset successfully"
}
```

#### 5. Logout
```
POST /api/auth/logout
Authorization: Bearer {token}

Response:
{
  "message": "Logged out successfully"
}
```

## Security Features

### Password Requirements
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character (@$!%*?&)

### Rate Limiting
- Login endpoint: 5 requests per minute per IP
- Password recovery: 3 requests per minute per IP
- All other endpoints: 100 requests per minute per IP

### Account Lockout
- Lockout threshold: 5 failed login attempts
- Lockout duration: 15 minutes
- Automatic unlock after lockout period

### JWT Configuration
- Token expiry: 60 minutes (configurable)
- Signature algorithm: HMAC SHA256
- Claims include: UserId, Username, Email

## Testing

Run the test suite:
```bash
cd tests/TimeSheetPortal.Tests
dotnet test
```

## Configuration

### JWT Settings (appsettings.json)
```json
"JwtSettings": {
  "SecretKey": "your-secret-key-here",
  "Issuer": "TimeSheetPortal",
  "Audience": "TimeSheetPortalUsers",
  "ExpiryMinutes": "60"
}
```

### Email Settings (appsettings.json)
```json
"EmailSettings": {
  "SmtpServer": "smtp.example.com",
  "SmtpPort": "587",
  "SmtpUsername": "your-username",
  "SmtpPassword": "your-password",
  "FromEmail": "noreply@timesheetportal.com",
  "FromName": "TimeSheet Portal",
  "ResetPasswordBaseUrl": "http://localhost:5000"
}
```

## Development

### Running Migrations
```bash
# Add a new migration
dotnet ef migrations add MigrationName --startup-project ../TimeSheetPortal.API/TimeSheetPortal.API.csproj

# Update database
dotnet ef database update --startup-project ../TimeSheetPortal.API/TimeSheetPortal.API.csproj

# Remove last migration
dotnet ef migrations remove --startup-project ../TimeSheetPortal.API/TimeSheetPortal.API.csproj
```

### Building the Project
```bash
dotnet build
```

### Running in Development Mode
```bash
cd src/TimeSheetPortal.API
dotnet run --launch-profile https
```

## Security Considerations

- Store JWT secret key in Azure Key Vault or secure configuration in production
- Use HTTPS/TLS for all traffic
- Implement proper SMTP configuration for email sending in production
- Review and adjust rate limiting rules based on your requirements
- Enable proper CORS policies for your frontend application
- Implement proper logging and monitoring
- Regular security audits and dependency updates

## Future Enhancements

- [ ] Implement TOTP-based MFA (Google Authenticator)
- [ ] Add OAuth 2.0 / OpenID Connect support
- [ ] Implement refresh token mechanism
- [ ] Add CAPTCHA for repeated failed login attempts
- [ ] Implement Redis caching for session management
- [ ] Add role-based authorization
- [ ] Implement audit trail for all authentication events
- [ ] Add password history to prevent reuse

## License

This project is proprietary and confidential.
