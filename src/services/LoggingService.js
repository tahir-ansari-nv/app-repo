const LoginAttemptLog = require('../models/LoginAttemptLog');
const { v4: uuidv4 } = require('crypto');

/**
 * Logging Service
 * Captures and logs login attempts for auditing and security monitoring
 */
class LoggingService {
  constructor() {
    this.logs = [];
  }

  /**
   * Log a login attempt
   * @param {Object} attemptData - Login attempt data
   * @returns {LoginAttemptLog} Created log entry
   */
  logLoginAttempt({
    email,
    ipAddress,
    success,
    failureReason = null,
    userAgent = null
  }) {
    const log = new LoginAttemptLog({
      id: this._generateId(),
      timestamp: new Date(),
      email,
      ipAddress,
      success,
      failureReason,
      userAgent
    });

    this.logs.push(log);
    
    // In production, this would write to a persistent store or external logging service
    this._writeToConsole(log);
    
    return log;
  }

  /**
   * Get all logs (for demonstration/testing purposes)
   * @returns {Array<LoginAttemptLog>} All log entries
   */
  getLogs() {
    return this.logs;
  }

  /**
   * Get logs for a specific email
   * @param {string} email - User email
   * @returns {Array<LoginAttemptLog>} Log entries for the email
   */
  getLogsByEmail(email) {
    return this.logs.filter(log => log.email === email);
  }

  /**
   * Clear all logs (for testing purposes)
   */
  clearLogs() {
    this.logs = [];
  }

  /**
   * Generate a unique ID
   * @returns {string} Unique ID
   */
  _generateId() {
    return `${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
  }

  /**
   * Write log to console (in production, this would be more sophisticated)
   * @param {LoginAttemptLog} log - Log entry
   */
  _writeToConsole(log) {
    const status = log.success ? 'SUCCESS' : 'FAILED';
    const reason = log.failureReason ? ` - Reason: ${log.failureReason}` : '';
    
    console.log(
      `[${log.timestamp.toISOString()}] LOGIN ${status} - ` +
      `Email: ${log.email}, IP: ${log.ipAddress}${reason}`
    );
  }
}

module.exports = new LoggingService();
