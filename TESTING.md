# API Testing Guide

This guide provides sample requests for testing the Login API.

## Test Users

The following test users are automatically seeded in development mode:

1. **Active Admin User**
   - Email: `admin@example.com`
   - Password: `Admin123!`
   - Status: Active

2. **Active Regular User**
   - Email: `user@example.com`
   - Password: `User123!`
   - Status: Active

3. **Inactive User**
   - Email: `inactive@example.com`
   - Password: `Inactive123!`
   - Status: Inactive (login will fail)

## cURL Examples

### Successful Login

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "Admin123!"
  }'
```

**Expected Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAt": "2025-12-09T06:50:00Z"
}
```

### Failed Login - Invalid Credentials

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@example.com",
    "password": "WrongPassword"
  }'
```

**Expected Response (401 Unauthorized):**
```json
{
  "message": "Invalid credentials or account locked."
}
```

### Failed Login - Inactive Account

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "inactive@example.com",
    "password": "Inactive123!"
  }'
```

**Expected Response (401 Unauthorized):**
```json
{
  "message": "Invalid credentials or account locked."
}
```

### Failed Login - Invalid Email Format

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "not-an-email",
    "password": "SomePassword"
  }'
```

**Expected Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": [
      "The Email field is not a valid e-mail address."
    ]
  }
}
```

### Failed Login - Password Too Short

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "123"
  }'
```

**Expected Response (400 Bad Request):**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Password": [
      "The field Password must be a string with a minimum length of 6 and a maximum length of 100."
    ]
  }
}
```

## Testing Rate Limiting

To test the brute force protection, make 6 failed login attempts within 15 minutes:

```bash
# Run this command 6 times
for i in {1..6}; do
  echo "Attempt $i:"
  curl -X POST https://localhost:5001/api/auth/login \
    -H "Content-Type: application/json" \
    -d '{
      "email": "admin@example.com",
      "password": "WrongPassword"
    }'
  echo -e "\n"
done
```

After the 5th failed attempt, subsequent requests should return 401 Unauthorized due to account lockout.

## Using the JWT Token

Once you receive a token, you can use it to access protected endpoints (if any):

```bash
TOKEN="your-jwt-token-here"

curl -X GET https://localhost:5001/api/protected/resource \
  -H "Authorization: Bearer $TOKEN"
```

## Postman Collection

You can import this collection into Postman for easier testing:

```json
{
  "info": {
    "name": "LoginApi",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Login - Success",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"email\": \"admin@example.com\",\n  \"password\": \"Admin123!\"\n}"
        },
        "url": {
          "raw": "https://localhost:5001/api/auth/login",
          "protocol": "https",
          "host": ["localhost"],
          "port": "5001",
          "path": ["api", "auth", "login"]
        }
      }
    },
    {
      "name": "Login - Invalid Credentials",
      "request": {
        "method": "POST",
        "header": [
          {
            "key": "Content-Type",
            "value": "application/json"
          }
        ],
        "body": {
          "mode": "raw",
          "raw": "{\n  \"email\": \"admin@example.com\",\n  \"password\": \"WrongPassword\"\n}"
        },
        "url": {
          "raw": "https://localhost:5001/api/auth/login",
          "protocol": "https",
          "host": ["localhost"],
          "port": "5001",
          "path": ["api", "auth", "login"]
        }
      }
    }
  ]
}
```

## Verifying JWT Tokens

You can decode and verify JWT tokens at [jwt.io](https://jwt.io).

Paste your token and verify that it contains:
- `sub`: User ID
- `email`: User email address
- `jti`: Unique token ID
- `iat`: Issued at timestamp
- `exp`: Expiration timestamp
- `iss`: Issuer (LoginApi)
- `aud`: Audience (LoginApiUsers)
