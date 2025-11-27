namespace VRM_Plugin.Core.Abstractions.Data.DTOs;

/// <summary>
/// DTO para transferir información de usuario entre capas
/// Usado para autenticación y gestión de usuarios
/// 
/// ? Ubicación: Data/DTOs/ (homologado con módulos)
/// </summary>
public class UserDto
{
    /// <summary>
    /// ID único del usuario (mapea a user_id en BD)
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre de usuario (login)
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Correo electrónico del usuario
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre completo del usuario
    /// </summary>
    public string NombreCompleto { get; set; } = string.Empty;
    
    /// <summary>
    /// Rol del usuario (UN SOLO ROL por usuario)
    /// Ejemplo: "Admin", "GerenteFinanzas", "Contador"
    /// </summary>
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// ID del permiso/rol (formato numérico del rol)
    /// Ejemplo: "1" para Admin, "2" para GerenteFinanzas
    /// </summary>
    public string RoleId { get; set; } = string.Empty;

    /// <summary>
    /// Password del usuario (hasheada)
    /// 
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
