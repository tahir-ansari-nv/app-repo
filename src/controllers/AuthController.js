const AuthenticationService = require('../services/AuthenticationService');

/**
 * Authentication Controller
 * Handles HTTP requests for authentication endpoints
 */
class AuthController {
  /**
   * Handle login request
   * POST /api/auth/login
   */
  async login(req, res, next) {
    try {
      const { email, password } = req.body;
      
      // Get IP address and user agent from request
      const ipAddress = req.ip || req.connection.remoteAddress || 'unknown';
      const userAgent = req.get('user-agent') || null;

      // Authenticate user
      const result = await AuthenticationService.authenticate(
        email,
        password,
        ipAddress,
        userAgent
      );

      // Handle authentication result
      if (result.success) {
        return res.status(200).json({
          token: result.token,
          expiresAt: result.expiresAt
        });
      } else {
        return res.status(result.statusCode || 401).json({
          error: result.error
        });
      }
    } catch (error) {
      next(error);
    }
  }

  /**
   * Health check endpoint
   * GET /api/auth/health
   */
  health(req, res) {
    res.status(200).json({
      status: 'ok',
      timestamp: new Date().toISOString()
    });
  }
}

module.exports = new AuthController();
