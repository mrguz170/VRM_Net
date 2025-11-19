using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Core.Abstractions;
using System.Security.Claims;

namespace VRM_Plugin.Blazor.Server.Services;

/// <summary>
/// Implementación del servicio de autorización granular de módulos.
/// ? Actualizado para usar nueva arquitectura con IDs numéricos.
/// Verifica permisos a nivel de acción usando los IDs de permisos del usuario autenticado.
/// </summary>
public class ModuleAuthorizationService : IModuleAuthorizationService
{
    private readonly IModuleManager _moduleManager;
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly ILogger<ModuleAuthorizationService> _logger;

    public ModuleAuthorizationService(
        IModuleManager moduleManager,
        AuthenticationStateProvider authStateProvider,
        ILogger<ModuleAuthorizationService> logger)
    {
        _moduleManager = moduleManager;
        _authStateProvider = authStateProvider;
        _logger = logger;
    }

    /// <summary>
    /// Verifica si el usuario actual puede ejecutar una acción específica (por ActionKey legacy)
    /// </summary>
    public async Task<bool> CanExecuteActionAsync(string actionKey)
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Usuario no autenticado intentó acceder a {ActionKey}", actionKey);
                return false;
            }

            // Obtener IDs de permisos del usuario desde claims
            var userPermissionIds = GetUserPermissionIds(user);

            if (!userPermissionIds.Any())
            {
                _logger.LogWarning("Usuario {UserName} no tiene permisos asignados", user.Identity.Name);
                return false;
            }

            return await CheckActionPermissionByKeyAsync(actionKey, userPermissionIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar permiso para {ActionKey}", actionKey);
            return false;
        }
    }

    /// <summary>
    /// Verifica si el usuario actual puede ejecutar una acción específica (por ID)
    /// </summary>
    public async Task<bool> CanExecuteActionByIdAsync(int idAction)
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Usuario no autenticado intentó acceder a IdAction={IdAction}", idAction);
                return false;
            }

            var userPermissionIds = GetUserPermissionIds(user);

            if (!userPermissionIds.Any())
            {
                _logger.LogWarning("Usuario {UserName} no tiene permisos asignados", user.Identity.Name);
                return false;
            }

            return await CheckActionPermissionByIdAsync(idAction, userPermissionIds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar permiso para IdAction={IdAction}", idAction);
            return false;
        }
    }

    /// <summary>
    /// Verifica si un usuario específico puede ejecutar una acción
    /// </summary>
    public async Task<bool> UserCanExecuteActionAsync(string userId, string actionKey)
    {
        // TODO: Implementar cuando tengas IUsuarioService para obtener permisos por userId
        _logger.LogWarning("UserCanExecuteActionAsync no implementado completamente. Delegando a CanExecuteActionAsync");
        return await CanExecuteActionAsync(actionKey);
    }

    /// <summary>
    /// Implementación lectura de permisos de actions por componente IFM
    /// </summary>
    public async Task<List<string>> GetAvailableActionsAsync()  
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return new List<string>();

            var userPermissionIds = GetUserPermissionIds(user);
            var availableActions = new List<string>();
            var allModules = _moduleManager.GetAllModules();

            foreach (var module in allModules)
            {
                var actions = module.GetActions();

                foreach (var action in actions.Where(a => a.IsActive))
                {
                    // Si el usuario tiene al menos uno de los permisos requeridos
                    if (action.RequiredPermissionIds.Any(reqPermId => userPermissionIds.Contains(reqPermId)))
                    {
                        availableActions.Add(action.ActionKey);
                    }
                }
            }

            return availableActions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener acciones disponibles");
            return new List<string>();
        }
    }

    /// <summary>
    /// ? ACTUALIZADO: Obtiene las acciones disponibles para un módulo específico (por ID)
    /// </summary>
    public async Task<List<string>> GetAvailableActionsForModuleAsync(int idModule)
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return new List<string>();

            var userPermissionIds = GetUserPermissionIds(user);
            
            // Buscar módulo por IdModule
            var module = _moduleManager.GetAllModules()
                .FirstOrDefault(m => m.ModuleId == idModule);

            if (module == null)
            {
                _logger.LogWarning("Módulo con IdModule={IdModule} no encontrado", idModule);
                return new List<string>();
            }

            var actions = module.GetActions();
            var availableActions = new List<string>();

            foreach (var action in actions.Where(a => a.IsActive))
            {
                if (action.RequiredPermissionIds.Any(reqPermId => userPermissionIds.Contains(reqPermId)))
                {
                    availableActions.Add(action.ActionKey);
                }
            }

            return await Task.FromResult(availableActions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener acciones del módulo IdModule={IdModule}", idModule);
            return new List<string>();
        }
    }

    /// <summary>
    /// ? ACTUALIZADO: Obtiene todas las acciones de un módulo (sin filtrar por permisos) por ID
    /// </summary>
    public async Task<Dictionary<string, string[]>> GetModuleActionsAsync(int idModule)
    {
        try
        {
            // Buscar módulo por IdModule
            var module = _moduleManager.GetAllModules()
                .FirstOrDefault(m => m.ModuleId == idModule);

            if (module == null)
            {
                _logger.LogWarning("Módulo con IdModule={IdModule} no encontrado", idModule);
                return new Dictionary<string, string[]>();
            }

            // Convertir de nueva estructura (IDs) a legacy (nombres) para compatibilidad
            var actions = module.GetActions();
            var legacyFormat = new Dictionary<string, string[]>();

            foreach (var action in actions.Where(a => a.IsActive))
            {
                // Por ahora, devolver IDs como strings para compatibilidad
                legacyFormat[action.ActionKey] = action.RequiredPermissionIds
                    .Select(id => id.ToString())
                    .ToArray();
            }

            return await Task.FromResult(legacyFormat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener acciones del módulo IdModule={IdModule}", idModule);
            return new Dictionary<string, string[]>();
        }
    }

    /// <summary>
    /// Verifica si el usuario puede acceder a un componente específico
    /// </summary>
    public async Task<bool> CanAccessComponentAsync(int idComponent)
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Usuario no autenticado intentó acceder a IdComponent={IdComponent}", idComponent);
                return false;
            }

            var userPermissionIds = GetUserPermissionIds(user);

            if (!userPermissionIds.Any())
            {
                _logger.LogWarning("Usuario {UserName} no tiene permisos asignados", user.Identity.Name);
                return false;
            }

            // Buscar el componente en todos los módulos
            foreach (var module in _moduleManager.GetAllModules())
            {
                var component = module.GetComponents()
                    .FirstOrDefault(c => c.ComponentId == idComponent && c.IsActive);

                if (component != null)
                {
                    return CheckComponentPermission(component, userPermissionIds, module.GetComponents());
                }
            }

            _logger.LogWarning("Componente {IdComponent} no encontrado", idComponent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar acceso a componente {IdComponent}", idComponent);
            return false;
        }
    }

    /// <summary>
    /// Implementación de permisos para componentes IFM
    /// </summary>
    public async Task<List<ModuleComponentDto>> GetVisibleComponentsAsync()
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return new List<ModuleComponentDto>();

            var userPermissionIds = GetUserPermissionIds(user);
            var visibleComponents = new List<ModuleComponentDto>();

            foreach (var module in _moduleManager.GetAllModules())
            {
                var components = module.GetComponents();

                foreach (var component in components.Where(c => c.IsActive && c.ShowInMenu))
                {
                    if (CheckComponentPermission(component, userPermissionIds, components))
                    {
                        visibleComponents.Add(component);
                    }
                }
            }

            return visibleComponents.OrderBy(c => c.MenuOrder).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener componentes visibles");
            return new List<ModuleComponentDto>();
        }
    }

    // ==================== MÉTODOS PRIVADOS ====================

    /// <summary>
    /// Extrae los IDs de permisos del usuario desde sus claims
    /// </summary>
    private List<int> GetUserPermissionIds(ClaimsPrincipal user)
    {
        // Opción 1: Si los permisos están en claims como "Permission"
        var permissionClaims = user.Claims
            .Where(c => c.Type == "Permission")
            .Select(c => c.Value)
            .ToList();

            return permissionClaims
                .Select(p => int.TryParse(p, out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
    }



    /// <summary>
    /// Verifica si el usuario tiene permiso para acceder a una acción (por ActionKey)
    /// </summary>
    private async Task<bool> CheckActionPermissionByKeyAsync(string actionKey, List<int> userPermissionIds)
    {
        // Buscar la acción en todos los módulos
        foreach (var module in _moduleManager.GetAllModules())
        {
            var action = module.GetActions()
                .FirstOrDefault(a => a.ActionKey.Equals(actionKey, StringComparison.OrdinalIgnoreCase) && a.IsActive);

            if (action != null)
            {
                var hasPermission = action.RequiredPermissionIds.Any(reqPermId => userPermissionIds.Contains(reqPermId));

                if (!hasPermission)
                {
                    _logger.LogWarning(
                        "Usuario sin permisos para {ActionKey}. Permisos del usuario: [{UserPerms}], Permisos requeridos: [{RequiredPerms}]",
                        actionKey,
                        string.Join(", ", userPermissionIds),
                        string.Join(", ", action.RequiredPermissionIds));
                }

                return await Task.FromResult(hasPermission);
            }
        }

        _logger.LogWarning("Acción {ActionKey} no encontrada en ningún módulo", actionKey);
        return false;
    }

    /// <summary>
    /// Verifica si el usuario tiene permiso para acceder a una acción (por ID)
    /// </summary>
    private async Task<bool> CheckActionPermissionByIdAsync(int idAction, List<int> userPermissionIds)
    {
        foreach (var module in _moduleManager.GetAllModules())
        {
            var action = module.GetActions()
                .FirstOrDefault(a => a.ActionKeyId == idAction && a.IsActive);

            if (action != null)
            {
                var hasPermission = action.RequiredPermissionIds.Any(reqPermId => userPermissionIds.Contains(reqPermId));
                return await Task.FromResult(hasPermission);
            }
        }

        return false;
    }

    /// <summary>
    /// Verifica permisos de componente con herencia desde el padre
    /// </summary>
    private bool CheckComponentPermission(ModuleComponentDto component, List<int> userPermissionIds, List<ModuleComponentDto> allComponents)
    {
        // Si el componente tiene permisos definidos, usarlos
        if (component.RequiredPermissionIds.Any())
        {
            return component.RequiredPermissionIds.Any(reqPermId => userPermissionIds.Contains(reqPermId));
        }

        // Si no tiene permisos y tiene padre, heredar del padre
        if (component.ParentId.HasValue)
        {
            var parent = allComponents.FirstOrDefault(c => c.ComponentId == component.ParentId.Value);
            if (parent != null)
            {
                return CheckComponentPermission(parent, userPermissionIds, allComponents);
            }
        }

        // Si no tiene permisos ni padre, es público
        return true;
    }
}
