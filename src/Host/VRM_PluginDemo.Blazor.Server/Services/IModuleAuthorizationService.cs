using VRM_Plugin.Core.Abstractions.Data.DTOs;

namespace VRM_Plugin.Blazor.Server.Services;

/// <summary>
/// Servicio para verificar permisos granulares de acciones dentro de módulos.
/// ? Actualizado para trabajar con IDs numéricos y jerarquía de componentes.
/// Proporciona autorización a nivel de acción y componente.
/// </summary>
public interface IModuleAuthorizationService
{
    // ==================== AUTORIZACIÓN POR ACCIONES ====================

    /// <summary>
    /// Verifica si el usuario actual tiene permiso para ejecutar una acción específica (por ActionKey).
    /// ?? LEGACY: Se mantiene para compatibilidad. Preferir CanExecuteActionByIdAsync().
    /// </summary>
    /// <param name="actionKey">
    /// Clave de la acción en formato "{ModuleName}.{Entidad}.{Accion}"
    /// Ejemplos: "Finanzas.Facturas.TimbrarSAT", "Prospectos.RevisionLegal"
    /// </param>
    /// <returns>true si el usuario tiene al menos uno de los permisos requeridos</returns>
    Task<bool> CanExecuteActionAsync(string actionKey);

    /// <summary>
    /// ? RECOMENDADO: Verifica si el usuario actual tiene permiso para ejecutar una acción específica (por ID).
    /// Más rápido y type-safe que la versión con ActionKey.
    /// </summary>
    /// <param name="idAction">ID numérico de la acción</param>
    /// <returns>true si el usuario tiene permiso</returns>
    Task<bool> CanExecuteActionByIdAsync(int idAction);

    /// <summary>
    /// Verifica si un usuario específico tiene permiso para una acción.
    /// Útil para validaciones en el backend sin contexto HTTP.
    /// </summary>
    /// <param name="userId">ID del usuario a verificar</param>
    /// <param name="actionKey">Clave de la acción</param>
    /// <returns>true si el usuario tiene permiso</returns>
    Task<bool> UserCanExecuteActionAsync(string userId, string actionKey);

    /// <summary>
    /// Obtiene todas las acciones disponibles para el usuario actual.
    /// Útil para generar menús dinámicos o interfaces adaptativas.
    /// </summary>
    /// <returns>Lista de claves de acciones permitidas</returns>
    Task<List<string>> GetAvailableActionsAsync();

    /// <summary>
    /// ? ACTUALIZADO: Obtiene todas las acciones disponibles para el usuario actual en un módulo específico.
    /// Usa ModuleId (int) para consistencia con arquitectura basada en IDs.
    /// </summary>
    /// <param name="idModule">ID numérico del módulo (ej: 1 = Finanzas, 2 = Prospectos)</param>
    /// <returns>Lista de claves de acciones permitidas en ese módulo</returns>
    Task<List<string>> GetAvailableActionsForModuleAsync(int idModule);

    /// <summary>
    /// ? ACTUALIZADO: Obtiene el diccionario completo de acciones y roles de un módulo.
    /// Útil para administración y configuración.
    /// ?? LEGACY: Mantiene compatibilidad con código anterior (devuelve strings en lugar de IDs).
    /// </summary>
    /// <param name="idModule">ID numérico del módulo</param>
    /// <returns>Diccionario de acción ? roles permitidos (como strings para compatibilidad)</returns>
    Task<Dictionary<string, string[]>> GetModuleActionsAsync(int idModule);

    // ==================== AUTORIZACIÓN POR COMPONENTES ====================

    /// <summary>
    /// ? NUEVO: Verifica si el usuario puede acceder a un componente específico.
    /// Considera herencia de permisos desde el componente padre.
    /// </summary>
    /// <param name="idComponent">ID del componente</param>
    /// <returns>true si el usuario tiene acceso</returns>
    Task<bool> CanAccessComponentAsync(int idComponent);

    /// <summary>
    /// ? NUEVO: Obtiene los componentes visibles para el usuario actual.
    /// Útil para construir el menú de navegación dinámicamente.
    /// Respeta jerarquía (IdParent) y herencia de permisos.
    /// </summary>
    /// <returns>Lista de componentes ordenados por MenuOrder</returns>
    Task<List<ModuleComponentDto>> GetVisibleComponentsAsync();
}
