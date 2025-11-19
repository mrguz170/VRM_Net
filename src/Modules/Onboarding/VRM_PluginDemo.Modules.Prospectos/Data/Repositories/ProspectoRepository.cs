using VRM_Plugin.Core.Abstractions.Common;
using VRM_Plugin.Modules.Prospectos.Data.DTOs;

namespace VRM_Plugin.Modules.Prospectos.Data.Repositories;

/// <summary>
/// Implementación del repositorio de Prospectos
/// 
/// ? EJEMPLO SIMPLIFICADO: Demuestra cómo conectarse a BD
/// 
/// ?? FLUJO DE CONEXIÓN A BASE DE DATOS:
/// ????????????????????????????????????????????????????????????????
/// 
/// 1?? INYECCIÓN DE DatabaseHelper (viene del Host)
///    - DatabaseHelper ya tiene el connection string de appsettings.json
///    - Se inyecta automáticamente por DI (registrado en Program.cs)
/// 
/// 2?? EJECUCIÓN DE STORED PROCEDURES
///    - _dbHelper.ExecuteStoredProcedure<T>() ? Retorna lista
///    - _dbHelper.ExecuteStoredProcedureSingle<T>() ? Retorna un objeto
///    - _dbHelper.ExecuteScalar() ? Retorna valor único (ID, count, etc.)
/// 
/// 3?? MAPEO AUTOMÁTICO
///    - DatabaseHelper convierte automáticamente:
///      • snake_case (BD) ? PascalCase (C#)
///      • id_prospecto ? IdProspecto
///      • razon_social ? RazonSocial
/// 
/// 4?? MANEJO DE PARÁMETROS
///    - Se pasan en Dictionary<string, object>
///    - Valores null se convierten a DBNull.Value
/// 
/// ????????????????????????????????????????????????????????????????
/// </summary>
public class ProspectoRepository : IProspectoRepository
{
    // ?? 1. DatabaseHelper se inyecta automáticamente por DI
    private readonly DatabaseHelper _dbHelper;
    
    public ProspectoRepository(DatabaseHelper dbHelper)
    {
        _dbHelper = dbHelper;
    }
    
    /// <summary>
    /// ?? EJEMPLO 1: Ejecutar SP sin parámetros, retorna lista
    /// 
    /// Flujo:
    /// 1. _dbHelper ejecuta "sp_Prospectos_GetAll"
    /// 2. Obtiene DataTable de MySQL
    /// 3. Convierte automáticamente cada fila a ProspectoDto
    /// 4. Retorna List<ProspectoDto>
    /// </summary>
    public async Task<List<ProspectoDto>> GetAllAsync()
    {
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedure<ProspectoDto>(
                "sp_Prospectos_GetAll"));
    }
    
    /// <summary>
    /// ?? EJEMPLO 2: Ejecutar SP con parámetros, retorna objeto único
    /// 
    /// Flujo:
    /// 1. Crear Dictionary con parámetros del SP
    /// 2. _dbHelper ejecuta "sp_Prospectos_GetById"
    /// 3. Pasa parámetros al SP
    /// 4. Obtiene primera fila y convierte a ProspectoDto
    /// 5. Retorna ProspectoDto o null si no existe
    /// </summary>
    public async Task<ProspectoDto?> GetByIdAsync(int idProspecto)
    {
        // ?? Crear parámetros para el SP
        var parametros = new Dictionary<string, object>
        {
            ["p_id_prospecto"] = idProspecto
        };
        
        return await Task.Run(() => 
            _dbHelper.ExecuteStoredProcedureSingle<ProspectoDto>(
                "sp_Prospectos_GetById", 
                parametros));
    }
    
    /// <summary>
    /// ?? EJEMPLO 3: Ejecutar SP de INSERT, retorna ID generado
    /// 
    /// Flujo:
    /// 1. Crear Dictionary con datos a insertar
    /// 2. Convertir valores null a DBNull.Value
    /// 3. _dbHelper ejecuta "sp_Prospectos_Insert"
    /// 4. El SP retorna el ID generado (LAST_INSERT_ID)
    /// 5. ExecuteScalar obtiene ese ID
    /// 6. Se convierte a int y retorna
    /// </summary>
    public async Task<int> CreateAsync(CreateProspectoDto prospecto)
    {
        // ?? Preparar parámetros del SP
        var parametros = new Dictionary<string, object>
        {
            ["p_razon_social"] = prospecto.RazonSocial,
            ["p_rfc"] = prospecto.RFC,
            ["p_correo_electronico"] = prospecto.CorreoElectronico,
            ["p_creado_por"] = prospecto.CreadoPor
        };
        
        // ?? ExecuteScalar retorna el ID generado por el SP
        var resultado = await Task.Run(() => 
            _dbHelper.ExecuteScalar(
                "sp_Prospectos_Insert", 
                parametros));
        
        return Convert.ToInt32(resultado ?? 0);
    }
}
