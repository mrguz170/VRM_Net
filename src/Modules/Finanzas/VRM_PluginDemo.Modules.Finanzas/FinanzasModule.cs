using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Core.Abstractions.Entities;
using VRM_Plugin.Modules.Finanzas.Services;
using VRM_Plugin.Modules.Finanzas.Components;

namespace VRM_Plugin.Modules.Finanzas;

/// <summary>
/// Módulo de gestión financiera.
/// Implementa IModule para integrarse en el sistema de plugins.
/// ✅ Arquitectura con IDs numéricos y permisos separados (navegación vs acciones).
/// ✅ Organización mediante jerarquía de componentes (sin Category).
/// </summary>
public class FinanzasModule : IModule
{
    public int IdModule { get; set; } = 1;
    public string ModuleName => "Finanzas";
    public string DisplayName => "Gestión de Finanzas";
    public string Description => "Módulo para gestionar operaciones financieras. Incluye facturas, pagos, conciliaciones y cuentas por pagar.";
    public string Version => "1.0.0";

    public List<ModuleComponent> GetComponents()
    {
        return new List<ModuleComponent>
        {
            // ===== COMPONENTE RAÍZ (ACTÚA COMO CATEGORÍA) =====
            new ModuleComponent 
            { 
                IdComponent = 1, 
                IdModule = 1, 
                IdParent = null,  // ✅ NULL = Categoría raíz en menú
                ComponentCode = "Finanzas.Root", 
                Name = "Finanzas", 
                Description = "Módulo principal de finanzas", 
                Route = "",  // Sin ruta, solo contenedor
                Icon = "ri-money-dollar-circle-line", 
                MenuOrder = 20, 
                ShowInMenu = true, 
                ComponentType = null,  // Sin componente Blazor, solo contenedor
                RequiredPermissionIds = new List<int> { 1, 2, 3, 4 }, 
                IsActive = true 
            },
            
            // ===== SUBMENÚ: FACTURAS =====
            new ModuleComponent 
            { 
                IdComponent = 2, 
                IdModule = 1, 
                IdParent = 1,  // ✅ Hijo de "Finanzas"
                ComponentCode = "Finanzas.Facturas", 
                Name = "Facturas", 
                Description = "Gestión de facturas", 
                Route = "/finanzas/facturas", 
                Icon = "ri-file-list-3-line", 
                MenuOrder = 1, 
                ShowInMenu = true, 
                ComponentType = typeof(Components.Facturas), 
                RequiredPermissionIds = new List<int>(),  // Hereda del padre
                IsActive = true 
            },
            
            // ===== SUBMENÚ: COBROS Y PAGOS =====
            new ModuleComponent 
            { 
                IdComponent = 3, 
                IdModule = 1, 
                IdParent = 1,  // ✅ Hijo de "Finanzas"
                ComponentCode = "Finanzas.CobrosYPagos", 
                Name = "Cobros y Pagos", 
                Description = "Gestión de cobros y pagos", 
                Route = "/finanzas/cobros-pagos", 
                Icon = "ri-exchange-dollar-line", 
                MenuOrder = 2, 
                ShowInMenu = true, 
                ComponentType = typeof(Components.CobrosYPagos), 
                RequiredPermissionIds = new List<int> { 1, 2, 3 }, 
                IsActive = true 
            }
        };
    }

