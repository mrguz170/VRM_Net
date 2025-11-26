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
    /// Comprueba si el usuario actual puede ejecutar <paramref name="actionKey"/> en <paramref name="module"/>.
    /// </summary>
    public async Task<bool> CanExecuteActionAsync(IModule module, string actionKey)
    {
        try
        {
            if (module == null || string.IsNullOrWhiteSpace(actionKey))
                return false;

            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogDebug("Usuario no autenticado para acción {ActionKey}", actionKey);
                return false;
            }

            var userPermissionIds = GetUserPermissionIds(user);
            if (!userPermissionIds.Any())
            {
                _logger.LogDebug("Usuario {UserName} no tiene permisos para acción {ActionKey}", user.Identity?.Name, actionKey);
                return false;
            }

            var actions = module.GetActions();
            var action = actions.FirstOrDefault(a => a.ActionKey.Equals(actionKey, StringComparison.OrdinalIgnoreCase) && a.IsActive);
            if (action == null)
            {
                _logger.LogDebug("Acción {ActionKey} no encontrada en módulo {ModuleName}", actionKey, module.ModuleName);
                return false;
            }

            var has = action.RequiredPermissionIds.Any(req => userPermissionIds.Contains(req));
            if (!has)
            {
                _logger.LogWarning("Usuario {User} sin permisos para {ActionKey}. UserPerms=[{UserPerms}] Required=[{Req}]",
                    user.Identity?.Name,
                    actionKey,
                    string.Join(", ", userPermissionIds),
                    string.Join(", ", action.RequiredPermissionIds));
            }

            return has;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando permiso para {ActionKey} en módulo {Module}", actionKey, module?.ModuleName);
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
            .Where(c => c.Type == "RoleId")
            .Select(c => c.Value)
            .ToList();

            return permissionClaims
                .Select(p => int.TryParse(p, out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
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
