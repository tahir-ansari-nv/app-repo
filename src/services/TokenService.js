const jwt = require('jsonwebtoken');
const config = require('../config');

/**
 * Token Service
 * Handles JWT token creation, signing, and expiration management
 */
class TokenService {
  /**
   * Generate a JWT token for a user
   * @param {Object} user - User object
   * @returns {Object} Token and expiration information
   */
  generateToken(user) {
    const payload = {
      sub: user.email,
      userId: user.id,
      roles: user.roles || []
    };

    const token = jwt.sign(payload, config.jwt.secret, {
      expiresIn: config.jwt.expiration
    });

    // Calculate expiration date
    const expiresAt = this._calculateExpiration(config.jwt.expiration);

    return {
      token,
      expiresAt
    };
  }

  /**
   * Verify a JWT token
   * @param {string} token - JWT token
   * @returns {Object} Decoded token payload
   */
  verifyToken(token) {
    try {
      return jwt.verify(token, config.jwt.secret);
    } catch (error) {
      throw new Error('Invalid or expired token');
    }
  }

  /**
   * Calculate expiration date from expiration string
   * @param {string} expiration - Expiration string (e.g., '1h', '7d')
   * @returns {string} ISO date string
   */
  _calculateExpiration(expiration) {
    const now = new Date();
    const match = expiration.match(/^(\d+)([smhd])$/);
    
    if (!match) {
      // Default to 1 hour if format is invalid
      return new Date(now.getTime() + 3600000).toISOString();
    }

    const [, value, unit] = match;
    const multipliers = {
      s: 1000,
      m: 60000,
      h: 3600000,
      d: 86400000
    };

    const milliseconds = parseInt(value) * multipliers[unit];
    return new Date(now.getTime() + milliseconds).toISOString();
  }
}

module.exports = new TokenService();
