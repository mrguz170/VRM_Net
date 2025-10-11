using VRM_PluginDemo.Modules.Finanzas.Domain;

namespace VRM_PluginDemo.Modules.Finanzas.Services;

/// <summary>
/// Servicio para gestionar facturas.
/// </summary>
public interface IFacturaService
{
    /// <summary>
    /// Obtiene todas las facturas.
    /// </summary>
    Task<List<Factura>> GetFacturasAsync();
    
    /// <summary>
    /// Obtiene una factura por su ID.
    /// </summary>
    Task<Factura?> GetFacturaByIdAsync(int id);
    
    /// <summary>
    /// Crea una nueva factura.
    /// </summary>
    Task<Factura> CreateFacturaAsync(Factura factura);
    
    /// <summary>
    /// Actualiza una factura existente.
    /// </summary>
    Task<Factura> UpdateFacturaAsync(Factura factura);
    
    /// <summary>
    /// Cancela una factura.
    /// </summary>
    Task<bool> CancelarFacturaAsync(int id, string motivo);
    
    /// <summary>
    /// Obtiene facturas por estado.
    /// </summary>
    Task<List<Factura>> GetFacturasByEstadoAsync(EstadoFactura estado);
    
    /// <summary>
    /// Obtiene el total de ingresos en un periodo.
    /// </summary>
    Task<decimal> GetTotalIngresosAsync(DateTime fechaInicio, DateTime fechaFin);
}
