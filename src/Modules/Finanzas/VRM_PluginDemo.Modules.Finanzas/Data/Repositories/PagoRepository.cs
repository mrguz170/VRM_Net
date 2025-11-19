using VRM_Plugin.Core.Abstractions.Common;
using VRM_Plugin.Modules.Finanzas.Data.DTOs;

namespace VRM_Plugin.Modules.Finanzas.Data.Repositories;

/// <summary>
/// Implementación del repositorio de Pagos
/// ? Usa DatabaseHelper de Common/ (homologado)
/// </summary>
public class PagoRepository : IPagoRepository
{
    private readonly DatabaseHelper _dbHelper;
    
    public PagoRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }
    
    public async Task<List<PagoDto>> GetAllAsync()
    {
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedure<PagoDto>(
                "sp_Finanzas_Pagos_GetAll"));
    }
    
    public async Task<PagoDto?> GetByIdAsync(int idPago)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_id_pago"] = idPago
        };
        
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedureSingle<PagoDto>(
                "sp_Finanzas_Pagos_GetById", 
                parametros));
    }
    
    public async Task<List<PagoDto>> GetByFacturaAsync(int idFactura)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_id_factura"] = idFactura
        };
        
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedure<PagoDto>(
                "sp_Finanzas_Pagos_GetByFactura", 
                parametros));
    }
    
    public async Task<List<PagoDto>> GetByRangoFechasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_fecha_inicio"] = fechaInicio,
            ["p_fecha_fin"] = fechaFin
        };
        
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedure<PagoDto>(
                "sp_Finanzas_Pagos_GetByFechas", 
                parametros));
    }
    
    public async Task<int> CreateAsync(CreatePagoDto pago)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_id_factura"] = pago.IdFactura ?? (object)DBNull.Value,
            ["p_numero_referencia"] = pago.NumeroReferencia,
            ["p_fecha_pago"] = pago.FechaPago,
            ["p_monto"] = pago.Monto,
            ["p_metodo_pago"] = pago.MetodoPago,
            ["p_numero_transaccion"] = pago.NumeroTransaccion ?? (object)DBNull.Value,
            ["p_concepto"] = pago.Concepto,
            ["p_notas_internas"] = pago.NotasInternas ?? (object)DBNull.Value,
            ["p_registrado_por"] = pago.RegistradoPor
        };
        
        var resultado = await Task.Run(() => 
            _dbHelper.ExecuteScalar(
                "sp_Finanzas_Pagos_Insert", 
                parametros));
        
        return Convert.ToInt32(resultado ?? 0);
    }
    
    public async Task<bool> UpdateAsync(PagoDto pago)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_id_pago"] = pago.IdPago,
            ["p_notas_internas"] = pago.NotasInternas ?? (object)DBNull.Value,
            ["p_estado_pago"] = pago.EstadoPago,
            ["p_actualizado_por"] = pago.ActualizadoPor ?? (object)DBNull.Value
        };
        
        var filasAfectadas = await Task.Run(() => 
            _dbHelper.ExecuteNonQuery(
                "sp_Finanzas_Pagos_Update", 
                parametros));
        
        return filasAfectadas > 0;
    }
    
    public async Task<bool> CancelarAsync(int idPago, string motivo, string canceladoPor)
    {
        var parametros = new Dictionary<string, object>
        {
            ["p_id_pago"] = idPago,
            ["p_motivo"] = motivo,
            ["p_cancelado_por"] = canceladoPor
        };
        
        var filasAfectadas = await Task.Run(() => 
            _dbHelper.ExecuteNonQuery(
                "sp_Finanzas_Pagos_Cancelar", 
                parametros));
        
        return filasAfectadas > 0;
    }
}
