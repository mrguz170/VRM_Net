namespace VRM_Plugin.Core.Abstractions.Services;

/// <summary>
/// Servicio para obtener información del usuario autenticado actual
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Obtiene el ID del usuario autenticado actual
    /// </summary>
    Task<string?> GetUserIdAsync();
    
    /// <summary>
    /// Obtiene el nombre de usuario autenticado actual
    /// </summary>
    Task<string?> GetUsernameAsync();
    
    /// <summary>
    /// Obtiene el email del usuario autenticado actual
    /// </summary>
    Task<string?> GetEmailAsync();
    
    /// <summary>
    /// Obtiene el rol del usuario autenticado actual
    /// </summary>
    Task<string?> GetRoleAsync();
    
    /// <summary>
    /// Verifica si hay un usuario autenticado
    /// </summary>
    Task<bool> IsAuthenticatedAsync();
}
