using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Core.Abstractions;
using System.Security.Claims;
using VRM_Plugin.Core.Abstractions.Services;

namespace VRM_Plugin.Blazor.Server.Services;

/// <summary>
/// Implementación del servicio de autorización usadas para verificar componentes y acciones de módulos.
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
    /// Comprueba si el usuario actual puede ejecutar una acción en un módulo
    /// </summary>
    public async Task<bool> CanExecuteActionAsync(IModule module, string actionKey)
    {
        try
        {
            if (module == null || string.IsNullOrWhiteSpace(actionKey))
                return false;

            var (isAuthenticated, userPermissionIds, userName) = await GetUserPermissionsAsync();
            
            if (!isAuthenticated || !userPermissionIds.Any())
            {
                _logger.LogDebug("Usuario {User} no autenticado o sin permisos para {ActionKey}", userName, actionKey);
                return false;
            }

            var actions = module.GetActions();
            var action = actions.FirstOrDefault(a => a.ActionKey.Equals(actionKey, StringComparison.OrdinalIgnoreCase) && a.IsActive);
            
            if (action == null)
            {
                _logger.LogDebug("Acción {ActionKey} no encontrada en {Module}", actionKey, module.ModuleName);
                return false;
            }

            var hasPermission = action.RequiredPermissionIds.Any(req => userPermissionIds.Contains(req));
            
            if (!hasPermission)
            {
                _logger.LogWarning(
                    "Usuario {User} sin permisos para {ActionKey}. UserPerms=[{UserPerms}] Required=[{Req}]",
                    userName, actionKey,
                    string.Join(", ", userPermissionIds),
                    string.Join(", ", action.RequiredPermissionIds));
            }

            return hasPermission;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando permiso {ActionKey} en {Module}", actionKey, module?.ModuleName);
            return false;
        }
    }

    /// <summary>
    /// Verifica si el usuario puede acceder a un componente específico
    /// </summary>
    public async Task<bool> CanAccessComponentAsync(int idComponent)
    {
        try
        {
            var (isAuthenticated, userPermissionIds, userName) = await GetUserPermissionsAsync();
            
            if (!isAuthenticated)
            {
                _logger.LogWarning("Usuario no autenticado intentó acceder a componente {ComponentId}", idComponent);
                return false;
            }

            if (!userPermissionIds.Any())
            {
                _logger.LogWarning("Usuario {User} no tiene permisos asignados", userName);
                return false;
            }

            // Buscar componente en todos los módulos
            foreach (var module in _moduleManager.GetAllModules())
            {
                var allComponents = module.GetComponents();
                var component = allComponents.FirstOrDefault(c => c.ComponentId == idComponent && c.IsActive);

                if (component != null)
                {
                    return CheckComponentPermission(component, userPermissionIds, allComponents);
                }
            }

            _logger.LogWarning("Componente {ComponentId} no encontrado", idComponent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando acceso a componente {ComponentId}", idComponent);
            return false;
        }
    }

    /// <summary>
    /// Obtiene componentes visibles para el usuario actual según sus permisos
    /// </summary>
    public async Task<List<ModuleComponentDto>> GetVisibleComponentsAsync()
    {
        try
        {
            var (isAuthenticated, userPermissionIds, userName) = await GetUserPermissionsAsync();
            
            if (!isAuthenticated)
            {
                _logger.LogDebug("Usuario no autenticado - lista vacía");
                return new List<ModuleComponentDto>();
            }

            if (!userPermissionIds.Any())
            {
                _logger.LogWarning("Usuario {User} sin roles asignados", userName);
                return new List<ModuleComponentDto>();
            }

            var visibleComponents = new List<ModuleComponentDto>();

            foreach (var module in _moduleManager.GetAllModules())
            {
                var allComponents = module.GetComponents();
                var menuComponents = allComponents.Where(c => c.IsActive && c.ShowInMenu);

                foreach (var component in menuComponents)
                {
                    if (CheckComponentPermission(component, userPermissionIds, allComponents))
                    {
                        visibleComponents.Add(component);
                    }
                }
            }

            _logger.LogDebug(
                "Usuario {User} con roles [{Roles}] ? {Count} componentes",
                userName,
                string.Join(", ", userPermissionIds),
                visibleComponents.Count);

            return visibleComponents.OrderBy(c => c.MenuOrder).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo componentes visibles");
            return new List<ModuleComponentDto>();
        }
    }

    // ==================== MÉTODOS PRIVADOS ====================

    /// <summary>
    /// Obtiene usuario autenticado y sus permisos (centralizado)
    /// </summary>
    private async Task<(bool isAuthenticated, List<int> permissionIds, string userName)> GetUserPermissionsAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            return (false, new List<int>(), "Anonymous");
        }

        var userName = user.Identity.Name ?? "Unknown";
        var permissionIds = GetUserPermissionIds(user);

        return (true, permissionIds, userName);
    }

    /// <summary>
    /// Extrae los IDs de roles del usuario desde claims
    /// </summary>
    private List<int> GetUserPermissionIds(ClaimsPrincipal user)
    {
        var permissionClaims = user.Claims
            .Where(c => c.Type == "RoleId")
            .Select(c => c.Value)
            .ToList();

        return permissionClaims
            .Select(p => int.TryParse(p, out var id) ? id : 0)
            .Where(id => id > 0)
            .ToList();
    }

    /// <summary>
    /// Verifica permisos de componente sin herencia del padre
    /// </summary>
    private bool CheckComponentPermission(ModuleComponentDto component, List<int> userPermissionIds, List<ModuleComponentDto> allComponents)
    {
        // Componente público (sin restricciones)
        if (component.RequiredPermissionIds == null)
        {
            return true;
        }

        // Sin roles asignados ? denegado
        if (!component.RequiredPermissionIds.Any())
        {
            return false;
        }

        // ? Usuario tiene al menos uno de los roles requeridos
        if (component.RequiredPermissionIds.Any(reqPermId => userPermissionIds.Contains(reqPermId)))
        {
            return true;
        }

        // ? NO heredar del padre - cada componente es independiente
        // Si el componente tiene permisos explícitos pero el usuario no los cumple ? DENEGAR
        return false;
    }
}