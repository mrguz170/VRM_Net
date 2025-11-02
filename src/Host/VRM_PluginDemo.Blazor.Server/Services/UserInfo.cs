namespace VRM_Plugin.Blazor.Server.Services;

/// <summary>
/// DTO para serializar/deserializar información del usuario entre SSR e Interactive Server.
/// Se usa con PersistentComponentState para mantener el estado de autenticación.
/// </summary>
public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string ClienteId { get; set; } = string.Empty;
  public List<string> Roles { get; set; } = new();
}
