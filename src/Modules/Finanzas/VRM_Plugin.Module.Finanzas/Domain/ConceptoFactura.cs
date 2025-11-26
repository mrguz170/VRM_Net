namespace VRM_Plugin.Modules.Finanzas.Domain;

/// <summary>
/// Representa un concepto o línea de una factura.
/// </summary>
public class ConceptoFactura
{
    public int Id { get; set; }
    
    public int FacturaId { get; set; }
    
    public string Descripcion { get; set; } = string.Empty;
    
    public decimal Cantidad { get; set; }
    
    public string UnidadMedida { get; set; } = "Pieza";
    
    public decimal PrecioUnitario { get; set; }
    
    public decimal Descuento { get; set; }
    
    public decimal Subtotal => (Cantidad * PrecioUnitario) - Descuento;
    
    public decimal TasaImpuesto { get; set; } = 0.16m; // IVA 16%
    
    public decimal Impuestos => Subtotal * TasaImpuesto;
    
    public decimal Total => Subtotal + Impuestos;
}
