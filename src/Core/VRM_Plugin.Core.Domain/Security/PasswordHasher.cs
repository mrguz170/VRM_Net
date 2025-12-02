using System.Security.Cryptography;

namespace VRM_Plugin.Blazor.Server.Security;

/// <summary>
/// Utilidad para hashing seguro de contraseñas usando PBKDF2 (Password-Based Key Derivation Function 2)
/// NOTA: Solo para uso interno autenticación del sistema
/// </summary>
public static class PasswordHasher
{
    private const int SaltSize = 16;      // 128 bits
    private const int HashSize = 32;      // 256 bits
    private const int Iterations = 10000; // PBKDF2 iterations

    /// <summary>
    /// Genera un hash seguro de una contraseña con salt aleatorio
    /// </summary>
    /// <param name="password">Contraseña en texto plano</param>
    /// <returns>Hash en Base64 (salt + hash combinados)</returns>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("La contraseña no puede estar vacía", nameof(password));

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

    /// <summary>
    /// Verifica si una contraseña coincide con el hash almacenado
    /// </summary>
    /// <param name="password">Contraseña en texto plano a verificar</param>
    /// <param name="storedHash">Hash almacenado en Base64</param>
    /// <returns>True si la contraseña coincide, false en caso contrario</returns>
    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password))
            return false;
        
        if (string.IsNullOrWhiteSpace(storedHash))
            return false;

        try
        {
            var combinedBytes = Convert.FromBase64String(storedHash);
            
            // Validar longitud esperada
            if (combinedBytes.Length != SaltSize + HashSize)
                return false;
            
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
        catch (FormatException)
        {
            // Hash en formato inválido
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
