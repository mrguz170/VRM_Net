namespace VRM_Plugin.Module.PanelAdmin.Domain;

/// <summary>
/// Entidad Usuario del módulo PanelAdmin
/// </summary>
public class Usuario
{
    public string Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string ApellidoPaterno { get; set; } = string.Empty;
    public string ApellidoMaterno { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    
    public DateTime FechaModificacion { get; set; }
    public string created_user_id { get; set; } = string.Empty;
    
}
