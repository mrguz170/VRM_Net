namespace VRM_Plugin.Module.PanelAdmin.Domain;

/// <summary>
/// Entidad Rol del módulo PanelAdmin
/// </summary>
public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime? Fecha { get; set; }
    public string UserCreated { get; set; } = string.Empty;
}
