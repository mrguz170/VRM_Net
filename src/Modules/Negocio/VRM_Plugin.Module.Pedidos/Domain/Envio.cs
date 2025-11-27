namespace VRM_Plugin.Module.Pedidos.Domain;

/// <summary>
/// Entidad Envio del módulo Pedidos
/// </summary>
public class Envio
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
