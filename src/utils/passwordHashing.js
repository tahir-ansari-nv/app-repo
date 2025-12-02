const bcrypt = require('bcrypt');
const config = require('../config');

/**
 * Password Hashing Module
 * Provides secure password hashing and verification using bcrypt
 */
class PasswordHashingService {
  /**
   * Hash a password
   * @param {string} password - Plain text password
   * @returns {Promise<string>} Hashed password
   */
  async hash(password) {
    return await bcrypt.hash(password, config.security.bcryptRounds);
  }

  /**
   * Verify a password against a hash
   * @param {string} password - Plain text password
   * @param {string} hash - Hashed password
   * @returns {Promise<boolean>} True if password matches
   */
  async verify(password, hash) {
    return await bcrypt.compare(password, hash);
  }
}

module.exports = new PasswordHashingService();
