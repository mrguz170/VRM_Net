namespace VRM_PluginDemo.Modules.Finanzas.Domain;

/// <summary>
/// Representa una factura en el sistema.
/// </summary>
public class Factura
{
    public int Id { get; set; }
    
    public string Numero { get; set; } = string.Empty;
    
    public DateTime FechaEmision { get; set; }
    
    public DateTime? FechaVencimiento { get; set; }
    
    public string RfcCliente { get; set; } = string.Empty;
    
    public string NombreCliente { get; set; } = string.Empty;
    
    public decimal Subtotal { get; set; }
    
    public decimal Impuestos { get; set; }
    
    public decimal Total { get; set; }
    
    public EstadoFactura Estado { get; set; }
    
    public string? NotasInternas { get; set; }
    
    public List<ConceptoFactura> Conceptos { get; set; } = new();
    
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    public string CreadoPor { get; set; } = string.Empty;
}
