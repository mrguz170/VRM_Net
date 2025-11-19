namespace VRM_Plugin.Modules.Finanzas.Data.DTOs;

/// <summary>
/// DTO para transferencia de datos de Factura
/// Mapea directamente con las columnas de la tabla en BD (snake_case)
/// </summary>
public class FacturaDto
{
    /// <summary>
    /// ID de la factura (PK en BD: id_factura)
    /// </summary>
    public int IdFactura { get; set; }
    
    /// <summary>
    /// Folio/Número de factura (BD: folio)
    /// </summary>
    public string Folio { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de emisión (BD: fecha_emision)
    /// </summary>
    public DateTime FechaEmision { get; set; }
    
    /// <summary>
    /// Fecha de vencimiento (BD: fecha_vencimiento)
    /// </summary>
    public DateTime? FechaVencimiento { get; set; }
    
    /// <summary>
    /// RFC del cliente (BD: rfc_cliente)
    /// </summary>
    public string RfcCliente { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre del cliente (BD: nombre_cliente)
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;
    
    /// <summary>
    /// Subtotal antes de impuestos (BD: subtotal)
    /// </summary>
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Total de impuestos (BD: impuestos)
    /// </summary>
    public decimal Impuestos { get; set; }
    
    /// <summary>
    /// Total final (BD: total)
    /// </summary>
    public decimal Total { get; set; }
    
    /// <summary>
    /// Estado de la factura como int (BD: estado_factura)
    /// 0=Borrador, 1=Emitida, 2=Timbrada, 3=Cancelada
    /// </summary>
    public int EstadoFactura { get; set; }
    
    /// <summary>
    /// UUID del SAT si está timbrada (BD: uuid_sat)
    /// </summary>
    public string? UuidSat { get; set; }
    
    /// <summary>
    /// Fecha de timbrado SAT (BD: fecha_timbrado)
    /// </summary>
    public DateTime? FechaTimbrado { get; set; }
    
    /// <summary>
    /// Notas internas (BD: notas_internas)
    /// </summary>
    public string? NotasInternas { get; set; }
    
    /// <summary>
    /// Fecha de creación del registro (BD: fecha_creacion)
    /// </summary>
    public DateTime FechaCreacion { get; set; }
    
    /// <summary>
    /// Usuario que creó el registro (BD: creado_por)
    /// </summary>
    public string CreadoPor { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de última actualización (BD: fecha_actualizacion)
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }
    
    /// <summary>
    /// Usuario que actualizó (BD: actualizado_por)
    /// </summary>
    public string? ActualizadoPor { get; set; }
    
    /// <summary>
    /// Indica si el registro está activo (BD: activo)
    /// </summary>
    public bool Activo { get; set; } = true;
}
