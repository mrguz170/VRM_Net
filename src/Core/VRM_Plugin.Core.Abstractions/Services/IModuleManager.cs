using System.Reflection;
using VRM_Plugin.Core.Abstractions;

namespace VRM_Plugin.Core.Abstractions.Services;

/// <summary>
/// Interfaz para gestionar módulos cargados en el sistema
/// </summary>
public interface IModuleManager
{
    /// <summary>
    /// Obtiene todos los módulos cargados
    /// </summary>
    IReadOnlyList<IModule> GetAllModules();
        
}