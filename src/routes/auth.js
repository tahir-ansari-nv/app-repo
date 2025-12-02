const express = require('express');
const AuthController = require('../controllers/AuthController');
const { loginValidationRules, validate } = require('../middleware/validation');
const rateLimiter = require('../middleware/rateLimiter');

const router = express.Router();

/**
 * POST /api/auth/login
 * Authenticate user and return JWT token
 */
router.post(
  '/login',
  rateLimiter.middleware,
  loginValidationRules,
  validate,
  (req, res, next) => AuthController.login(req, res, next)
);

/**
 * GET /api/auth/health
 * Health check endpoint
 */
router.get('/health', (req, res) => AuthController.health(req, res));

module.exports = router;
