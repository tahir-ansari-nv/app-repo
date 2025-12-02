const config = require('../config');

/**
 * Simple in-memory rate limiter
 * In production, use a proper rate limiting library like express-rate-limit
 * or a distributed solution with Redis
 */
class RateLimiter {
  constructor() {
    this.requests = new Map();
  }

  /**
   * Rate limiting middleware
   */
  middleware = (req, res, next) => {
    const ip = req.ip || req.connection.remoteAddress;
    const now = Date.now();
    const windowMs = config.rateLimit.windowMs;
    const maxRequests = config.rateLimit.maxRequests;

    // Get or create request log for this IP
    if (!this.requests.has(ip)) {
      this.requests.set(ip, []);
    }

    const requestLog = this.requests.get(ip);

    // Remove old requests outside the time window
    const recentRequests = requestLog.filter(
      timestamp => now - timestamp < windowMs
    );

    // Check if rate limit exceeded
    if (recentRequests.length >= maxRequests) {
      return res.status(429).json({
        error: 'Too many requests',
        message: 'Too many login attempts. Please try again later.',
        retryAfter: Math.ceil((recentRequests[0] + windowMs - now) / 1000)
      });
    }

    // Add current request
    recentRequests.push(now);
    this.requests.set(ip, recentRequests);

    next();
  };

  /**
   * Clear all rate limit data (for testing)
   */
  clear() {
    this.requests.clear();
  }
}

module.exports = new RateLimiter();
