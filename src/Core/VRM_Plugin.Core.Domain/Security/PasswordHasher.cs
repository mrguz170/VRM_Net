using System.Globalization;
using System.Security.Cryptography;
using System.Text;

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

    /// <summary>
    /// Genera una contraseña temporal para nuevos usuarios
    /// Formato: Primeras 2 letras del nombre + Primeras 2 letras del apellido + Fecha actual (DDMMYYYY)
    /// Ejemplo: Para "Gustavo Bañuelos Ochoa" genera: GUBA06122025
    /// </summary>
    /// <param name="nombre">Nombre del usuario</param>
    /// <param name="apellido">Apellido del usuario</param>
    /// <returns>Contraseña temporal en texto plano</returns>
    public static string GenerateDefaultPassword(string nombre, string apellido)
    {
        // Normalizar y limpiar el nombre (primeras 2 letras)
        string nombreParte = NormalizarTexto(nombre)
            .ToUpper()
            .Substring(0, Math.Min(2, NormalizarTexto(nombre).Length))
            .PadRight(2, 'X');

        // Normalizar y limpiar el apellido (primeras 2 letras)
        string apellidoParte = NormalizarTexto(apellido)
            .ToUpper()
            .Substring(0, Math.Min(2, NormalizarTexto(apellido).Length))
            .PadRight(2, 'X');

        // Fecha actual en formato DDMMYYYY
        string fechaParte = DateTime.Now.ToString("ddMMyyyy");

        return $"{nombreParte}{apellidoParte}{fechaParte}";
    }

    /// <summary>
    /// Normaliza texto removiendo acentos y caracteres especiales
    /// </summary>
    private static string NormalizarTexto(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return "XX";

        // Remover espacios y caracteres no alfabéticos
        texto = new string(texto.Where(c => char.IsLetter(c)).ToArray());

        // Remover acentos
        string normalizado = texto.Normalize(NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();

        foreach (char c in normalizado)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Genera contraseña temporal y devuelve junto con su hash
    /// </summary>
    /// <returns>Tupla con (contraseña en texto plano, hash)</returns>
    public static (string PlainPassword, string HashedPassword) GenerateAndHashDefaultPassword(string nombre, string apellidoP)
    {
        var plainPassword = GenerateDefaultPassword(nombre, apellidoP);
        var hashedPassword = HashPassword(plainPassword);
        return (plainPassword, hashedPassword);
    }
}
