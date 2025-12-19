# TimeSheet Portal API Documentation

## Overview

The TimeSheet Portal API provides secure authentication and authorization services for the TimeSheet Management Portal. This document details all available endpoints, request/response formats, and usage examples.

## Base URL

Development: `https://localhost:5001`

## Authentication

Most endpoints require authentication using JWT Bearer tokens. Include the token in the Authorization header:

```
Authorization: Bearer {your-jwt-token}
```

## Endpoints

### 1. User Login

Authenticates a user with username and password.

**Endpoint:** `POST /api/auth/login`

**Request Body:**
```json
{
  "username": "string (required, 3-256 characters)",
  "password": "string (required, 6-256 characters)"
}
```

**Success Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "requiresMFA": false
}
```

**Success Response with MFA (200 OK):**
```json
{
  "token": null,
  "requiresMFA": true
}
```

**Error Responses:**
- `400 Bad Request` - Invalid request format or validation errors
- `401 Unauthorized` - Invalid credentials
- `423 Locked` - Account is locked due to too many failed attempts

**Example:**
```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin@123"
  }'
```

**Rate Limit:** 5 requests per minute per IP address

---

### 2. MFA Verification

Verifies the MFA code sent to the user's email.

**Endpoint:** `POST /api/auth/mfa/verify`

**Request Body:**
```json
{
  "username": "string (required, max 256 characters)",
  "mfaCode": "string (required, max 10 characters)"
}
```

**Success Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Error Responses:**
- `400 Bad Request` - Invalid request format or validation errors
- `401 Unauthorized` - Invalid or expired MFA code

**Example:**
```bash
curl -X POST https://localhost:5001/api/auth/mfa/verify \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "mfaCode": "123456"
  }'
```

---

### 3. Password Recovery Request

Initiates a password recovery process by sending a reset token to the user's email.

**Endpoint:** `POST /api/auth/password-recovery/request`

**Request Body:**
```json
{
  "email": "string (required, valid email format, max 256 characters)"
}
```

**Success Response (200 OK):**
```json
{
  "message": "If the email exists, a password recovery has been initiated"
}
```

**Notes:**
- Always returns success for security reasons (doesn't reveal if email exists)
- Token expires after 1 hour
- Check email for password reset link

**Example:**
```bash
curl -X POST https://localhost:5001/api/auth/password-recovery/request \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@timesheetportal.com"
  }'
```

**Rate Limit:** 3 requests per minute per IP address

---

### 4. Password Reset

Resets the user's password using the token received via email.

**Endpoint:** `POST /api/auth/password-recovery/reset`

**Request Body:**
```json
{
  "token": "string (required, GUID format)",
  "newPassword": "string (required, 8-256 characters, must meet complexity requirements)"
}
```

**Password Requirements:**
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character (@$!%*?&)

**Success Response (200 OK):**
```json
{
  "message": "Password reset successfully"
}
```

**Error Responses:**
- `400 Bad Request` - Invalid token, expired token, or password doesn't meet requirements

**Example:**
```bash
curl -X POST https://localhost:5001/api/auth/password-recovery/reset \
  -H "Content-Type: application/json" \
  -d '{
    "token": "550e8400-e29b-41d4-a716-446655440000",
    "newPassword": "NewPassword@123"
  }'
```

---

### 5. Logout

Logs out the current user (token revocation on client side).

**Endpoint:** `POST /api/auth/logout`

**Authentication:** Required (Bearer token)

**Success Response (200 OK):**
```json
{
  "message": "Logged out successfully"
}
```

**Example:**
```bash
curl -X POST https://localhost:5001/api/auth/logout \
  -H "Authorization: Bearer {your-jwt-token}"
```

---

## Error Responses

All error responses follow this format:

```json
{
  "message": "Error description",
  "details": "Optional additional details"
}
```

### Common HTTP Status Codes

- `200 OK` - Request succeeded
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Authentication failed or token invalid
- `423 Locked` - Account is locked
- `429 Too Many Requests` - Rate limit exceeded
- `500 Internal Server Error` - Server error

---

## Security Features

### Rate Limiting

- Login endpoint: 5 requests per minute per IP
- Password recovery: 3 requests per minute per IP
- All other endpoints: 100 requests per minute per IP

### Account Lockout

- Threshold: 5 failed login attempts
- Lockout duration: 15 minutes
- Automatic unlock after lockout period expires

### JWT Token

- Expiry: 60 minutes (configurable)
- Algorithm: HMAC SHA256
- Claims included: UserId, Username, Email

### Password Security

- Passwords hashed using BCrypt with salt
- Password complexity requirements enforced
- Password history not tracked (future enhancement)

---

## Test Users

The following test users are seeded in development:

### Admin User
- **Username:** `admin`
- **Password:** `Admin@123`
- **Email:** `admin@timesheetportal.com`
- **MFA Enabled:** No

### Test User
- **Username:** `testuser`
- **Password:** `Test@123`
- **Email:** `test@timesheetportal.com`
- **MFA Enabled:** Yes

---

## Swagger UI

Interactive API documentation is available in development at:

`https://localhost:5001/swagger`

---

## Rate Limit Headers

When rate limits are enforced, the following headers are included in responses:

- `X-Rate-Limit-Limit` - The maximum number of requests
- `X-Rate-Limit-Remaining` - Remaining requests in current window
- `X-Rate-Limit-Reset` - Time when the limit resets (Unix timestamp)

---

## Example Workflow

### Login Flow Without MFA

1. Send login request with username and password
2. Receive JWT token in response
3. Use token for subsequent authenticated requests

### Login Flow With MFA

1. Send login request with username and password
2. Receive response with `requiresMFA: true`
3. Check email for MFA code
4. Send MFA verification request with username and code
5. Receive JWT token in response
6. Use token for subsequent authenticated requests

### Password Recovery Flow

1. Send password recovery request with email
2. Check email for password reset link
3. Extract token from reset link
4. Send password reset request with token and new password
5. Password is reset, can now login with new password

---

## Support

For issues or questions, please contact the development team or create an issue in the repository.
