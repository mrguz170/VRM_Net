using VRM_Plugin.Modules.Prospectos.Data.DTOs;

namespace VRM_Plugin.Modules.Prospectos.Data.Repositories;

/// <summary>
/// Repositorio para acceso a datos de Prospectos
/// ? EJEMPLO SIMPLIFICADO: Solo 3 métodos para demostrar conexión a BD
/// 
/// ?? PATRÓN DE CONEXIÓN:
/// 1. Usa DatabaseHelper (inyectado desde el Host)
/// 2. Ejecuta Stored Procedures
/// 3. Mapeo automático de resultados a DTOs
/// </summary>
public interface IProspectoRepository
{
    /// <summary>
    /// Obtiene todos los prospectos activos
    /// SP: sp_Prospectos_GetAll
    /// 
    /// Ejemplo de llamada:
    /// var prospectos = await _repository.GetAllAsync();
    /// </summary>
    Task<List<ProspectoDto>> GetAllAsync();
    
    /// <summary>
    /// Obtiene un prospecto por ID
    /// SP: sp_Prospectos_GetById
    /// 
    /// Ejemplo de llamada:
    /// var prospecto = await _repository.GetByIdAsync(1);
    /// </summary>
    Task<ProspectoDto?> GetByIdAsync(int idProspecto);
    
    /// <summary>
    /// Crea un nuevo prospecto
    /// SP: sp_Prospectos_Insert
    /// Retorna el ID generado
    /// 
    /// Ejemplo de llamada:
    /// var dto = new CreateProspectoDto { RazonSocial = "Empresa SA", ... };
    /// var id = await _repository.CreateAsync(dto);
    /// </summary>
    Task<int> CreateAsync(CreateProspectoDto prospecto);
}
