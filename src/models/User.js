/**
 * User Model
 * Represents a user in the system with authentication credentials
 */
class User {
  constructor({ id, email, passwordHash, roles = [], isActive = true }) {
    this.id = id;
    this.email = email;
    this.passwordHash = passwordHash;
    this.roles = roles;
    this.isActive = isActive;
  }

  /**
   * Create a user object without the password hash (for safe serialization)
   */
  toSafeObject() {
    return {
      id: this.id,
      email: this.email,
      roles: this.roles,
      isActive: this.isActive
    };
  }
}

module.exports = User;
