using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Data;
using VRM_Plugin.Blazor.Server.Services;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Core.Abstractions.Entities;
using VRM_Plugin.Data;
using VRM_Plugin.Modules.Finanzas.Components;
using VRM_Plugin.Modules.Finanzas.Services;

namespace VRM_Plugin.Modules.Finanzas;

/// <summary>
/// Módulo de gestión financiera.
/// Implementa IModule para integrarse en el sistema de plugins.
/// ✅ Arquitectura con IDs numéricos y permisos separados (navegación vs acciones).
/// ✅ Organización mediante jerarquía de componentes (sin Category).
/// </summary>
public class FinanzasModule : IModule
{
    // ==================== IDENTIFICACIÓN ====================
    private readonly IConfiguration _configuration;
    private string? _connectionString;
    private Dictionary<string, string[]>? _cachedPermissions = new Dictionary<string, string[]>();
    private List<ModuleComponent>? _ComponentValues = new List<ModuleComponent>();
    private string? _namemodule;
    private string? _displayname;
    private string? _description;
    private string? _version;

    public int IdModule { get; set; } = 1;
    public string ModuleName => "Finanzas";
    public string DisplayName => _displayname;
    public string Description => _description;
    public string Version => _version;

    public List<ModuleComponent> GetComponents()
    {
        try
        {
            DatabaseHelper Ds = new DatabaseHelper(_connectionString);
           
                var component = Ds.ExecuteStoredProcedure("sp_get_component", new Dictionary<string, object>
        {
            { "module_id", 1 }
        });

            _ComponentValues = new List<ModuleComponent>();

                      if (_ComponentValues.Count == 0)
                        foreach (DataRow row in component.Rows)
                        {

                        var permisos = Convert.ToString(row["roles"]).Split(',', StringSplitOptions.RemoveEmptyEntries);
                        List<int> listaroles = permisos.Select(int.Parse).ToList();

                        _ComponentValues.Add(new ModuleComponent
                            {
                                IdComponent = Convert.ToInt32(row["component_id"]),
                                IdModule = Convert.ToInt32(row["module_id"]),
                                IdParent = row["parent_id"] != DBNull.Value ? Convert.ToInt32(row["parent_id"]) : null,
                                Name = row["component_name"].ToString()!,
                                Description = row["description"].ToString()!,
                                Route = row["route"].ToString()!,
                                Icon = row["icon"].ToString()!,
                                MenuOrder = Convert.ToInt32(row["menu_order"]),
                                ShowInMenu = Convert.ToBoolean(row["show_in_menu"]),
                                RequiredPermissionIds = permisos != null ? listaroles : new List<int>(1),
                                IsActive = Convert.ToBoolean(row["is_active"])
                            });
                        
                }

                return _ComponentValues;
            }
        catch (Exception ex)
        {
            return new List<ModuleComponent>
            {
                new ModuleComponent
                {
                    IdComponent = 1,
                    IdModule = 1,
                    IdParent = null,  // ✅ NULL = Categoría raíz en menú
                    Name = "Finanzas",
                    Description = "Módulo principal de finanzas",
                    Route = "",  // Sin ruta, solo contenedor
                    Icon = "ri-money-dollar-circle-line",
                    MenuOrder = 20,
                    ShowInMenu = true,
                    RequiredPermissionIds = new List<int> { 1, 2, 3, 4 },
                    IsActive = true
                }
            };
        }
    }

    public void GetModule() {

        try
        {
            DatabaseHelper Ds = new DatabaseHelper(_connectionString);

            var module_info = Ds.ExecuteStoredProcedure("sp_get_module_info", new Dictionary<string, object>
        {
            { "module_id", IdModule }
        });

            foreach (DataRow row in module_info.Rows)
            {
                _namemodule = row["module_name"].ToString()!;
                _displayname = row["display_name"].ToString()!;
                _version = row["version"].ToString()!;
                _description    = row["description"].ToString()!;
            }
            
            }
        catch (Exception ex)
        {

        }
        }

    public Dictionary<string, string[]> GetActionPermission(string id)
    {
        try
        {
            DatabaseHelper Ds = new DatabaseHelper(_connectionString);

            var permissions = Ds.ExecuteStoredProcedure("ConsultaPermisos", new Dictionary<string, object>
        {
            { "Modulo", "Finanzas" }
        });

            foreach (DataRow row in permissions.Rows)
            {
                string key = row["PermisoId"].ToString()!;  // nombre de columna clave
                string permisosStr = row["Roles"].ToString()!; // valores separados por coma

                // convertir el string en arreglo
                string[] valores = permisosStr.Split(',', StringSplitOptions.RemoveEmptyEntries);

                _cachedPermissions[key] = valores;
            }

            return _cachedPermissions;
        }
        catch (Exception ex)
        {
            // Si hay error, usar los permisos por defecto definidos en el código
            return new Dictionary<string, string[]>
            {
                ["Finanzas.Facturas.Crear"] = new[] { "Admin", "GerenteFinanzas", "CoordinadorFinanzas" },
                // ... resto de los permisos ...
            };
        }
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
        // Actualizar la cadena de conexión si es necesario
        _connectionString ??= configuration.GetConnectionString("DefaultConnection");

        // Registrar servicios del módulo
        services.AddScoped<IFacturaService, FacturaService>();
        services.AddScoped<IPagoService, PagoService>();
        services.AddScoped<IModule, FinanzasModule>();
    }

    public bool IsEnabledForClient(string clienteId) => true;

    public async Task OnModuleLoadedAsync()
    {
        Console.WriteLine($"[{ModuleName}] Módulo cargado - IdModule: {IdModule}");
        Console.WriteLine($"[{ModuleName}] Componentes: {GetComponents().Count}, Acciones: {GetActions().Count}");
        await Task.CompletedTask;
    }

}
