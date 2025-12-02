const UserRepository = require('../repositories/UserRepository');
const passwordHashing = require('../utils/passwordHashing');
const TokenService = require('./TokenService');
const LoggingService = require('./LoggingService');

/**
 * Authentication Service
 * Encapsulates business logic for verifying credentials and generating tokens
 */
class AuthenticationService {
  /**
   * Authenticate a user with email and password
   * @param {string} email - User email
   * @param {string} password - User password
   * @param {string} ipAddress - Request IP address
   * @param {string} userAgent - Request user agent
   * @returns {Promise<Object>} Authentication result with token or error
   */
  async authenticate(email, password, ipAddress, userAgent = null) {
    try {
      // Fetch user from repository
      const user = await UserRepository.findByEmail(email);

      // If user not found, log failed attempt and return error
      if (!user) {
        LoggingService.logLoginAttempt({
          email,
          ipAddress,
          success: false,
          failureReason: 'User not found',
          userAgent
        });

        return {
          success: false,
          error: 'Invalid credentials',
          statusCode: 401
        };
      }

      // Check if user account is active
      if (!user.isActive) {
        LoggingService.logLoginAttempt({
          email,
          ipAddress,
          success: false,
          failureReason: 'Account inactive',
          userAgent
        });

        return {
          success: false,
          error: 'Invalid credentials',
          statusCode: 401
        };
      }

      // Verify password
      const isPasswordValid = await passwordHashing.verify(password, user.passwordHash);

      if (!isPasswordValid) {
        LoggingService.logLoginAttempt({
          email,
          ipAddress,
          success: false,
          failureReason: 'Invalid password',
          userAgent
        });

        return {
          success: false,
          error: 'Invalid credentials',
          statusCode: 401
        };
      }

      // Generate JWT token
      const { token, expiresAt } = TokenService.generateToken(user);

      // Log successful login
      LoggingService.logLoginAttempt({
        email,
        ipAddress,
        success: true,
        userAgent
      });

      return {
        success: true,
        token,
        expiresAt
      };
    } catch (error) {
      // Log unexpected errors
      console.error('Authentication error:', error);
      
      LoggingService.logLoginAttempt({
        email,
        ipAddress,
        success: false,
        failureReason: 'Internal error',
        userAgent
      });

      return {
        success: false,
        error: 'Internal server error',
        statusCode: 500
      };
    }
  }
}

module.exports = new AuthenticationService();
