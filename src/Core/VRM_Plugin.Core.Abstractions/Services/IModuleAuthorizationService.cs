using VRM_Plugin.Core.Abstractions.Data.DTOs;

namespace VRM_Plugin.Core.Abstractions.Services;

/// <summary>
/// Servicio para verificar permisos granulares de acciones dentro de módulos.
/// ? Actualizado para trabajar con IDs numéricos y jerarquía de componentes.
/// Proporciona autorización a nivel de acción y componente.
/// </summary>
public interface IModuleAuthorizationService
{
    // ==================== AUTORIZACIÓN POR ACCIONES ====================

    /// <summary>
    /// Valida si el usuario actual puede ejecutar una acción específica dentro de un módulo.   
    /// </summary>
    Task<bool> CanExecuteActionAsync(IModule module, string actionKey);

    
    // ==================== AUTORIZACIÓN POR COMPONENTES ====================

    /// <summary>
    /// Verifica si el usuario puede acceder a un componente específico.
    /// Considera herencia de permisos desde el componente padre.
    /// </summary>
    /// <param name="idComponent">ID del componente</param>
    /// <returns>true si el usuario tiene acceso</returns>
    Task<bool> CanAccessComponentAsync(int idComponent);

    /// <summary>
    /// Obtiene los componentes visibles para el usuario actual.
    /// Útil para construir el menú de navegación dinámicamente.
    /// Respeta jerarquía (IdParent) y herencia de permisos.
    /// </summary>
    /// <returns>Lista de componentes ordenados por MenuOrder</returns>
    Task<List<ModuleComponentDto>> GetVisibleComponentsAsync();
}
