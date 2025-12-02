# Login API

A secure, robust Login API that authenticates users based on email and password. The system verifies credentials using securely hashed passwords (bcrypt), responds with JWT tokens on successful logins, and rejects invalid attempts with appropriate error codes. Each login attempt is logged for auditing and security monitoring.

## Features

- ✅ Secure password hashing with bcrypt
- ✅ JWT token generation with configurable expiration
- ✅ Comprehensive input validation
- ✅ Rate limiting to prevent brute-force attacks
- ✅ Detailed login attempt logging
- ✅ RESTful API design
- ✅ Comprehensive test coverage
- ✅ Security best practices

## Architecture

### Components

- **API Layer / Controller**: Exposes Login POST endpoint
- **Authentication Service**: Encapsulates business logic of verifying credentials and generating tokens
- **User Repository**: Interface to user data storage
- **Password Hashing Module**: Implements secure password comparison using bcrypt
- **JWT Token Generator**: Signs and issues JWT tokens with configured claims and expiry
- **Logging Module**: Captures each login attempt with relevant metadata
- **Rate Limiter**: Prevents brute-force attacks

## Installation

```bash
# Install dependencies
npm install
```

## Configuration

Copy `.env.example` to `.env` and configure:

```bash
cp .env.example .env
```

Configuration options:

- `PORT`: Server port (default: 3000)
- `NODE_ENV`: Environment (development/production)
- `JWT_SECRET`: Secret key for JWT signing (change in production!)
- `JWT_EXPIRATION`: Token expiration time (e.g., '1h', '7d')
- `BCRYPT_ROUNDS`: Number of bcrypt rounds (default: 10)
- `RATE_LIMIT_WINDOW_MS`: Rate limit time window in milliseconds
- `RATE_LIMIT_MAX_REQUESTS`: Max requests per window

## Usage

### Start the server

```bash
# Development mode with auto-reload
npm run dev

# Production mode
npm start
```

### Run tests

```bash
# Run all tests
npm test

# Run tests in watch mode
npm run test:watch
```

## API Endpoints

### POST /api/auth/login

Authenticate user and return JWT token.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Success Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2024-07-01T12:00:00.000Z"
}
```

**Error Responses:**

- `400 Bad Request`: Missing or malformed email/password fields
  ```json
  {
    "error": "Validation failed",
    "details": [
      {
        "field": "email",
        "message": "Must be a valid email address"
      }
    ]
  }
  ```

- `401 Unauthorized`: Invalid credentials
  ```json
  {
    "error": "Invalid credentials"
  }
  ```

- `429 Too Many Requests`: Rate limiting triggered
  ```json
  {
    "error": "Too many requests",
    "message": "Too many login attempts. Please try again later.",
    "retryAfter": 300
  }
  ```

- `500 Internal Server Error`: Unexpected errors
  ```json
  {
    "error": "Internal server error"
  }
  ```

### GET /api/auth/health

Health check endpoint.

**Success Response (200):**
```json
{
  "status": "ok",
  "timestamp": "2024-07-01T10:30:00.000Z"
}
```

## Sample Users

For testing purposes, the following users are pre-configured:

| Email | Password | Roles |
|-------|----------|-------|
| user@example.com | password123 | user |
| admin@example.com | admin123 | admin, user |

## Security Considerations

### Implemented

- ✅ Secure password hashing with bcrypt (10 rounds)
- ✅ JWT tokens signed with secret key
- ✅ Input validation and sanitization
- ✅ Rate limiting per IP address
- ✅ Generic error messages to avoid leaking user existence
- ✅ Secure HTTP headers (X-Content-Type-Options, X-Frame-Options, X-XSS-Protection)
- ✅ No sensitive data in logs (no passwords or tokens)
- ✅ Account status checking (isActive flag)

### Recommendations for Production

- 🔒 Use HTTPS only (enforce with reverse proxy)
- 🔒 Store JWT secret in secure secrets manager
- 🔒 Use environment-specific configuration
- 🔒 Connect to persistent database (PostgreSQL, MongoDB, etc.)
- 🔒 Implement distributed rate limiting with Redis
- 🔒 Add structured logging to external service (ELK, Splunk)
- 🔒 Implement account lockout after repeated failures
- 🔒 Add MFA support
- 🔒 Implement token refresh mechanism
- 🔒 Monitor and alert on suspicious patterns
- 🔒 Regular security audits and dependency updates

## Data Model

### User

```javascript
{
  id: "UUID",
  email: "string (unique, indexed)",
  passwordHash: "string (bcrypt hash)",
  roles: ["array of strings"],
  isActive: "boolean"
}
```

### LoginAttemptLog

```javascript
{
  id: "UUID",
  timestamp: "datetime",
  email: "string",
  ipAddress: "string",
  success: "boolean",
  failureReason: "string (optional)",
  userAgent: "string (optional)"
}
```

## Testing

The project includes comprehensive tests:

- Integration tests for API endpoints
- Unit tests for services and utilities
- Test coverage reporting

Run tests with:
```bash
npm test
```

## Project Structure

```
app-repo/
├── src/
│   ├── config/           # Configuration management
│   ├── controllers/      # API controllers
│   ├── middleware/       # Express middleware
│   ├── models/          # Data models
│   ├── repositories/    # Data access layer
│   ├── routes/          # API routes
│   ├── services/        # Business logic
│   ├── utils/           # Utility functions
│   ├── app.js           # Express app setup
│   └── server.js        # Server entry point
├── tests/               # Test files
├── .env.example         # Example environment variables
├── .gitignore          # Git ignore rules
├── jest.config.js      # Jest configuration
├── package.json        # Dependencies and scripts
└── README.md           # This file
```

## License

ISC