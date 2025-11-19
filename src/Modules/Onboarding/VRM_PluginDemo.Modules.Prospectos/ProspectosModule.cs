using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Modules.Prospectos.Services;
using VRM_Plugin.Modules.Prospectos.Data.Repositories;

namespace VRM_Plugin.Modules.Prospectos;

/// <summary>
/// Módulo de gestión de prospectos.
/// Implementa IModule para integrarse en el sistema de plugins.
/// ✅ REFACTORIZADO: El módulo YA NO accede a BD directamente
/// ✅ Los datos se inyectan desde el Host mediante SetComponents(), SetActions(), etc.
/// ✅ Arquitectura con IDs numéricos y permisos separados.
/// ✅ Organización mediante jerarquía de componentes (sin Category).
/// </summary>
public class ProspectosModule : IModule
{
    // ==================== CAMPOS PRIVADOS ====================
    
    // ✅ Datos inyectados por el Host (desde BD)
    private List<ModuleComponentDto> _components = new();
    private List<ModuleActionDto> _actions = new();
    private Dictionary<string, string[]> _actionPermissions = new();

    // ✅ Metadata del módulo
    private string _moduleName = string.Empty;
    private string _displayName = string.Empty;
    private string _description = string.Empty;
    private string _version = string.Empty;
    
    //private string _moduleName = "Gestión de Prospectos";
    //private string _displayName = "Gestión de Prospectos";
    //private string _description = "Módulo para gestionar solicitudes de proveedores";
    //private string _version = "1.0.0";


    // ==================== PROPIEDADES PÚBLICAS ====================

    public int ModuleId { get; set; } = 2;
    public string ModuleName => _moduleName;
    public string DisplayName => _displayName;
    public string Description => _description;
    public string Version => _version;
    
    // ==================== MÉTODOS PÚBLICOS ====================
        
    /// <summary>
    /// ✅ Devuelve los componentes inyectados por el Host
    /// </summary>
    public List<ModuleComponentDto> GetComponents()
    {
        if (_components.Count > 0)
        {
            return _components;
        }
        
        // Fallback: valores por defecto
        return GetDefaultComponents();
    }
    
    /// <summary>
    /// ✅ Devuelve las acciones inyectadas por el Host
    /// </summary>
    public List<ModuleActionDto> GetActions()
    {
        if (_actions.Count > 0)
        {
            return _actions;
        }
        
        // Fallback: valores por defecto
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
            ["Prospectos.Ver"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.AprobarFinal"] = new[] { "Admin", "GestorProspectos" }
        };
    }
    
    // ==================== VALORES POR DEFECTO (FALLBACK) ====================
    
    private List<ModuleComponentDto> GetDefaultComponents()
    {
        return new List<ModuleComponentDto>
        {
            new ModuleComponentDto 
            { 
                ComponentId = 4, 
                ModuleId = 2, 
                ParentId = null,
                Name = "Prospectos", 
                Route = "/prospectos", 
                Icon = "ri-list-check-3",              
                ShowInMenu = true, 
                MenuOrder = 10, 
                RequiredPermissionIds = new List<int> { 1, 5, 6 }, 
                IsActive = true 
            }
        };
    }
    
