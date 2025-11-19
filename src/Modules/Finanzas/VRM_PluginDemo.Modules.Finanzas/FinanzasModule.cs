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
/// ✅ REFACTORIZADO: El módulo YA NO accede a BD directamente
/// ✅ Los datos se inyectan desde el Host mediante SetComponents(), SetActions(), etc.
/// ✅ Arquitectura con IDs numéricos y permisos separados (navegación vs acciones).
/// ✅ Organización mediante jerarquía de componentes (sin Category).
/// </summary>
public class FinanzasModule : IModule
{
    // ==================== CAMPOS PRIVADOS ====================
    
    // ✅ Datos inyectados por el Host (desde BD)
    private List<ModuleComponentDto> _components = new();
    private List<ModuleActionDto> _actions = new();
    private Dictionary<string, string[]> _actionPermissions = new();
    
    // ✅ Metadata del módulo (se inyectan desde BD, valores vacíos por defecto)
    private string _moduleName = string.Empty;
    private string _displayName = string.Empty;
    private string _description = string.Empty;
    private string _version = string.Empty;
    
    // ==================== PROPIEDADES PÚBLICAS ====================
    
    /// <summary>
    /// ✅ ÚNICA propiedad hardcodeada: ID numérico del módulo
    /// </summary>
    public int ModuleId { get; set; } = 1;
    
    public string ModuleName => _moduleName;
    public string DisplayName => _displayName;
    public string Description => _description;
    public string Version => _version;
    
    // ==================== MÉTODOS PÚBLICOS ====================
    
    /// <summary>
    /// ✅ Método requerido por IModule (obsoleto pero necesario para compatibilidad)
    /// </summary>
    
    
    /// <summary>
    /// ✅ Devuelve los componentes inyectados por el Host
    /// Si no hay componentes inyectados, devuelve valores por defecto
    /// </summary>
    public List<ModuleComponentDto> GetComponents()
    {
        if (_components.Count > 0)
        {
            return _components;
        }
        
        // Fallback: valores por defecto si no se cargaron desde BD
        return GetDefaultComponents();
    }
    
    /// <summary>
    /// ✅ Devuelve las acciones inyectadas por el Host
    /// Si no hay acciones inyectadas, devuelve valores por defecto
    /// </summary>
    public List<ModuleActionDto> GetActions()
    {
        if (_actions.Count > 0)
        {
            return _actions;
        }
        
        // Fallback: valores por defecto si no se cargaron desde BD
        return GetDefaultActions();
    }
    
    /// <summary>
    /// ✅ Método requerido por IModule (devuelve permisos por acción)
    /// </summary>
    public Dictionary<string, string[]> GetActionPermission(string id)
    {
        return GetActionPermissions();
    }
    
    // ==================== MÉTODOS DE INYECCIÓN (Llamados por el Host) ====================
    
    /// <summary>
    /// ✅ El Host llama este método para inyectar componentes desde BD
    /// </summary>
    public void SetComponents(List<ModuleComponentDto> components)
    {
        _components = components ?? new List<ModuleComponentDto>();
    }
    
    /// <summary>
    /// ✅ El Host llama este método para inyectar acciones desde BD
    /// </summary>
    public void SetActions(List<ModuleActionDto> actions)
    {
        _actions = actions ?? new List<ModuleActionDto>();
    }
    
    /// <summary>
    /// ✅ El Host llama este método para inyectar metadata desde BD
    /// </summary>    
    public void SetMetadata(string moduleName, string displayName, string description, string version)
    {
        _moduleName = moduleName ?? _moduleName;
        _displayName = displayName ?? _displayName;
        _description = description ?? _description;
        _version = version ?? _version;
    }

    /// <summary>
    /// ✅ El Host llama este método para inyectar permisos por acción desde BD
    /// </summary>
    public void SetActionPermissions(Dictionary<string, string[]> permissions)
    {
        _actionPermissions = permissions ?? new Dictionary<string, string[]>();
    }
    
    /// <summary>
    /// ✅ Obtiene los permisos por acción (inyectados desde BD)
    /// </summary>
    public Dictionary<string, string[]> GetActionPermissions()
    {
        if (_actionPermissions.Count > 0)
        {
            return _actionPermissions;
        }
        
        // Fallback: valores por defecto
        return new Dictionary<string, string[]>
        {
            ["Finanzas.Facturas.Crear"] = new[] { "Admin", "GerenteFinanzas", "CoordinadorFinanzas" },
            ["Finanzas.Facturas.Editar"] = new[] { "Admin", "GerenteFinanzas", "CoordinadorFinanzas" },
            ["Finanzas.Facturas.TimbrarSAT"] = new[] { "Admin", "GerenteFinanzas" }
        };
    }
    
