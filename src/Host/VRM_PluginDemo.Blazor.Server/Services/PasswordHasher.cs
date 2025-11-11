using System.Security.Cryptography;

namespace VRM_Plugin.Blazor.Server.Services
{
    public class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public static string HashPassword(string password)
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] salt = new byte[SaltSize];
                rng.GetBytes(salt);

                using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
                {
                    var hash = pbkdf2.GetBytes(HashSize);
                    var combinedBytes = new byte[SaltSize + HashSize];
                    Array.Copy(salt, 0, combinedBytes, 0, SaltSize);
                    Array.Copy(hash, 0, combinedBytes, SaltSize, HashSize);
                    return Convert.ToBase64String(combinedBytes);
                }
            }
        }

        public static bool VerifyPassword(string password, string storedHash)
        {
            var combinedBytes = Convert.FromBase64String(storedHash);
            var salt = new byte[SaltSize];
            var hash = new byte[HashSize];
            Array.Copy(combinedBytes, 0, salt, 0, SaltSize);
            Array.Copy(combinedBytes, SaltSize, hash, 0, HashSize);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                var computedHash = pbkdf2.GetBytes(HashSize);
                return computedHash.SequenceEqual(hash);
            }
        }
    }
}
