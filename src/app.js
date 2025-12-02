const express = require('express');
const authRoutes = require('./routes/auth');
const { errorHandler, notFoundHandler } = require('./middleware/errorHandler');

/**
 * Create and configure Express application
 */
function createApp() {
  const app = express();

  // Middleware
  app.use(express.json());
  app.use(express.urlencoded({ extended: true }));

  // Security headers
  app.use((req, res, next) => {
    res.setHeader('X-Content-Type-Options', 'nosniff');
    res.setHeader('X-Frame-Options', 'DENY');
    res.setHeader('X-XSS-Protection', '1; mode=block');
    next();
  });

  // Trust proxy for correct IP address (important for rate limiting)
  app.set('trust proxy', true);

  // Routes
  app.use('/api/auth', authRoutes);

  // Root endpoint
  app.get('/', (req, res) => {
    res.json({
      message: 'Login API',
      version: '1.0.0',
      endpoints: {
        login: 'POST /api/auth/login',
        health: 'GET /api/auth/health'
      }
    });
  });

  // Error handling
  app.use(notFoundHandler);
  app.use(errorHandler);

  return app;
}

module.exports = createApp;
