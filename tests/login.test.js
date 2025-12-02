const request = require('supertest');
const createApp = require('../src/app');
const LoggingService = require('../src/services/LoggingService');
const rateLimiter = require('../src/middleware/rateLimiter');

describe('Login API Integration Tests', () => {
  let app;

  beforeEach(() => {
    app = createApp();
    LoggingService.clearLogs();
    rateLimiter.clear();
  });

  describe('POST /api/auth/login', () => {
    it('should return 200 and JWT token with valid credentials', async () => {
      const response = await request(app)
        .post('/api/auth/login')
        .send({
          email: 'user@example.com',
          password: 'password123'
        });

      expect(response.status).toBe(200);
      expect(response.body).toHaveProperty('token');
      expect(response.body).toHaveProperty('expiresAt');
      expect(typeof response.body.token).toBe('string');
      expect(response.body.token.length).toBeGreaterThan(0);
    });

    it('should return 401 with invalid password', async () => {
      const response = await request(app)
        .post('/api/auth/login')
        .send({
          email: 'user@example.com',
          password: 'wrongpassword'
        });

      expect(response.status).toBe(401);
      expect(response.body).toHaveProperty('error');
      expect(response.body.error).toBe('Invalid credentials');
    });

    it('should return 401 with non-existent user', async () => {
      const response = await request(app)
        .post('/api/auth/login')
        .send({
          email: 'nonexistent@example.com',
          password: 'password123'
        });

      expect(response.status).toBe(401);
      expect(response.body).toHaveProperty('error');
      expect(response.body.error).toBe('Invalid credentials');
    });

    it('should return 400 with missing email', async () => {
      const response = await request(app)
        .post('/api/auth/login')
        .send({
          password: 'password123'
        });

      expect(response.status).toBe(400);
      expect(response.body).toHaveProperty('error');
      expect(response.body.error).toBe('Validation failed');
    });

    it('should return 400 with missing password', async () => {
      const response = await request(app)
        .post('/api/auth/login')
        .send({
          email: 'user@example.com'
        });

      expect(response.status).toBe(400);
      expect(response.body).toHaveProperty('error');
    });

    it('should return 400 with invalid email format', async () => {
      const response = await request(app)
        .post('/api/auth/login')
        .send({
          email: 'invalid-email',
          password: 'password123'
        });

      expect(response.status).toBe(400);
      expect(response.body).toHaveProperty('error');
    });

    it('should log successful login attempts', async () => {
      await request(app)
        .post('/api/auth/login')
        .send({
          email: 'user@example.com',
          password: 'password123'
        });

      const logs = LoggingService.getLogs();
      expect(logs.length).toBeGreaterThan(0);
      
      const successLog = logs.find(log => log.success === true);
      expect(successLog).toBeDefined();
      expect(successLog.email).toBe('user@example.com');
    });

    it('should log failed login attempts', async () => {
      await request(app)
        .post('/api/auth/login')
        .send({
          email: 'user@example.com',
          password: 'wrongpassword'
        });

      const logs = LoggingService.getLogs();
      const failedLog = logs.find(log => log.success === false);
      
      expect(failedLog).toBeDefined();
      expect(failedLog.email).toBe('user@example.com');
      expect(failedLog.failureReason).toBeDefined();
    });

    it('should enforce rate limiting after max requests', async () => {
      const maxRequests = 5;
      
      // Make maximum allowed requests
      for (let i = 0; i < maxRequests; i++) {
        await request(app)
          .post('/api/auth/login')
          .send({
            email: 'user@example.com',
            password: 'password123'
          });
      }

      // Next request should be rate limited
      const response = await request(app)
        .post('/api/auth/login')
        .send({
          email: 'user@example.com',
          password: 'password123'
        });

      expect(response.status).toBe(429);
      expect(response.body).toHaveProperty('error');
      expect(response.body.error).toBe('Too many requests');
    });

    it('should work with admin user credentials', async () => {
      const response = await request(app)
        .post('/api/auth/login')
        .send({
          email: 'admin@example.com',
          password: 'admin123'
        });

      expect(response.status).toBe(200);
      expect(response.body).toHaveProperty('token');
    });
  });

  describe('GET /api/auth/health', () => {
    it('should return health status', async () => {
      const response = await request(app)
        .get('/api/auth/health');

      expect(response.status).toBe(200);
      expect(response.body).toHaveProperty('status');
      expect(response.body.status).toBe('ok');
      expect(response.body).toHaveProperty('timestamp');
    });
  });

  describe('GET /', () => {
    it('should return API information', async () => {
      const response = await request(app).get('/');

      expect(response.status).toBe(200);
      expect(response.body).toHaveProperty('message');
      expect(response.body).toHaveProperty('endpoints');
    });
  });

  describe('404 handling', () => {
    it('should return 404 for unknown routes', async () => {
      const response = await request(app).get('/api/unknown');

      expect(response.status).toBe(404);
      expect(response.body).toHaveProperty('error');
    });
  });
});
