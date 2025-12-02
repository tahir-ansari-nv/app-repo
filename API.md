# API Documentation

## Base URL
```
http://localhost:3000/api
```

## Authentication Endpoints

### POST /auth/login

Authenticate a user with email and password credentials.

#### Request

**Headers:**
```
Content-Type: application/json
```

**Body:**
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```

**Parameters:**

| Field    | Type   | Required | Description                |
|----------|--------|----------|----------------------------|
| email    | string | Yes      | User's email address       |
| password | string | Yes      | User's password            |

**Email Validation:**
- Must be a valid email format
- Will be normalized (lowercased and trimmed)

#### Responses

**Success (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJ1c2VyQGV4YW1wbGUuY29tIiwidXNlcklkIjoiMSIsInJvbGVzIjpbInVzZXIiXSwiaWF0IjoxNzY0NjY4MDMwLCJleHAiOjE3NjQ2NzE2MzB9.dtt8fdSsx4IAlGlQIJJUapxPkEo7q6gGMcKih4BYjXQ",
  "expiresAt": "2024-07-01T12:00:00.000Z"
}
```

**Response Fields:**

| Field     | Type   | Description                                    |
|-----------|--------|------------------------------------------------|
| token     | string | JWT token for authenticated requests           |
| expiresAt | string | ISO 8601 timestamp when token expires          |

**Error Responses:**

**400 Bad Request** - Invalid input
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

Common validation errors:
- Missing email field
- Invalid email format
- Missing password field

**401 Unauthorized** - Invalid credentials
```json
{
  "error": "Invalid credentials"
}
```

Returned when:
- User does not exist
- Password is incorrect
- Account is inactive

**429 Too Many Requests** - Rate limit exceeded
```json
{
  "error": "Too many requests",
  "message": "Too many login attempts. Please try again later.",
  "retryAfter": 300
}
```

Rate limit: 5 requests per 15 minutes per IP address

**500 Internal Server Error** - Server error
```json
{
  "error": "Internal server error"
}
```

#### Example Usage

**cURL:**
```bash
curl -X POST http://localhost:3000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }'
```

**JavaScript (fetch):**
```javascript
const response = await fetch('http://localhost:3000/api/auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    email: 'user@example.com',
    password: 'password123'
  })
});

const data = await response.json();
console.log(data.token);
```

**Python (requests):**
```python
import requests

response = requests.post(
    'http://localhost:3000/api/auth/login',
    json={
        'email': 'user@example.com',
        'password': 'password123'
    }
)

data = response.json()
print(data['token'])
```

---

### GET /auth/health

Health check endpoint to verify API availability.

#### Request

No parameters required.

#### Response

**Success (200 OK):**
```json
{
  "status": "ok",
  "timestamp": "2024-07-01T10:30:00.000Z"
}
```

**Response Fields:**

| Field     | Type   | Description                      |
|-----------|--------|----------------------------------|
| status    | string | Always "ok" when server is up    |
| timestamp | string | Current server time (ISO 8601)   |

#### Example Usage

**cURL:**
```bash
curl http://localhost:3000/api/auth/health
```

**JavaScript (fetch):**
```javascript
const response = await fetch('http://localhost:3000/api/auth/health');
const data = await response.json();
console.log(data.status); // "ok"
```

---

## Using JWT Tokens

After successful login, use the received JWT token to authenticate subsequent requests to protected endpoints.

### Token Format

The token is a JWT (JSON Web Token) with the following structure:

**Header:**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload:**
```json
{
  "sub": "user@example.com",
  "userId": "1",
  "roles": ["user"],
  "iat": 1764668030,
  "exp": 1764671630
}
```

**Payload Fields:**

| Field  | Type     | Description                           |
|--------|----------|---------------------------------------|
| sub    | string   | Subject (user's email)                |
| userId | string   | User's unique identifier              |
| roles  | string[] | User's roles/permissions              |
| iat    | number   | Issued at (Unix timestamp)            |
| exp    | number   | Expiration time (Unix timestamp)      |

### Using Tokens in Requests

Include the token in the `Authorization` header:

```
Authorization: Bearer <token>
```

**Example:**
```bash
curl -X GET http://localhost:3000/api/protected-endpoint \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

---

## Sample Users

For testing purposes, the following users are available:

| Email               | Password    | Roles        |
|---------------------|-------------|--------------|
| user@example.com    | password123 | user         |
| admin@example.com   | admin123    | admin, user  |

**⚠️ Note:** These are test credentials only. In production, users would be created through a registration endpoint.

---

## Rate Limiting

All login endpoints are rate-limited to prevent brute-force attacks.

**Limits:**
- 5 requests per 15-minute window
- Tracked per IP address

**When rate limited:**
- HTTP 429 status code
- Response includes `retryAfter` field (seconds until limit resets)

**Example response:**
```json
{
  "error": "Too many requests",
  "message": "Too many login attempts. Please try again later.",
  "retryAfter": 300
}
```

---

## Error Codes

| Code | Meaning                | Common Causes                                          |
|------|------------------------|--------------------------------------------------------|
| 200  | Success                | Request completed successfully                         |
| 400  | Bad Request            | Invalid input, missing fields, validation errors       |
| 401  | Unauthorized           | Invalid credentials, inactive account                  |
| 404  | Not Found              | Endpoint does not exist                                |
| 429  | Too Many Requests      | Rate limit exceeded                                    |
| 500  | Internal Server Error  | Server error (database issues, unexpected errors)      |

---

## Security Headers

All responses include security headers:

| Header                    | Value         | Purpose                              |
|---------------------------|---------------|--------------------------------------|
| X-Content-Type-Options    | nosniff       | Prevent MIME type sniffing           |
| X-Frame-Options           | DENY          | Prevent clickjacking                 |
| X-XSS-Protection          | 1; mode=block | Enable XSS protection                |

---

## Best Practices

### Client Implementation

1. **Store tokens securely**
   - Use httpOnly cookies or secure storage
   - Never store in localStorage for sensitive applications

2. **Handle token expiration**
   - Check `expiresAt` field
   - Implement token refresh logic
   - Handle 401 responses gracefully

3. **Implement retry logic**
   - Exponential backoff for 429 errors
   - Respect `retryAfter` header

4. **Error handling**
   - Display user-friendly messages
   - Don't expose technical details to users
   - Log errors for debugging

5. **Security**
   - Always use HTTPS in production
   - Don't log tokens or passwords
   - Implement CSRF protection for web apps

### Example Client Implementation

```javascript
class AuthClient {
  constructor(baseUrl) {
    this.baseUrl = baseUrl;
    this.token = null;
    this.expiresAt = null;
  }

  async login(email, password) {
    try {
      const response = await fetch(`${this.baseUrl}/api/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });

      if (!response.ok) {
        const error = await response.json();
        throw new Error(error.error || 'Login failed');
      }

      const data = await response.json();
      this.token = data.token;
      this.expiresAt = new Date(data.expiresAt);
      
      return data;
    } catch (error) {
      console.error('Login error:', error.message);
      throw error;
    }
  }

  isTokenValid() {
    return this.token && this.expiresAt > new Date();
  }

  getAuthHeader() {
    if (!this.isTokenValid()) {
      throw new Error('Token expired or not available');
    }
    return `Bearer ${this.token}`;
  }
}

// Usage
const client = new AuthClient('http://localhost:3000');
await client.login('user@example.com', 'password123');
```

---

## Support

For issues or questions:
- Check the README.md for setup instructions
- Review the SECURITY.md for security considerations
- Run the test suite to verify your environment
