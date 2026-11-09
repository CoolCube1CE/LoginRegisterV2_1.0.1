using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;

namespace LoginRegisterV2
{
    public static class PasswordHelper
    {
        // Hashes a plain-text password into a "salt:hash" string safe to store in the DB
        public static string HashPassword(string password)
        {
            byte[] salt = new byte[16];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000))
            {
                byte[] hash = pbkdf2.GetBytes(32);
                return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
            }
        }

        // Compares a plain-text password against a stored "salt:hash" string
        public static bool VerifyPassword(string password, string storedValue)
        {
            if (string.IsNullOrEmpty(storedValue) || !storedValue.Contains(":"))
                return false;

            string[] parts = storedValue.Split(':');
            byte[] salt = Convert.FromBase64String(parts[0]);
            byte[] storedHash = Convert.FromBase64String(parts[1]);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000))
            {
                byte[] computedHash = pbkdf2.GetBytes(32);
                return CryptographicOperations_SlowEquals(storedHash, computedHash);
            }
        }

        // Constant-time comparison so login timing can't leak info about the password
        private static bool CryptographicOperations_SlowEquals(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int diff = 0;
            for (int i = 0; i < a.Length; i++)
                diff |= a[i] ^ b[i];
            return diff == 0;
        }
    }
}