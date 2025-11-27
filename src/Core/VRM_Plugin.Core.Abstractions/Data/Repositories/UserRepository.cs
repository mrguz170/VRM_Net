using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VRM_Plugin.Blazor.Server.Security;
using VRM_Plugin.Core.Abstractions.Common;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Core.Abstractions.Services;


namespace VRM_Plugin.Core.Abstractions.Data.Repositories;

/// <summary>
/// Repositorio de usuarios (infraestructura del SISTEMA)
/// 
/// ? Ubicación: Data/Repositories/ (homologado con módulos)
/// ? Usa DatabaseHelper de Common/
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly DatabaseHelper _db;
    private readonly ILogger<UserRepository> _logger;
    
    public UserRepository(
        IConfiguration configuration,
        ILogger<UserRepository> logger)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found");
        
        _db = new DatabaseHelper(connectionString);
        _logger = logger;
    }
    
    /// <summary>
    /// Obtiene un usuario por login y valida contraseña
    /// Llama al SP: sp_get_user
    /// 
    /// El SP debe devolver columnas:
    /// - user_id ? UserDto.UserId
    /// - user_name o username ? UserDto.Username
    /// - email ? UserDto.Email
    /// - NombreCompleto ? UserDto.NombreCompleto
    /// - role ? UserDto.Role (UN SOLO ROL como string, ej: "Admin")
    /// - permission ? UserDto.Permission (ID del rol/permiso)
    /// - password (opcional) ? Para validación
    /// </summary>
    public UserDto? GetUserByLogin(string login, string password)
    {
        try
        {
            _logger.LogDebug("Obteniendo usuario por login: {Login}", login);
            
            // ? Usar parseador genérico
            var users = _db.ExecuteStoredProcedure<UserDto>("sp_get_user", new Dictionary<string, object>
            {
                { "login", login }
            });
            
            if (!users.Any())
            {
                _logger.LogWarning("Usuario no encontrado: {Login}", login);
                return null;
            }
            
            var user = users.First();
            
            // ? VALIDACIÓN DE CONTRASEÑA
            // Opción 1: Si el SP ya validó la contraseña, omitir esta sección
            // Opción 2: Si el SP devuelve el hash en una columna "password", descomentar:
            
            var hashedPassword = user.Password; // Necesitarías agregar propiedad temporal en UserDto
            if (!PasswordHasher.VerifyPassword(password, hashedPassword))
            {
                _logger.LogWarning("Contraseña inválida para usuario: {Login}", login);
                return null;
            }
            
            
            _logger.LogInformation("Usuario autenticado exitosamente: {Username} con rol: {Role}", 
                user.Username, 
                user.Role);
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario por login: {Login}", login);
            return null;
        }
    }
}

