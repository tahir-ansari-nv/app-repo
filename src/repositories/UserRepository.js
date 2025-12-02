const User = require('../models/User');
const bcrypt = require('bcrypt');
const config = require('../config');

/**
 * UserRepository
 * Handles user data access and storage
 * This is an in-memory implementation. In production, this would connect to a database.
 */
class UserRepository {
  constructor() {
    this.users = new Map();
    this.initialized = false;
    this.initPromise = this._initializeSampleUsers();
  }

  /**
   * Initialize with sample users for demonstration
   */
  async _initializeSampleUsers() {
    // Create sample users with hashed passwords
    const sampleUsers = [
      {
        id: '1',
        email: 'user@example.com',
        password: 'password123',
        roles: ['user']
      },
      {
        id: '2',
        email: 'admin@example.com',
        password: 'admin123',
        roles: ['admin', 'user']
      }
    ];

    for (const userData of sampleUsers) {
      const passwordHash = await bcrypt.hash(userData.password, config.security.bcryptRounds);
      const user = new User({
        id: userData.id,
        email: userData.email,
        passwordHash,
        roles: userData.roles,
        isActive: true
      });
      this.users.set(user.email, user);
    }
    this.initialized = true;
  }

  /**
   * Ensure repository is initialized
   */
  async _ensureInitialized() {
    if (!this.initialized) {
      await this.initPromise;
    }
  }

  /**
   * Find a user by email
   * @param {string} email - User email
   * @returns {Promise<User|null>} User object or null if not found
   */
  async findByEmail(email) {
    await this._ensureInitialized();
    const user = this.users.get(email);
    return user || null;
  }

  /**
   * Create a new user
   * @param {Object} userData - User data
   * @returns {Promise<User>} Created user
   */
  async create(userData) {
    await this._ensureInitialized();
    if (this.users.has(userData.email)) {
      throw new Error('User with this email already exists');
    }

    const user = new User(userData);
    this.users.set(user.email, user);
    return user;
  }

  /**
   * Check if a user exists by email
   * @param {string} email - User email
   * @returns {Promise<boolean>} True if user exists
   */
  async exists(email) {
    await this._ensureInitialized();
    return this.users.has(email);
  }
}

module.exports = new UserRepository();
