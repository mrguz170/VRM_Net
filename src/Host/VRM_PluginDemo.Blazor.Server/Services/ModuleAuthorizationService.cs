using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace VRM_PluginDemo.Blazor.Server.Services;

/// <summary>
/// Implementación del servicio de autorización granular de módulos.
/// Verifica permisos a nivel de acción usando los roles del usuario autenticado.
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

    public async Task<bool> CanExecuteActionAsync(string actionKey)
    {
        try
        {
            // Obtener el usuario autenticado
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            // Verificar si está autenticado
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                _logger.LogWarning("Usuario no autenticado intentó acceder a {ActionKey}", actionKey);
                return false;
            }

            // Extraer roles del usuario
            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            if (!userRoles.Any())
            {
                _logger.LogWarning("Usuario {UserName} no tiene roles asignados", user.Identity.Name);
                return false;
            }

            // Verificar permiso
            return await CheckActionPermissionAsync(actionKey, userRoles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar permiso para {ActionKey}", actionKey);
            return false;
        }
    }

    public async Task<bool> UserCanExecuteActionAsync(string userId, string actionKey)
    {
        // TODO: Implementar cuando tengas IUsuarioService
        // Por ahora, delega al usuario actual
        _logger.LogWarning("UserCanExecuteActionAsync no implementado completamente. Delegando a CanExecuteActionAsync");
        return await CanExecuteActionAsync(actionKey);
    }

    public async Task<List<string>> GetAvailableActionsAsync()
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return new List<string>();

            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var availableActions = new List<string>();
            var allModules = _moduleManager.GetAllModules();

            foreach (var module in allModules)
            {
                var moduleActions = module.GetActionPermissions();

                foreach (var action in moduleActions)
                {
                    // Si el usuario tiene al menos uno de los roles requeridos
                    if (action.Value.Any(requiredRole => userRoles.Contains(requiredRole)))
                    {
                        availableActions.Add(action.Key);
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

    public async Task<List<string>> GetAvailableActionsForModuleAsync(string moduleId)
    {
        try
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return new List<string>();

            var userRoles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            var module = _moduleManager.GetModule(moduleId);
            if (module == null)
            {
                _logger.LogWarning("Módulo {ModuleId} no encontrado", moduleId);
                return new List<string>();
            }

            var moduleActions = module.GetActionPermissions();
            var availableActions = new List<string>();

            foreach (var action in moduleActions)
            {
                if (action.Value.Any(requiredRole => userRoles.Contains(requiredRole)))
                {
                    availableActions.Add(action.Key);
                }
            }

            return await Task.FromResult(availableActions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener acciones del módulo {ModuleId}", moduleId);
            return new List<string>();
        }
    }

    public async Task<Dictionary<string, string[]>> GetModuleActionsAsync(string moduleId)
    {
        try
        {
            var module = _moduleManager.GetModule(moduleId);

            if (module == null)
            {
                _logger.LogWarning("Módulo {ModuleId} no encontrado", moduleId);
                return new Dictionary<string, string[]>();
            }

            return await Task.FromResult(module.GetActionPermissions());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener acciones del módulo {ModuleId}", moduleId);
            return new Dictionary<string, string[]>();
        }
    }

    /// <summary>
    /// Verifica si el usuario tiene permiso para ejecutar una acción específica.
    /// </summary>
    private async Task<bool> CheckActionPermissionAsync(string actionKey, List<string> userRoles)
    {
        // Extraer ModuleId de la actionKey (ej: "Finanzas.Facturas.TimbrarSAT" -> "Finanzas")
        var parts = actionKey.Split('.');
        if (parts.Length < 2)
        {
            _logger.LogWarning("Formato de actionKey inválido: {ActionKey}. Formato esperado: ModuleId.Entidad.Accion", actionKey);
            return false;
        }

        var moduleId = parts[0];
        var module = _moduleManager.GetModule(moduleId);

        if (module == null)
        {
            _logger.LogWarning("Módulo {ModuleId} no encontrado para acción {ActionKey}", moduleId, actionKey);
            return false;
        }

        var actionPermissions = module.GetActionPermissions();

        if (!actionPermissions.ContainsKey(actionKey))
        {
            _logger.LogWarning("Acción {ActionKey} no definida en módulo {ModuleId}", actionKey, moduleId);
            return false;
        }

        var requiredRoles = actionPermissions[actionKey];
        var hasPermission = requiredRoles.Any(requiredRole => userRoles.Contains(requiredRole));

        if (!hasPermission)
        {
            _logger.LogWarning(
                "Usuario sin permisos para {ActionKey}. Roles del usuario: [{UserRoles}], Roles requeridos: [{RequiredRoles}]",
                actionKey,
                string.Join(", ", userRoles),
                string.Join(", ", requiredRoles));
        }
        else
        {
            _logger.LogDebug(
                "Usuario autorizado para {ActionKey}. Rol coincidente encontrado en: [{UserRoles}]",
                actionKey,
                string.Join(", ", userRoles));
        }

        return await Task.FromResult(hasPermission);
    }
}
