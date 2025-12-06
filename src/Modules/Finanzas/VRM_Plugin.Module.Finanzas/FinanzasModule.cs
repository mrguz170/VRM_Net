using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Modules.Finanzas.Services;
using VRM_Plugin.Modules.Finanzas.Data.Repositories;

namespace VRM_Plugin.Modules.Finanzas;

/// <summary>
/// Módulo de gestión financiera.
/// Implementa IModule para integrarse en el sistema de plugins.
/// Los datos se inyectan desde el Host mediante SetComponents(), SetActions(), etc.
/// Arquitectura con IDs numéricos y permisos separados (navegación vs acciones).
/// </summary>
public class FinanzasModule : IModule
{
    // ==================== CAMPOS PRIVADOS ====================
    
    // Datos inyectados por el Host (desde BD)
    private List<ModuleComponentDto> _components = new();
    private List<ModuleActionDto> _actions = new();

    // Metadata del módulo (se inyectan desde BD, valores vacíos por defecto)
    private int _moduleId = 0;
    private string _moduleName = "Finanzas";
    private string _displayName = string.Empty;
    private string _description = string.Empty;
    private string _version = string.Empty;

    // ==================== PROPIEDADES PÚBLICAS ====================

    /// <summary>
    /// ID numérico del módulo
    /// </summary>
    public int ModuleId { get; set; }
    public string ModuleName => _moduleName;
    public string DisplayName => _displayName;
    public string Description => _description;
    public string Version => _version;
    
    // ==================== MÉTODOS PÚBLICOS ====================

    /// <summary>
    /// Devuelve los componentes inyectados por el Host
    /// </summary>
    public List<ModuleComponentDto> GetComponents()
    {
        if (_components.Count > 0)
        {
            return _components;
        }

        // Fallback: valores por defecto si no se cargaron desde BD
        return new List<ModuleComponentDto>();
    }
    
    /// <summary>
    /// Devuelve las acciones inyectadas por el Host
    /// </summary>
    public List<ModuleActionDto> GetActions()
    {
        if (_actions.Count > 0)
        {
            return _actions;
        }

        return new List<ModuleActionDto>();
    }
        
    // ==================== MÉTODOS DE INYECCIÓN (Llamados por el Host) ====================
    
    /// <summary>
    /// El Host llama este método para inyectar componentes desde BD
    /// </summary>
    public void SetComponents(List<ModuleComponentDto> components)
    {
        _components = components ?? new List<ModuleComponentDto>();
    }
    
    /// <summary>
    /// El Host llama este método para inyectar acciones desde BD
    /// </summary>
    public void SetActions(List<ModuleActionDto> actions)
    {
        _actions = actions ?? new List<ModuleActionDto>();
    }

    /// <summary>
    /// El Host llama este método para inyectar metadata desde BD
    /// </summary>    
    public void SetMetadata(int moduleId, string moduleName, string displayName, string description, string version)
    {
        _moduleId = moduleId != 0 ? moduleId : _moduleId;
        _moduleName = moduleName ?? _moduleName;
        _displayName = displayName ?? _displayName;
        _description = description ?? _description;
        _version = version ?? _version;
    }


    // ==================== CONFIGURACIÓN DE SERVICIOS ====================

    /// <summary>
    /// Registra servicios de NEGOCIO y repositorios del módulo
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        //  Registrar repositorios del módulo (capa de datos)
        services.AddScoped<IFacturaRepository, FacturaRepository>();
        services.AddScoped<IPagoRepository, PagoRepository>();
        
        //  Registrar servicios de negocio del módulo
        services.AddScoped<IFacturaService, FacturaService>();
        
        
        //  Registrar el módulo como IModule para inyección en layouts/componentes
        services.AddSingleton<IModule>(this);
        services.AddSingleton(this); 
    }
    
    
    public async Task OnModuleLoadedAsync()
    {
        Console.WriteLine($"[{ModuleName}] Módulo cargado - ModuleId: {ModuleId}");
        Console.WriteLine($"[{ModuleName}] DisplayName: {DisplayName}");
        Console.WriteLine($"[{ModuleName}] Componentes: {_components.Count}, Acciones: {_actions.Count}");
        await Task.CompletedTask;
    }
}
