using VRM_Plugin.Modules.Finanzas.Domain;

namespace VRM_Plugin.Modules.Finanzas.Services;

/// <summary>
/// Servicio para gestionar pagos.
/// </summary>
public interface IPagoService
{
    /// <summary>
    /// Obtiene todos los pagos.
    /// </summary>
    Task<List<Pago>> GetPagosAsync();
    
    /// <summary>
    /// Obtiene un pago por su ID.
    /// </summary>
    Task<Pago?> GetPagoByIdAsync(int id);
    
    /// <summary>
    /// Registra un nuevo pago.
    /// </summary>
    Task<Pago> RegistrarPagoAsync(Pago pago);
    
    /// <summary>
    /// Obtiene pagos de una factura específica.
    /// </summary>
    Task<List<Pago>> GetPagosByFacturaIdAsync(int facturaId);
    
    /// <summary>
    /// Obtiene pagos pendientes.
    /// </summary>
    Task<List<Pago>> GetPagosPendientesAsync();
}
