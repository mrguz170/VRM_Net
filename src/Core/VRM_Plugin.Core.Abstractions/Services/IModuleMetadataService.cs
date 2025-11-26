using VRM_Plugin.Core.Abstractions.Data.DTOs;

namespace VRM_Plugin.Core.Abstractions.Services;

/// <summary>
/// Servicio para obtener metadata de módulos desde BD
/// </summary>
public interface IModuleMetadataService
{
    /// <summary>
    /// Obtiene la información completa de un módulo
    /// </summary>
    ModuleDto GetModuleMetadata(int moduleId);
    
    /// <summary>
    /// Obtiene los componentes de un módulo
    /// </summary>
    List<ModuleComponentDto> GetComponentsByModuleId(int moduleId);
    
    /// <summary>
    /// Obtiene las acciones de un módulo
    /// </summary>
    List<ModuleActionDto> GetActionsByModuleId(int moduleId);
    
    }
