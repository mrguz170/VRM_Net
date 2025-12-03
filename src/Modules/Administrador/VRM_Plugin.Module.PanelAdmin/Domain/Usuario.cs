namespace VRM_Plugin.Module.PanelAdmin.Domain;

/// <summary>
/// Entidad Usuario del módulo PanelAdmin
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int Active { get; set; } = 0;
    public string Role { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
