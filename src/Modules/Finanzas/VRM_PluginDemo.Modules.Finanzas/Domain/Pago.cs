namespace VRM_PluginDemo.Modules.Finanzas.Domain;

/// <summary>
/// Representa un pago realizado o por realizar.
/// </summary>
public class Pago
{
    public int Id { get; set; }
    
    public int? FacturaId { get; set; }
    
    public string NumeroReferencia { get; set; } = string.Empty;
    
    public DateTime FechaPago { get; set; }
    
    public decimal Monto { get; set; }
    
    public MetodoPago MetodoPago { get; set; }
    
    public string? NumeroTransaccion { get; set; }
    
    public string Concepto { get; set; } = string.Empty;
    
    public EstadoPago Estado { get; set; }
    
    public string? NotasInternas { get; set; }
    
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    
    public string RegistradoPor { get; set; } = string.Empty;
}
