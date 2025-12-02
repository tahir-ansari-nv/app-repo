require('dotenv').config();

const config = {
  server: {
    port: process.env.PORT || 3000,
    env: process.env.NODE_ENV || 'development'
  },
  jwt: {
    secret: process.env.JWT_SECRET || 'dev-secret-key',
    expiration: process.env.JWT_EXPIRATION || '1h'
  },
  security: {
    bcryptRounds: parseInt(process.env.BCRYPT_ROUNDS) || 10
  },
  rateLimit: {
    windowMs: parseInt(process.env.RATE_LIMIT_WINDOW_MS) || 900000, // 15 minutes
    maxRequests: parseInt(process.env.RATE_LIMIT_MAX_REQUESTS) || 5
  }
};

module.exports = config;
