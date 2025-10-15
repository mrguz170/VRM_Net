using System.Reflection;
using VRM_PluginDemo.Core.Abstractions;

namespace VRM_PluginDemo.Blazor.Server.Services;

/// <summary>
/// Interfaz para gestionar módulos cargados en el sistema
/// </summary>
public interface IModuleManager
{
    /// <summary>
    /// Obtiene todos los módulos cargados
    /// </summary>
    IReadOnlyList<IModule> GetAllModules();

    /// <summary>
    /// Obtiene un módulo por su ID
    /// </summary>
    IModule? GetModuleById(string moduleId);

    /// <summary>
    /// Obtiene módulos habilitados para un cliente específico
    /// </summary>
    IEnumerable<IModule> GetModulesForClient(string clienteId);

    /// <summary>
    /// Obtiene módulos por categoría
    /// </summary>
    IEnumerable<IModule> GetModulesByCategory(string category);

    /// <summary>
    /// Verifica si un módulo está cargado
    /// </summary>
    bool IsModuleLoaded(string moduleId);

    /// <summary>
    /// Obtiene un módulo específico por su ID
    /// </summary>
    /// <param name="moduleId">ID del módulo a obtener</param>
    /// <returns>El módulo si existe, null si no</returns>
    IModule? GetModule(string moduleId);
}