namespace VRM_Plugin.Module.Viaticos.Domain;

/// <summary>
/// Entidad Anticipo del módulo Viaticos
/// </summary>
public class Anticipo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
