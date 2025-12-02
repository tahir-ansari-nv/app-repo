/**
 * LoginAttemptLog Model
 * Represents a login attempt for auditing and security monitoring
 */
class LoginAttemptLog {
  constructor({
    id,
    timestamp,
    email,
    ipAddress,
    success,
    failureReason = null,
    userAgent = null
  }) {
    this.id = id;
    this.timestamp = timestamp || new Date();
    this.email = email;
    this.ipAddress = ipAddress;
    this.success = success;
    this.failureReason = failureReason;
    this.userAgent = userAgent;
  }
}

module.exports = LoginAttemptLog;
