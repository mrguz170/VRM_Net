using VRM_Plugin.Core.Abstractions.Common;
using VRM_Plugin.Modules.Finanzas.Data.DTOs;

namespace VRM_Plugin.Modules.Finanzas.Data.Repositories;

/// <summary>
/// Implementación del repositorio de Facturas
/// ? Usa DatabaseHelper de Common/ (homologado)
/// ? DatabaseHelper mapea snake_case (BD) ? PascalCase (DTO)
/// </summary>
public class FacturaRepository : IFacturaRepository
{
    private readonly DatabaseHelper _dbHelper;
    
    public FacturaRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }
    
    /// <summary>
    /// ? USO GENÉRICO: DatabaseHelper mapea automáticamente
    /// - id_factura ? IdFactura
    /// - fecha_emision ? FechaEmision
    /// - estado_factura ? EstadoFactura
    /// </summary>
    public async Task<List<FacturaDto>> GetAllAsync()
    {
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedure<FacturaDto>(
                "sp_Finanzas_Facturas_GetAll"));
    }
    
    public async Task<FacturaDto?> GetByIdAsync(int idFactura)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_id_factura"] = idFactura
        };
        
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedureSingle<FacturaDto>(
                "sp_Finanzas_Facturas_GetById", 
                parametros));
    }
    
    public async Task<List<FacturaDto>> GetByEstadoAsync(int estadoFactura)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_estado_factura"] = estadoFactura
        };
        
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedure<FacturaDto>(
                "sp_Finanzas_Facturas_GetByEstado", 
                parametros));
    }
    
    public async Task<List<FacturaDto>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_fecha_inicio"] = fechaInicio,
            ["p_fecha_fin"] = fechaFin
        };
        
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedure<FacturaDto>(
                "sp_Finanzas_Facturas_GetByFechas", 
                parametros));
    }
    
    public async Task<int> CreateAsync(CreateFacturaDto factura)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_folio"] = factura.Folio,
            ["p_fecha_emision"] = factura.FechaEmision,
            ["p_fecha_vencimiento"] = factura.FechaVencimiento ?? (object)DBNull.Value,
            ["p_rfc_cliente"] = factura.RfcCliente,
            ["p_nombre_cliente"] = factura.NombreCliente,
            ["p_subtotal"] = factura.Subtotal,
            ["p_impuestos"] = factura.Impuestos,
            ["p_total"] = factura.Total,
            ["p_notas_internas"] = factura.NotasInternas ?? (object)DBNull.Value,
            ["p_creado_por"] = factura.CreadoPor
        };
        
        // ExecuteScalar retorna el ID generado
        var resultado = await Task.Run(() => 
            _dbHelper.ExecuteScalar(
                "sp_Finanzas_Facturas_Insert", 
                parametros));
        
        return Convert.ToInt32(resultado ?? 0);
    }
    
    public async Task<bool> UpdateAsync(UpdateFacturaDto factura)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_id_factura"] = factura.IdFactura,
            ["p_notas_internas"] = factura.NotasInternas ?? (object)DBNull.Value,
            ["p_estado_factura"] = factura.EstadoFactura,
            ["p_actualizado_por"] = factura.ActualizadoPor
        };
        
        var filasAfectadas = await Task.Run(() => 
            _dbHelper.ExecuteNonQuery(
                "sp_Finanzas_Facturas_Update", 
                parametros));
        
        return filasAfectadas > 0;
    }
    
    public async Task<bool> TimbrarAsync(TimbrarFacturaDto datos)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_id_factura"] = datos.IdFactura,
            ["p_uuid_sat"] = datos.UuidSat,
            ["p_fecha_timbrado"] = datos.FechaTimbrado,
            ["p_timbrado_por"] = datos.TimbradoPor
        };
        
        var filasAfectadas = await Task.Run(() => 
            _dbHelper.ExecuteNonQuery(
                "sp_Finanzas_Facturas_Timbrar", 
                parametros));
        
        return filasAfectadas > 0;
    }
    
    public async Task<bool> CancelarAsync(int idFactura, string motivo, string canceladoPor)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_id_factura"] = idFactura,
            ["p_motivo"] = motivo,
            ["p_cancelado_por"] = canceladoPor
        };
        
        var filasAfectadas = await Task.Run(() => 
            _dbHelper.ExecuteNonQuery(
                "sp_Finanzas_Facturas_Cancelar", 
                parametros));
        
        return filasAfectadas > 0;
    }
    
    public async Task<decimal> GetTotalIngresosAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_fecha_inicio"] = fechaInicio,
            ["p_fecha_fin"] = fechaFin
        };
        
        var resultado = await Task.Run(() => 
            _dbHelper.ExecuteScalar(
                "sp_Finanzas_Facturas_GetTotalIngresos", 
                parametros));
        
        return Convert.ToDecimal(resultado ?? 0);
    }
}
