using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Core.Abstractions.Entities;
using VRM_Plugin.Modules.Prospectos.Services;
using VRM_Plugin.Data;

namespace VRM_Plugin.Modules.Prospectos;

/// <summary>
/// Módulo de gestión de prospectos.
/// Implementa IModule para integrarse en el sistema de plugins.
/// ✅ Arquitectura con IDs numéricos y permisos separados.
/// ✅ Organización mediante jerarquía de componentes (sin Category).
/// </summary>
public class ProspectosModule : IModule
{

    private readonly IConfiguration _configuration;
    private string? _connectionString;
    private Dictionary<string, string[]>? _cachedPermissions = new Dictionary<string, string[]>();

    public int IdModule { get; set; } = 2;
    public string ModuleName => "Prospectos";
    public string DisplayName => "Gestión de Prospectos";
    public string Description => "Módulo para gestionar solicitudes de proveedores. Permite recibir, revisar y aprobar empresas que desean ser proveedores.";
    public string Version => "1.0.0";

    public void GetModule()
    {

    }
    public List<ModuleComponent> GetComponents()
    {
        return new List<ModuleComponent>
        {
            // ===== COMPONENTE RAÍZ (SIN CATEGORÍA PADRE) =====
            new ModuleComponent 
            { 
                IdComponent = 4, 
                IdModule = 2, 
                IdParent = null,  // ✅ NULL = Aparece en raíz del menú
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
            new ModuleAction { IdAction = 20, IdComponent = 4, ActionKey = "Prospectos.Ver", Name = "Ver Prospectos", Description = "Permite visualizar prospectos", IdActionType = 1, RequiredPermissionIds = new List<int> { 1, 5, 6, 7, 8, 9 }, IsActive = true },
            new ModuleAction { IdAction = 21, IdComponent = 4, ActionKey = "Prospectos.Crear", Name = "Crear Prospecto", Description = "Permite crear nuevos prospectos", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleAction { IdAction = 22, IdComponent = 4, ActionKey = "Prospectos.Editar", Name = "Editar Prospecto", Description = "Permite modificar prospectos", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleAction { IdAction = 23, IdComponent = 4, ActionKey = "Prospectos.Eliminar", Name = "Eliminar Prospecto", Description = "Permite eliminar prospectos", IdActionType = 3, RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleAction { IdAction = 24, IdComponent = 4, ActionKey = "Prospectos.AsignarRevisor", Name = "Asignar Revisor", Description = "Asigna un revisor a un prospecto", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleAction { IdAction = 25, IdComponent = 4, ActionKey = "Prospectos.RevisionLegal", Name = "Revisión Legal", Description = "Realiza revisión legal del prospecto", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 7 }, IsActive = true },
            new ModuleAction { IdAction = 26, IdComponent = 4, ActionKey = "Prospectos.RevisionFinanciera", Name = "Revisión Financiera", Description = "Realiza revisión financiera del prospecto", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 8 }, IsActive = true },
            new ModuleAction { IdAction = 27, IdComponent = 4, ActionKey = "Prospectos.RevisionTecnica", Name = "Revisión Técnica", Description = "Realiza revisión técnica del prospecto", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 9 }, IsActive = true },
            new ModuleAction { IdAction = 28, IdComponent = 4, ActionKey = "Prospectos.AprobarFinal", Name = "Aprobar Prospecto", Description = "Aprobación final del prospecto", IdActionType = 3, RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleAction { IdAction = 29, IdComponent = 4, ActionKey = "Prospectos.RechazarFinal", Name = "Rechazar Prospecto", Description = "Rechazo final del prospecto", IdActionType = 3, RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleAction { IdAction = 30, IdComponent = 4, ActionKey = "Prospectos.ConvertirProveedor", Name = "Convertir a Proveedor", Description = "Convierte prospecto aprobado en proveedor", IdActionType = 3, RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleAction { IdAction = 31, IdComponent = 4, ActionKey = "Prospectos.VerDocumentos", Name = "Ver Documentos", Description = "Permite ver documentos del prospecto", IdActionType = 1, RequiredPermissionIds = new List<int> { 1, 5, 6, 7, 8 }, IsActive = true },
            new ModuleAction { IdAction = 32, IdComponent = 4, ActionKey = "Prospectos.SolicitarDocumentos", Name = "Solicitar Documentos", Description = "Solicita documentos adicionales", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 5, 7, 8 }, IsActive = true },
            new ModuleAction { IdAction = 33, IdComponent = 4, ActionKey = "Prospectos.VerEstadisticas", Name = "Ver Estadísticas", Description = "Ver estadísticas de prospectos", IdActionType = 1, RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleAction { IdAction = 34, IdComponent = 4, ActionKey = "Prospectos.ExportarDatos", Name = "Exportar Datos", Description = "Exporta datos de prospectos", IdActionType = 1, RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleAction { IdAction = 35, IdComponent = 4, ActionKey = "Prospectos.ConfigurarAreas", Name = "Configurar Áreas", Description = "Configura áreas de revisión", IdActionType = 2, RequiredPermissionIds = new List<int> { 1, 5 }, IsActive = true },
            new ModuleAction { IdAction = 36, IdComponent = 4, ActionKey = "Prospectos.ConfigurarFlujo", Name = "Configurar Flujo", Description = "Configura flujo de aprobación", IdActionType = 3, RequiredPermissionIds = new List<int> { 1 }, IsActive = true }
        };
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProspectoService, ProspectoService>();
    }

    public bool IsEnabledForClient(string clienteId) => true;

    public async Task OnModuleLoadedAsync()
    {
        Console.WriteLine($"[{ModuleName}] Módulo cargado - IdModule: {IdModule}");
        await Task.CompletedTask;
    }
}