    private List<ModuleActionDto> GetDefaultActions()
    {
        return new List<ModuleActionDto>
        {
            new ModuleActionDto { ActionKeyId = 20, ComponentId = 4, ActionKey = "Prospectos.Ver", Name = "Ver Prospectos", Description = "Permite visualizar prospectos", RequiredPermissionIds = new List<int> { 1, 5, 6, 7, 8, 9 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 21, ComponentId = 4, ActionKey = "Prospectos.Crear", Name = "Crear Prospecto", Description = "Permite crear nuevos prospectos", RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 22, ComponentId = 4, ActionKey = "Prospectos.Editar", Name = "Editar Prospecto", Description = "Permite modificar prospectos", RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 23, ComponentId = 4, ActionKey = "Prospectos.Eliminar", Name = "Eliminar Prospecto", Description = "Permite eliminar prospectos", RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 24, ComponentId = 4, ActionKey = "Prospectos.AsignarRevisor", Name = "Asignar Revisor", Description = "Asigna un revisor a un prospecto", RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 25, ComponentId = 4, ActionKey = "Prospectos.RevisionLegal", Name = "Revisión Legal", Description = "Realiza revisión legal del prospecto", RequiredPermissionIds = new List<int> { 1, 7 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 26, ComponentId = 4, ActionKey = "Prospectos.RevisionFinanciera", Name = "Revisión Financiera", Description = "Realiza revisión financiera del prospecto", RequiredPermissionIds = new List<int> { 1, 8 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 27, ComponentId = 4, ActionKey = "Prospectos.RevisionTecnica", Name = "Revisión Técnica", Description = "Realiza revisión técnica del prospecto", RequiredPermissionIds = new List<int> { 1, 9 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 28, ComponentId = 4, ActionKey = "Prospectos.AprobarFinal", Name = "Aprobar Prospecto", Description = "Aprobación final del prospecto", RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 29, ComponentId = 4, ActionKey = "Prospectos.RechazarFinal", Name = "Rechazar Prospecto", Description = "Rechazo final del prospecto", RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 30, ComponentId = 4, ActionKey = "Prospectos.ConvertirProveedor", Name = "Convertir a Proveedor", Description = "Convierte prospecto aprobado en proveedor", RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 31, ComponentId = 4, ActionKey = "Prospectos.VerDocumentos", Name = "Ver Documentos", Description = "Permite ver documentos del prospecto", RequiredPermissionIds = new List<int> { 1, 5, 6, 7, 8 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 32, ComponentId = 4, ActionKey = "Prospectos.SolicitarDocumentos", Name = "Solicitar Documentos", Description = "Solicita documentos adicionales", RequiredPermissionIds = new List<int> { 1, 5, 7, 8 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 33, ComponentId = 4, ActionKey = "Prospectos.VerEstadisticas", Name = "Ver Estadísticas", Description = "Ver estadísticas de prospectos", RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 34, ComponentId = 4, ActionKey = "Prospectos.ExportarDatos", Name = "Exportar Datos", Description = "Exporta datos de prospectos", RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 35, ComponentId = 4, ActionKey = "Prospectos.ConfigurarAreas", Name = "Configurar Áreas", Description = "Configura áreas de revisión",  RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleActionDto { ActionKeyId = 36, ComponentId = 4, ActionKey = "Prospectos.ConfigurarFlujo", Name = "Configurar Flujo", Description = "Configura flujo de aprobación",  RequiredPermissionIds = new List<int> { 1 }, IsActive = true }
        };
    }
    
    // ==================== CONFIGURACIÓN DE SERVICIOS ====================
    
    /// <summary>
    /// ✅ Registra servicios de NEGOCIO y repositorios del módulo
    /// DatabaseHelper ya está registrado por el Host
    /// 
    /// 📌 FLUJO DE REGISTRO:
    /// 1. Host registra DatabaseHelper (con connection string)
    /// 2. Módulo registra sus repositorios (que usan DatabaseHelper)
    /// 3. Módulo registra sus servicios (que usan repositorios)
    /// 4. DI resuelve toda la cadena automáticamente
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // ✅ Registrar repositorio del módulo (capa de datos)
        services.AddScoped<IProspectoRepository, ProspectoRepository>();
        
        // ✅ Registrar servicio de negocio del módulo
        services.AddScoped<IProspectoService, ProspectoService>();
    }

    public bool IsEnabledForClient(string clienteId) => true;

    public async Task OnModuleLoadedAsync()
    {
        Console.WriteLine($"[{ModuleName}] Módulo cargado - IdModule: {ModuleId}");
        Console.WriteLine($"[{ModuleName}] DisplayName: {DisplayName}");
        Console.WriteLine($"[{ModuleName}] Componentes: {_components.Count}, Acciones: {_actions.Count}");
        await Task.CompletedTask;
    }
}