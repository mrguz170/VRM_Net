namespace VRM_Plugin.Modules.Finanzas.Data.DTOs;

/// <summary>
/// DTO para transferencia de datos de Factura
/// Mapea directamente con las columnas de la tabla en BD 
/// </summary>
public class FacturaDto
{
    /// <summary>
    /// ID de la factura 
    /// </summary>
    public int IdFactura { get; set; }
    
    /// <summary>
    /// Folio/Número de factura 
    /// </summary>
    public string Folio { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de emisión 
    /// </summary>
    public DateTime FechaEmision { get; set; }
    
    /// <summary>
    /// Fecha de vencimiento 
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }
    
    /// <summary>
    /// RFC del cliente 
    /// </summary>
    public string RfcCliente { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del cliente 
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;
    
    /// <summary>
    /// Subtotal antes de impuestos 
    /// </summary>
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Total de impuestos 
    /// </summary>
    public decimal Impuestos { get; set; }
    
    /// <summary>
    /// Total final 
    /// </summary>
    public decimal Total { get; set; }
    
    /// <summary>
    /// Estado de la factura como int 
    /// 0=Borrador, 1=Emitida, 2=Timbrada, 3=Cancelada
    /// </summary>
    public int EstadoFactura { get; set; }
    
    /// <summary>
    /// UUID del SAT si está timbrada 
    /// </summary>
    public string? UuidSat { get; set; }
    
    /// <summary>
    /// Fecha de timbrado SAT 
    /// </summary>
    public DateTime? FechaTimbrado { get; set; }
    
    /// <summary>
    /// Notas internas 
    /// </summary>
    public string? NotasInternas { get; set; }
    
    /// <summary>
    /// Fecha de creación del registro 
    /// </summary>
    public DateTime FechaCreacion { get; set; }
    
    /// <summary>
    /// Usuario que creó el registro 
    /// </summary>
    public string CreadoPor { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de última actualización 
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }
    
    /// <summary>
    /// Usuario que actualizó
    /// </summary>
    public string? ActualizadoPor { get; set; }
    
    /// <summary>
    /// Indica si el registro está activo 
    /// </summary>
    public bool Activo { get; set; } = true;
}
