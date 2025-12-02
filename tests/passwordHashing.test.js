const passwordHashing = require('../src/utils/passwordHashing');

describe('PasswordHashingService', () => {
  describe('hash', () => {
    it('should hash a password', async () => {
      const password = 'testpassword123';
      const hash = await passwordHashing.hash(password);

      expect(hash).toBeDefined();
      expect(typeof hash).toBe('string');
      expect(hash).not.toBe(password);
      expect(hash.length).toBeGreaterThan(0);
    });

    it('should generate different hashes for the same password', async () => {
      const password = 'testpassword123';
      const hash1 = await passwordHashing.hash(password);
      const hash2 = await passwordHashing.hash(password);

      // Bcrypt includes salt, so hashes should be different
      expect(hash1).not.toBe(hash2);
    });
  });

  describe('verify', () => {
    it('should verify a correct password', async () => {
      const password = 'testpassword123';
      const hash = await passwordHashing.hash(password);
      const isValid = await passwordHashing.verify(password, hash);

      expect(isValid).toBe(true);
    });

    it('should reject an incorrect password', async () => {
      const password = 'testpassword123';
      const wrongPassword = 'wrongpassword';
      const hash = await passwordHashing.hash(password);
      const isValid = await passwordHashing.verify(wrongPassword, hash);

      expect(isValid).toBe(false);
    });

    it('should reject empty password', async () => {
      const password = 'testpassword123';
      const hash = await passwordHashing.hash(password);
      const isValid = await passwordHashing.verify('', hash);

      expect(isValid).toBe(false);
    });
  });
});
