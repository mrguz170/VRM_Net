using VRM_Plugin.Core.Abstractions.Data.DTOs;

namespace VRM_Plugin.Core.Abstractions.Services;

/// <summary>
/// Repositorio para gestión de usuarios y autenticación
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Obtiene un usuario por su login/email y valida la contraseña
    /// </summary>
    UserDto? GetUserByLogin(string login, string password);


    Task<List<UserDto>>? GetAllUserAsync();

    Task<string> CreateUserAsync(UserDto dto);
}