    public List<ModuleAction> GetActions()
    {
        return new List<ModuleAction>
        {
            new ModuleAction { IdAction = 1, IdComponent = 2, ActionKey = "Finanzas.Facturas.Ver", Name = "Ver Facturas", Description = "Permite visualizar el listado de facturas", IdActionType = 1, RequiredPermissionIds = new List<int> { 1, 2, 3, 4 }, IsActive = true },
            new ModuleAction { IdAction = 2, IdComponent = 2, ActionKey = "Finanzas.Facturas.Crear", Name = "Crear Factura", Description = "Permite crear nuevas facturas", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 2, 3 }, IsActive = true },
            new ModuleAction { IdAction = 3, IdComponent = 2, ActionKey = "Finanzas.Facturas.Editar", Name = "Editar Factura", Description = "Permite modificar facturas existentes", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 2, 3 }, IsActive = true },
            new ModuleAction { IdAction = 4, IdComponent = 2, ActionKey = "Finanzas.Facturas.Eliminar", Name = "Eliminar Factura", Description = "Permite eliminar facturas", IdActionType = 3, RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleAction { IdAction = 5, IdComponent = 2, ActionKey = "Finanzas.Facturas.TimbrarSAT", Name = "Timbrar en SAT", Description = "Envía factura al SAT para timbrado fiscal", IdActionType = 3, RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleAction { IdAction = 6, IdComponent = 2, ActionKey = "Finanzas.Facturas.CancelarTimbrada", Name = "Cancelar Factura Timbrada", Description = "Cancela una factura ya timbrada en el SAT", IdActionType = 3, RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleAction { IdAction = 7, IdComponent = 3, ActionKey = "Finanzas.Pagos.Ver", Name = "Ver Pagos", Description = "Permite visualizar pagos", IdActionType = 1, RequiredPermissionIds = new List<int> { 1, 2, 3, 4 }, IsActive = true },
            new ModuleAction { IdAction = 8, IdComponent = 3, ActionKey = "Finanzas.Pagos.Crear", Name = "Crear Pago", Description = "Permite crear nuevos pagos", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 2, 3 }, IsActive = true },
            new ModuleAction { IdAction = 9, IdComponent = 3, ActionKey = "Finanzas.Pagos.Editar", Name = "Editar Pago", Description = "Permite modificar pagos existentes", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 2, 3 }, IsActive = true },
            new ModuleAction { IdAction = 10, IdComponent = 3, ActionKey = "Finanzas.Pagos.Autorizar", Name = "Autorizar Pago", Description = "Autoriza un pago para su ejecución", IdActionType = 3, RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleAction { IdAction = 11, IdComponent = 3, ActionKey = "Finanzas.Pagos.Cancelar", Name = "Cancelar Pago", Description = "Cancela un pago autorizado", IdActionType = 3, RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleAction { IdAction = 12, IdComponent = null, ActionKey = "Finanzas.Conciliacion.Ver", Name = "Ver Conciliaciones", Description = "Permite visualizar conciliaciones bancarias", IdActionType = 1, RequiredPermissionIds = new List<int> { 1, 2, 4 }, IsActive = true },
            new ModuleAction { IdAction = 13, IdComponent = null, ActionKey = "Finanzas.Conciliacion.Ejecutar", Name = "Ejecutar Conciliación", Description = "Ejecuta proceso de conciliación bancaria", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleAction { IdAction = 14, IdComponent = null, ActionKey = "Finanzas.Conciliacion.Aprobar", Name = "Aprobar Conciliación", Description = "Aprueba una conciliación bancaria", IdActionType = 3, RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleAction { IdAction = 15, IdComponent = null, ActionKey = "Finanzas.Reportes.VerGenerales", Name = "Ver Reportes Generales", Description = "Acceso a reportes financieros generales", IdActionType = 1, RequiredPermissionIds = new List<int> { 1, 2, 3, 4 }, IsActive = true },
            new ModuleAction { IdAction = 16, IdComponent = null, ActionKey = "Finanzas.Reportes.VerSensibles", Name = "Ver Reportes Confidenciales", Description = "Acceso a reportes financieros sensibles", IdActionType = 1, RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleAction { IdAction = 17, IdComponent = null, ActionKey = "Finanzas.Reportes.ExportarSensibles", Name = "Exportar Reportes Confidenciales", Description = "Exporta reportes financieros confidenciales", IdActionType = 3, RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleAction { IdAction = 18, IdComponent = null, ActionKey = "Finanzas.Configuracion.Ver", Name = "Ver Configuración", Description = "Permite ver configuración del módulo", IdActionType = 1, RequiredPermissionIds = new List<int> { 1, 2 }, IsActive = true },
            new ModuleAction { IdAction = 19, IdComponent = null, ActionKey = "Finanzas.Configuracion.Modificar", Name = "Modificar Configuración", Description = "Permite modificar configuración del módulo", IdActionType = 3, RequiredPermissionIds = new List<int> { 1 }, IsActive = true }
        };
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFacturaService, FacturaService>();
        services.AddScoped<IPagoService, PagoService>();
    }

    public bool IsEnabledForClient(string clienteId) => true;

    public async Task OnModuleLoadedAsync()
    {
        Console.WriteLine($"[{ModuleName}] Módulo cargado - IdModule: {IdModule}");
        Console.WriteLine($"[{ModuleName}] Componentes: {GetComponents().Count}, Acciones: {GetActions().Count}");
        await Task.CompletedTask;
    }
}
