using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using VRM_Plugin.Core.Abstractions.Common;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Core.Abstractions.Services;

namespace VRM_Plugin.Core.Abstractions.Data.Repositories;

/// <summary>
/// Repositorio de metadata de módulos (infraestructura del SISTEMA)
/// 
/// ? Ubicación: Data/Repositories/ (homologado con módulos)
/// ? Usa DatabaseHelper de Common/
/// </summary>
public class ModuleMetadataRepository : IModuleMetadataService
{
    private readonly DatabaseHelper _db;
    private readonly ILogger<ModuleMetadataRepository> _logger;
    
    public ModuleMetadataRepository(
        IConfiguration configuration,
        ILogger<ModuleMetadataRepository> logger)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found in configuration");
        
        _db = new DatabaseHelper(connectionString);
        _logger = logger;
    }
    
    /// <summary>
    /// Obtiene información básica del módulo desde BD
    /// Llama al SP: sp_get_module_info
    /// </summary>
    public ModuleDto GetModuleMetadata(int moduleId)
    {
        try
        {
            _logger.LogDebug("Obteniendo metadata del módulo {ModuleId} desde sp_get_module_info", moduleId);
            
            // ? Usar parseador genérico con ModuleDto
            var metadata = _db.ExecuteStoredProcedureSingle<ModuleDto>("sp_get_module_info", 
                new Dictionary<string, object>
                {
                    { "module_id", moduleId }
                });
            
            if (metadata == null)
            {
                _logger.LogWarning("No se encontró metadata para el módulo {ModuleId}", moduleId);
                return new ModuleDto();
            }
            
            _logger.LogInformation(
                "Metadata obtenida para módulo {ModuleId}: {DisplayName} v{Version}",
                moduleId,
                metadata.DisplayName,
                metadata.Version);
            
            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener metadata del módulo {ModuleId}", moduleId);
            return new ModuleDto();
        }
    }
    
    /// <summary>
    /// Obtiene los componentes del módulo desde BD
    /// Llama al SP: sp_get_component
    /// </summary>
    public List<ModuleComponentDto> GetComponentsByModuleId(int moduleId)
    {
        try
        {
            _logger.LogDebug("Obteniendo componentes del módulo {ModuleId} desde sp_get_component", moduleId);
            
            // ? Usar parseador genérico (simplifica ~40 líneas de código)
            var components = _db.ExecuteStoredProcedure<ModuleComponentDto>("sp_get_component", 
                new Dictionary<string, object>
                {
                    { "module_id", moduleId }
                });
            
            // ? Post-procesamiento: parsear roles de string a List<int>
            foreach (var component in components)
            {
                component.RequiredPermissionIds = ParseRolesString(component.RolesString);
            }
            
            _logger.LogInformation(
                "Obtenidos {Count} componentes para el módulo {ModuleId}",
                components.Count,
                moduleId);
            
            return components;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener componentes del módulo {ModuleId}", moduleId);
            return new List<ModuleComponentDto>();
        }
    }
    
    /// <summary>
    /// Obtiene las acciones del módulo desde BD
    /// Llama al SP: sp_get_actions
    /// </summary>
    public List<ModuleActionDto> GetActionsByModuleId(int moduleId)
    {
        try
        {
            _logger.LogDebug("Obteniendo acciones del módulo {ModuleId} desde sp_get_actions", moduleId);
            
            // ? Usar parseador genérico
            var actions = _db.ExecuteStoredProcedure<ModuleActionDto>("sp_get_actions", 
                new Dictionary<string, object>
                {
                    { "module_id", moduleId }
                });
            
            // ? Post-procesamiento: parsear roles de string a List<int>
            foreach (var action in actions)
            {
                action.RequiredPermissionIds = ParseRolesString(action.RolesString);
            }
            
            _logger.LogInformation(
                "Obtenidas {Count} acciones para el módulo {ModuleId}",
                actions.Count,
                moduleId);
            
            return actions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener acciones del módulo {ModuleId}", moduleId);
            return new List<ModuleActionDto>();
        }
    }
    
    /// <summary>
    /// Obtiene permisos por acción desde BD
    /// Llama al SP: ConsultaPermisos
    /// </summary>
    public Dictionary<string, string[]> GetActionPermissions(string moduleName)
    {
        try
        {
            _logger.LogDebug("Obteniendo permisos del módulo {ModuleName} desde ConsultaPermisos", moduleName);
            
            var result = _db.ExecuteStoredProcedure("ConsultaPermisos", new Dictionary<string, object>
            {
                { "Modulo", moduleName }
            });
            
            var permissions = new Dictionary<string, string[]>();
            
            foreach (DataRow row in result.Rows)
            {
                string key = row["PermisoId"]?.ToString() ?? string.Empty;
                string permisosStr = row["Roles"]?.ToString() ?? string.Empty;
                
                // Convertir el string separado por comas en arreglo
                string[] valores = permisosStr.Split(',', StringSplitOptions.RemoveEmptyEntries);
                
                if (!string.IsNullOrEmpty(key))
                {
                    permissions[key] = valores;
                }
            }
            
            _logger.LogInformation(
                "Obtenidos {Count} permisos para el módulo {ModuleName}",
                permissions.Count,
                moduleName);
            
            return permissions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener permisos del módulo {ModuleName}", moduleName);
            return new Dictionary<string, string[]>();
        }
    }
    
    // ==================== MÉTODOS HELPER PRIVADOS ====================
    
    /// <summary>
    /// Parsea un string de roles "1,2,3" a List&lt;int&gt;
    /// Reutilizable para componentes y acciones
    /// </summary>
    private List<int> ParseRolesString(string? rolesString)
    {
        if (string.IsNullOrWhiteSpace(rolesString))
            return new List<int>();
        
        try
        {
            return rolesString
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al parsear roles: {RolesString}", rolesString);
            return new List<int>();
        }
    }
}
