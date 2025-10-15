namespace VRM_PluginDemo.Blazor.Server.Services;

/// <summary>
/// Servicio para verificar permisos granulares de acciones dentro de módulos.
/// Proporciona autorización a nivel de acción, no solo a nivel de módulo.
/// </summary>
public interface IModuleAuthorizationService
{
    /// <summary>
    /// Verifica si el usuario actual tiene permiso para ejecutar una acción específica.
    /// </summary>
    /// <param name="actionKey">
    /// Clave de la acción en formato "{ModuleId}.{Entidad}.{Acción}"
    /// Ejemplos: "Finanzas.Facturas.TimbrarSAT", "Prospectos.RevisionLegal"
    /// </param>
    /// <returns>true si el usuario tiene al menos uno de los roles requeridos</returns>
    Task<bool> CanExecuteActionAsync(string actionKey);
    
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
    /// Obtiene todas las acciones disponibles para el usuario actual en un módulo específico.
    /// </summary>
    /// <param name="moduleId">ID del módulo (ej: "Finanzas", "Prospectos")</param>
    /// <returns>Lista de claves de acciones permitidas en ese módulo</returns>
    Task<List<string>> GetAvailableActionsForModuleAsync(string moduleId);
    
    /// <summary>
    /// Obtiene el diccionario completo de acciones y roles de un módulo.
    /// Útil para administración y configuración.
    /// </summary>
    /// <param name="moduleId">ID del módulo</param>
    /// <returns>Diccionario de acción ? roles permitidos</returns>
    Task<Dictionary<string, string[]>> GetModuleActionsAsync(string moduleId);
}