    // ==================== VALORES POR DEFECTO (FALLBACK) ====================
    
    /// <summary>
    /// Valores por defecto si no se cargaron componentes desde BD
    /// </summary>
    private List<ModuleComponentDto> GetDefaultComponents()
    {
        return new List<ModuleComponentDto>
        {
            new ModuleComponentDto
            {
                ComponentId = 1,
                ModuleId = 1,
                ParentId = null,
                Name = "Finanzas",
                Description = "Módulo principal de finanzas",
                Route = "",
                Icon = "ri-money-dollar-circle-line",
                MenuOrder = 20,
                ShowInMenu = true,
                RequiredPermissionIds = new List<int> { 1, 2, 3, 4 },
                IsActive = true
            }
        };
    }
    
    /// <summary>
    /// Valores por defecto si no se cargaron acciones desde BD
    /// </summary>
    private List<ModuleActionDto> GetDefaultActions()
    {
        return new List<ModuleActionDto>
        {
            new ModuleActionDto { ActionKeyId = 1, ComponentId = 2, ActionKey = "Finanzas.Facturas.Ver", Name = "Ver Facturas", Description = "Permite visualizar el listado de facturas",  RequiredPermissionIds = new List<int> { 1, 2, 3, 4 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 2, ComponentId = 2, ActionKey = "Finanzas.Facturas.Crear", Name = "Crear Factura", Description = "Permite crear nuevas facturas", RequiredPermissionIds = new List<int> { 1, 2, 3 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 3, ComponentId = 2, ActionKey = "Finanzas.Facturas.Editar", Name = "Editar Factura", Description = "Permite modificar facturas existentes",  RequiredPermissionIds = new List<int> { 1, 2, 3 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 4, ComponentId = 2, ActionKey = "Finanzas.Facturas.Eliminar", Name = "Eliminar Factura", Description = "Permite eliminar facturas",  RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 5, ComponentId = 2, ActionKey = "Finanzas.Facturas.TimbrarSAT", Name = "Timbrar en SAT", Description = "Envía factura al SAT para timbrado fiscal",  RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 6, ComponentId = 2, ActionKey = "Finanzas.Facturas.CancelarTimbrada", Name = "Cancelar Factura Timbrada", Description = "Cancela una factura ya timbrada en el SAT",  RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 7, ComponentId = 3, ActionKey = "Finanzas.Pagos.Ver", Name = "Ver Pagos", Description = "Permite visualizar pagos",  RequiredPermissionIds = new List<int> { 1, 2, 3, 4 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 8, ComponentId = 3, ActionKey = "Finanzas.Pagos.Crear", Name = "Crear Pago", Description = "Permite crear nuevos pagos", RequiredPermissionIds = new List<int> { 1, 2, 3 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 9, ComponentId = 3, ActionKey = "Finanzas.Pagos.Editar", Name = "Editar Pago", Description = "Permite modificar pagos existentes", RequiredPermissionIds = new List<int> { 1, 2, 3 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 19,ComponentId = null, ActionKey = "Finanzas.Configuracion.Modificar", Name = "Modificar Configuración", Description = "Permite modificar configuración del módulo",  RequiredPermissionIds = new List<int> { 1 }, IsActive = true }
        };
    }
    
    // ==================== CONFIGURACIÓN DE SERVICIOS ====================
    
    /// <summary>
    /// ✅ Registra servicios de NEGOCIO y repositorios del módulo
    /// DatabaseHelper ya está registrado por el Host
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // ✅ Registrar repositorios del módulo (capa de datos)
        services.AddScoped<IFacturaRepository, FacturaRepository>();
        services.AddScoped<IPagoRepository, PagoRepository>();
        
        // ✅ Registrar servicios de negocio del módulo
        services.AddScoped<IFacturaService, FacturaService>();
        services.AddScoped<IPagoService, PagoService>();
    }
    
    public bool IsEnabledForClient(string clienteId) => true;
    
    public async Task OnModuleLoadedAsync()
    {
        Console.WriteLine($"[{ModuleName}] Módulo cargado - ModuleId: {ModuleId}");
        Console.WriteLine($"[{ModuleName}] DisplayName: {DisplayName}");
        Console.WriteLine($"[{ModuleName}] Componentes: {_components.Count}, Acciones: {_actions.Count}");
        await Task.CompletedTask;
    }
}
