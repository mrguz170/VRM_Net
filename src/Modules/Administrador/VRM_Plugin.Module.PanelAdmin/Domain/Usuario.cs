namespace VRM_Plugin.Module.PanelAdmin.Domain;

/// <summary>
/// Entidad Usuario del módulo PanelAdmin
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
