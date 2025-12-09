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

    /// <summary>
    /// Crea un nuevo usuario
    /// Llama al SP: sp_set_user con opc=1
    /// </summary>
    public async Task<string> CreateUserAsync(UserDto dto)
    {
        return await Task.Run(() =>
        {
            var parameters = new Dictionary<string, object>
            {
                { "opc", 1 },  // 1 = Crear
                { "user_id_param", DBNull.Value },  // NULL para creación
                { "email_user", dto.Email },
                { "username", dto.Username },
                { "role_user_id", dto.RoleId },
                { "name_user", dto.Nombre },
                { "last_name_user", dto.ApellidoPaterno },
                { "second_last_name_user", dto.ApellidoMaterno },
                { "password_user", dto.Password },
                { "is_active_user", dto.IsActive },
                { "creator_user", dto.created_user_id },
                { "updater_user", DBNull.Value }  // NULL para creación
            };

            var dataTable = _db.ExecuteStoredProcedure("sp_set_user", parameters);

            if (dataTable.Rows.Count == 0)
            {
                _logger.LogWarning("SP sp_set_user no devolvió resultados");
                return string.Empty;
            }

            var newUserId = dataTable.Rows[0]["UserId"]?.ToString() ?? string.Empty;
            var operation = dataTable.Rows[0]["Operation"]?.ToString() ?? string.Empty;

            _logger.LogInformation("Usuario creado con ID: {UserId}, Operación: {Operation}", newUserId, operation);

            return newUserId;
        });
    }
    /// <summary>
    /// Actualiza un usuario existente
    /// Llama al SP: sp_set_user con opc=2
    /// </summary>
    public async Task<string> UpdateUserAsync(UserDto dto)
    {
        return await Task.Run(() =>
        {
            var parameters = new Dictionary<string, object>
            {
                { "opc", 2 },  // 2 = Actualizar
                { "user_id_param", dto.UserId },  // ID del usuario a actualizar
                { "email_user", dto.Email },
                { "username", dto.Username },
                { "role_user_id", dto.RoleId },
                { "name_user", dto.Nombre },
                { "last_name_user", dto.ApellidoPaterno },
                { "second_last_name_user", dto.ApellidoMaterno },
                { "password_user", dto.Password ?? (object)DBNull.Value },  // NULL si no se actualiza password
                { "is_active_user", dto.IsActive },
                { "creator_user", DBNull.Value },  // NULL para actualización
                { "updater_user", dto.updated_user_id }  // Usuario que actualiza
            };

            var dataTable = _db.ExecuteStoredProcedure("sp_set_user", parameters);

            if (dataTable.Rows.Count == 0)
            {
                _logger.LogWarning("SP sp_set_user no devolvió resultados");
                return string.Empty;
            }

            var userId = dataTable.Rows[0]["UserId"]?.ToString() ?? string.Empty;
            var operation = dataTable.Rows[0]["Operation"]?.ToString() ?? string.Empty;

            _logger.LogInformation("Usuario actualizado con ID: {UserId}, Operación: {Operation}", userId, operation);

            return userId;
        });
    }

    //public async Task<string> CreateUserAsync(UserDto dto)
    //{
    //    return await Task.Run(() =>
    //    {
    //        var parameters = new Dictionary<string, object>
    //    {
    //        { "email_user", dto.Email },
    //        { "username", dto.Username },
    //        { "role_user_id", dto.RoleId },
    //        { "name_user", dto.Nombre },
    //        { "last_name_user", dto.ApellidoPaterno },
    //        { "second_last_name_user", dto.ApellidoMaterno },
    //        { "password_user", dto.Password },
    //        { "is_active_user", dto.IsActive },
    //        { "creator_user", dto.created_user_id }
    //    };

    //        var dataTable = _db.ExecuteStoredProcedure("sp_set_user", parameters);

    //        if (dataTable.Rows.Count == 0)
    //        {
    //            _logger.LogWarning("SP sp_set_user no devolvió resultados");
    //            return string.Empty;
    //        }

    //        var newUserId = dataTable.Rows[0]["NewUserId"]?.ToString() ?? string.Empty;

    //        _logger.LogInformation("Usuario creado con ID: {UserId}", newUserId);

    //        return newUserId;
    //    });
    //}
}

