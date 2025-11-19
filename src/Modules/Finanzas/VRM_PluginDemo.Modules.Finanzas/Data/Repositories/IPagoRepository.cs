using VRM_Plugin.Modules.Finanzas.Data.DTOs;

namespace VRM_Plugin.Modules.Finanzas.Data.Repositories;

/// <summary>
/// Repositorio para acceso a datos de Pagos
/// </summary>
public interface IPagoRepository
{
    /// <summary>
    /// Obtiene todos los pagos activos
    /// SP: sp_Finanzas_Pagos_GetAll
    /// </summary>
    Task<List<PagoDto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un pago por ID
    /// SP: sp_Finanzas_Pagos_GetById
    /// </summary>
    Task<PagoDto?> GetByIdAsync(int idPago);
    
    /// <summary>
    /// Obtiene pagos asociados a una factura
    /// SP: sp_Finanzas_Pagos_GetByFactura
    /// </summary>
    Task<List<PagoDto>> GetByFacturaAsync(int idFactura);
    
    /// <summary>
    /// Obtiene pagos por rango de fechas
    /// SP: sp_Finanzas_Pagos_GetByFechas
    /// </summary>
    Task<List<PagoDto>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Crea un nuevo pago
    /// SP: sp_Finanzas_Pagos_Insert
    /// Retorna el ID generado
    /// </summary>
    Task<int> CreateAsync(CreatePagoDto pago);
    
    /// <summary>
    /// Actualiza un pago existente
    /// SP: sp_Finanzas_Pagos_Update
    /// </summary>
    Task<bool> UpdateAsync(PagoDto pago);
    
    /// <summary>
    /// Cancela un pago
    /// SP: sp_Finanzas_Pagos_Cancelar
    /// </summary>
    Task<bool> CancelarAsync(int idPago, string motivo, string canceladoPor);
}
