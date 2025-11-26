namespace VRM_Plugin.Modules.Finanzas.Data.DTOs;

/// <summary>
/// DTO para transferencia de datos de Pago
/// Mapea directamente con las columnas de la tabla en BD
/// </summary>
public class PagoDto
{
    /// <summary>
    /// ID del pago (PK en BD: id_pago)
    /// </summary>
    public int IdPago { get; set; }
    
    /// <summary>
    /// ID de la factura relacionada (FK: id_factura)
    /// </summary>
    public int? IdFactura { get; set; }
    
    /// <summary>
    /// Número de referencia del pago (BD: numero_referencia)
    /// </summary>
    public string NumeroReferencia { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha del pago (BD: fecha_pago)
    /// </summary>
    public DateTime FechaPago { get; set; }
    
    /// <summary>
    /// Monto del pago (BD: monto)
    /// </summary>
    public decimal Monto { get; set; }
    
    /// <summary>
    /// Método de pago como int (BD: metodo_pago)
    /// 0=Efectivo, 1=Transferencia, 2=Cheque, 3=TarjetaCredito, 4=TarjetaDebito
    /// </summary>
    public int MetodoPago { get; set; }
    
    /// <summary>
    /// Número de transacción bancaria (BD: numero_transaccion)
    /// </summary>
    public string? NumeroTransaccion { get; set; }
    
    /// <summary>
    /// Concepto del pago (BD: concepto)
    /// </summary>
    public string Concepto { get; set; } = string.Empty;
    
    /// <summary>
    /// Estado del pago como int (BD: estado_pago)
    /// 0=Pendiente, 1=Aplicado, 2=Cancelado
    /// </summary>
    public int EstadoPago { get; set; }
    
    /// <summary>
    /// Notas internas (BD: notas_internas)
    /// </summary>
    public string? NotasInternas { get; set; }
    
    /// <summary>
    /// Fecha de registro (BD: fecha_registro)
    /// </summary>
    public DateTime FechaRegistro { get; set; }
    
    /// <summary>
    /// Usuario que registró (BD: registrado_por)
    /// </summary>
    public string RegistradoPor { get; set; } = string.Empty;
    
    /// <summary>
    /// Fecha de actualización (BD: fecha_actualizacion)
    /// </summary>
    public DateTime? FechaActualizacion { get; set; }
    
    /// <summary>
    /// Usuario que actualizó (BD: actualizado_por)
    /// </summary>
    public string? ActualizadoPor { get; set; }
    
    /// <summary>
    /// Indica si está activo (BD: activo)
    /// </summary>
    public bool Activo { get; set; } = true;
}

/// <summary>
/// DTO para crear un nuevo pago
/// </summary>
public class CreatePagoDto
{
    public int? IdFactura { get; set; }
    public string NumeroReferencia { get; set; } = string.Empty;
    public DateTime FechaPago { get; set; } = DateTime.Now;
    public decimal Monto { get; set; }
    public int MetodoPago { get; set; }
    public string? NumeroTransaccion { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public string? NotasInternas { get; set; }
    public string RegistradoPor { get; set; } = string.Empty;
}
