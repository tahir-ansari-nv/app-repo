const TokenService = require('../src/services/TokenService');
const config = require('../src/config');

describe('TokenService', () => {
  describe('generateToken', () => {
    it('should generate a valid JWT token', () => {
      const user = {
        id: '123',
        email: 'test@example.com',
        roles: ['user']
      };

      const result = TokenService.generateToken(user);

      expect(result).toHaveProperty('token');
      expect(result).toHaveProperty('expiresAt');
      expect(typeof result.token).toBe('string');
      expect(result.token.length).toBeGreaterThan(0);
    });

    it('should include user information in token payload', () => {
      const user = {
        id: '123',
        email: 'test@example.com',
        roles: ['admin', 'user']
      };

      const result = TokenService.generateToken(user);
      const decoded = TokenService.verifyToken(result.token);

      expect(decoded.sub).toBe(user.email);
      expect(decoded.userId).toBe(user.id);
      expect(decoded.roles).toEqual(user.roles);
    });

    it('should set expiration date', () => {
      const user = {
        id: '123',
        email: 'test@example.com',
        roles: []
      };

      const result = TokenService.generateToken(user);
      const expiresAt = new Date(result.expiresAt);
      const now = new Date();

      expect(expiresAt.getTime()).toBeGreaterThan(now.getTime());
    });
  });

  describe('verifyToken', () => {
    it('should verify a valid token', () => {
      const user = {
        id: '123',
        email: 'test@example.com',
        roles: ['user']
      };

      const { token } = TokenService.generateToken(user);
      const decoded = TokenService.verifyToken(token);

      expect(decoded).toBeDefined();
      expect(decoded.sub).toBe(user.email);
    });

    it('should throw error for invalid token', () => {
      expect(() => {
        TokenService.verifyToken('invalid-token');
      }).toThrow();
    });

    it('should throw error for tampered token', () => {
      const user = {
        id: '123',
        email: 'test@example.com',
        roles: ['user']
      };

      const { token } = TokenService.generateToken(user);
      const tamperedToken = token.slice(0, -1) + 'X';

      expect(() => {
        TokenService.verifyToken(tamperedToken);
      }).toThrow();
    });
  });
});
