using VRM_Plugin.Modules.Finanzas.Data.DTOs;

namespace VRM_Plugin.Modules.Finanzas.Data.Repositories;

/// <summary>
/// Repositorio para acceso a datos de Facturas
/// Usa DatabaseHelper para ejecutar SPs de forma genérica
/// </summary>
public interface IFacturaRepository
{
    /// <summary>
    /// Obtiene todas las facturas activas
    /// SP: sp_Finanzas_Facturas_GetAll
    /// </summary>
    Task<List<FacturaDto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene una factura por ID
    /// SP: sp_Finanzas_Facturas_GetById
    /// </summary>
    Task<FacturaDto?> GetByIdAsync(int idFactura);
    
    /// <summary>
    /// Obtiene facturas por estado
    /// SP: sp_Finanzas_Facturas_GetByEstado
    /// </summary>
    Task<List<FacturaDto>> GetByEstadoAsync(int estadoFactura);
    
    /// <summary>
    /// Obtiene facturas por rango de fechas
    /// SP: sp_Finanzas_Facturas_GetByFechas
    /// </summary>
    Task<List<FacturaDto>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin);
    
    /// <summary>
    /// Crea una nueva factura
    /// SP: sp_Finanzas_Facturas_Insert
    /// Retorna el ID generado
    /// </summary>
    Task<int> CreateAsync(CreateFacturaDto factura);
    
    /// <summary>
    /// Actualiza una factura existente
    /// SP: sp_Finanzas_Facturas_Update
    /// </summary>
    Task<bool> UpdateAsync(UpdateFacturaDto factura);
    
    /// <summary>
    /// Timbra una factura en el SAT
    /// SP: sp_Finanzas_Facturas_Timbrar
    /// </summary>
    Task<bool> TimbrarAsync(TimbrarFacturaDto datos);
    
    /// <summary>
    /// Cancela una factura
    /// SP: sp_Finanzas_Facturas_Cancelar
    /// </summary>
    Task<bool> CancelarAsync(int idFactura, string motivo, string canceladoPor);
    
    /// <summary>
    /// Obtiene el total de ingresos en un periodo
    /// SP: sp_Finanzas_Facturas_GetTotalIngresos
    /// </summary>
    Task<decimal> GetTotalIngresosAsync(DateTime fechaInicio, DateTime fechaFin);
}
