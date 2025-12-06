using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VRM_Plugin.Blazor.Server.Security;
using VRM_Plugin.Core.Abstractions.Common;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Core.Abstractions.Services;


namespace VRM_Plugin.Core.Abstractions.Data.Repositories;

/// <summary>
/// Repositorio de usuarios (infraestructura del SISTEMA)
/// Ubicación: Data/Repositories/ 
/// Usa DatabaseHelper de Common/
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
    /// Obtiene un usuario por login y valida contraseñ
    /// Llama al SP: sp_get_user
    /// 
    /// El SP debe devolver columnas:
    /// - userid ? UserDto.UserId
    /// - username o username ? UserDto.Username
    /// - email ? UserDto.Email
    /// - NombreCompleto ? UserDto.NombreCompleto
    /// - role ? UserDto.Role (UN SOLO ROL como string, ej: "Admin")
    /// - RoleId ? UserDto.RoleId (ID del rol/permiso)
    /// - password ? Para validación
    /// </summary>
    public UserDto? GetUserByLogin(string login, string password)
    {
        try
        {
            _logger.LogDebug("Obteniendo usuario por login: {Login}", login);
            
            // Usar parseador genérico
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
            
            // VALIDACIÓN DE CONTRASEÑA           
            var hashedPassword = user.Password; 
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

    public async Task<List<UserDto>> GetAllUserAsync()
    {
        return await Task.Run(() =>
            _db.ExecuteStoredProcedure<UserDto>(
                "sp_get_all_user"));
    }

    public async Task<string> CreateUserAsync(UserDto dto)
    {
        return await Task.Run(() =>
        {
            var parameters = new Dictionary<string, object>
        {
            { "email_user", dto.Email },
            { "username", dto.Username },
            { "role_user_id", dto.RoleId },
            { "name_user", dto.Nombre },
            { "last_name_user", dto.ApellidoPaterno },
            { "second_last_name_user", dto.ApellidoMaterno },
            { "password_user", dto.Password },
            { "is_active_user", dto.IsActive },
            { "creator_user", dto.created_user_id }
        };

            var dataTable = _db.ExecuteStoredProcedure("sp_set_user", parameters);

            if (dataTable.Rows.Count == 0)
            {
                _logger.LogWarning("SP sp_set_user no devolvió resultados");
                return string.Empty;
            }

            var newUserId = dataTable.Rows[0]["NewUserId"]?.ToString() ?? string.Empty;

            _logger.LogInformation("Usuario creado con ID: {UserId}", newUserId);

            return newUserId;
        });
    }
}

