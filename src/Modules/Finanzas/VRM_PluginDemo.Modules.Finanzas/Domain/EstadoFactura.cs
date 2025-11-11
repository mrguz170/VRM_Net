namespace VRM_Plugin.Modules.Finanzas.Domain;

/// <summary>
/// Estados posibles de una factura.
/// </summary>
public enum EstadoFactura
{
    /// <summary>
    /// Factura creada pero no enviada.
    /// </summary>
    Borrador = 0,
    
    /// <summary>
    /// Factura enviada al cliente, pendiente de pago.
    /// </summary>
    Pendiente = 1,
    
    /// <summary>
    /// Factura timbrada en el SAT, pendiente de pago.
    /// </summary>
    Timbrada = 2,
    
    /// <summary>
    /// Factura pagada parcialmente.
    /// </summary>
    PagoParcial = 3,
    
    /// <summary>
    /// Factura pagada completamente.
    /// </summary>
    Pagada = 4,
    
    /// <summary>
    /// Factura vencida sin pago.
    /// </summary>
    Vencida = 5,
    
    /// <summary>
    /// Factura cancelada.
    /// </summary>
    Cancelada = 6
}
