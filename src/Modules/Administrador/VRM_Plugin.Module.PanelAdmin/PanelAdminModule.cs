using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Core.Abstractions.Data.Repositories;
using VRM_Plugin.Core.Abstractions.Services;
using VRM_Plugin.Module.PanelAdmin.Data.Repositories;
using VRM_Plugin.Module.PanelAdmin.Services;

namespace VRM_Plugin.Module.PanelAdmin;

public class PanelAdminModule : IModule
{
    // ==================== PROPIEDADES PÚBLICAS ====================
    
    public int ModuleId { get; set; }
    
    /// <summary>
    /// Identificador técnico INMUTABLE del módulo.
    /// Se usa para buscar metadata en BD, NO debe cambiar.
    /// </summary>
    public string ModuleName { get; } = "PanelAdmin";
    
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    // ==================== DATOS INYECTADOS ====================
    
    private List<ModuleComponentDto> _components = new();
    private List<ModuleActionDto> _actions = new();

    // ==================== MÉTODOS PÚBLICOS ====================

    /// <summary>
    /// Devuelve los componentes inyectados por el Host
    /// </summary>
    public List<ModuleComponentDto> GetComponents() => _components;
    
    /// <summary>
    /// Devuelve las acciones inyectadas por el Host
    /// </summary>
    public List<ModuleActionDto> GetActions() => _actions;
        
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
    /// El Host llama este método para inyectar metadata desde BD.
    /// Nota: moduleName se ignora porque ModuleName es inmutable.
    /// </summary>    
    public void SetMetadata(int moduleId, string moduleName, string displayName, string description, string version)
    {
        ModuleId = moduleId;
        // ModuleName NO se actualiza - es inmutable
        DisplayName = displayName ?? DisplayName;
        Description = description ?? Description;
        Version = version ?? Version;
    }

    // ==================== CONFIGURACIÓN DE SERVICIOS ====================
    
    /// <summary>
    /// Registra servicios de NEGOCIO y REPOSITORIOS del módulo
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        //  Registrar repositorios del módulo (capa de datos)
        services.AddScoped<IRolRepository, RolRepository>();

        //  Registrar servicios de negocio del módulo
        services.AddScoped<IRolService, RolService>();
        services.AddScoped<IUsuarioService, UsuarioService>();

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
