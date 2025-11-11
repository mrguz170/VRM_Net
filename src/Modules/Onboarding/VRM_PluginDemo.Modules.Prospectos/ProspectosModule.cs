using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Modules.Prospectos.Services;
using VRM_Plugin.Modules.Prospectos.Components;


namespace VRM_Plugin.Modules.Prospectos;

/// <summary>
/// Módulo de gestión de prospectos.
/// Implementa IModule para integrarse en el sistema de plugins.
/// </summary>
public class ProspectosModule : IModule
{
    // ==================== IDENTIFICACIÓN ====================

    public string ModuleId => "Prospectos";

    public string DisplayName => "Gestión de Prospectos";

    public string Description =>
        "Módulo para gestionar solicitudes de proveedores. " +
        "Permite recibir, revisar y aprobar empresas que desean ser proveedores.";

    public string Version => "1.0.0";

    public string Author => "Equipo de Desarrollo VRM";

    // ==================== PRESENTACIÓN VISUAL ====================

    /// <summary>
    /// Icono Remix: Usuario con lupa - representa búsqueda/gestión de prospectos
    /// </summary>
    public string Icon => "ri-list-check-3";

    // ==================== CATEGORIZACIÓN ====================

    public string Category => "Administración";

    // ==================== DEPENDENCIAS ====================

    public List<string> Dependencies => new()
    {
        // Este módulo no tiene dependencias de otros módulos
    };

    // ==================== PERMISOS ====================

    public List<string> RequiredPermissions => new()
    {
        "Admin",
        "GestorProspectos",
        "RevisorProspectos"
    };

    // ==================== PERMISOS GRANULARES POR ACCIÓN ====================

    /// <summary>
    /// Define permisos específicos para revisiones especializadas y aprobaciones finales.
    /// Permite que múltiples revisores trabajen en paralelo pero solo gestores aprueben finalmente.
    /// </summary>
    public Dictionary<string, string[]> GetActionPermissions()
    {
        return new Dictionary<string, string[]>
        {
            // ===== VISUALIZACIÓN Y GESTIÓN BÁSICA =====
            ["Prospectos.Ver"] = new[] { "Admin", "GestorProspectos", "RevisorProspectos", "RevisorLegal", "RevisorFinanzas", "RevisorTecnico" },
            ["Prospectos.Crear"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.Editar"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.Eliminar"] = new[] { "Admin", "GestorProspectos" },
            
            // ===== ASIGNACIÓN DE REVISORES =====
            ["Prospectos.AsignarRevisor"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.ReasignarRevisor"] = new[] { "Admin", "GestorProspectos" },
            
            // ===== REVISIONES POR ÁREA (Cada revisor solo su área) =====
            ["Prospectos.RevisionLegal"] = new[] { "Admin", "RevisorLegal" },
            ["Prospectos.RevisionFinanciera"] = new[] { "Admin", "RevisorFinanzas" },
            ["Prospectos.RevisionTecnica"] = new[] { "Admin", "RevisorTecnico" },
            ["Prospectos.RevisionCalidad"] = new[] { "Admin", "RevisorCalidad" },
            
            // ⭐ APROBACIONES FINALES - SOLO GESTORES
            ["Prospectos.AprobarFinal"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.RechazarFinal"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.ConvertirProveedor"] = new[] { "Admin", "GestorProspectos" },
            
            // ===== GESTIÓN DE DOCUMENTOS =====
            ["Prospectos.VerDocumentos"] = new[] { "Admin", "GestorProspectos", "RevisorProspectos", "RevisorLegal", "RevisorFinanzas" },
            ["Prospectos.SolicitarDocumentos"] = new[] { "Admin", "GestorProspectos", "RevisorLegal", "RevisorFinanzas" },
            ["Prospectos.AprobarDocumentos"] = new[] { "Admin", "GestorProspectos", "RevisorLegal" },
            
            // ===== REPORTES Y ESTADÍSTICAS =====
            ["Prospectos.VerEstadisticas"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.ExportarDatos"] = new[] { "Admin", "GestorProspectos" },
            
            // ===== CONFIGURACIÓN =====
            ["Prospectos.ConfigurarAreas"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.ConfigurarFlujo"] = new[] { "Admin" }
        };
    }


    public Dictionary<string, string[]> GetActionPermission(string id)
    {
        return new Dictionary<string, string[]>
        {
            // ===== VISUALIZACIÓN Y GESTIÓN BÁSICA =====
            ["Prospectos.Ver"] = new[] { "Admin", "GestorProspectos", "RevisorProspectos", "RevisorLegal", "RevisorFinanzas", "RevisorTecnico" },
            ["Prospectos.Crear"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.Editar"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.Eliminar"] = new[] { "Admin", "GestorProspectos" },

            // ===== ASIGNACIÓN DE REVISORES =====
            ["Prospectos.AsignarRevisor"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.ReasignarRevisor"] = new[] { "Admin", "GestorProspectos" },

            // ===== REVISIONES POR ÁREA (Cada revisor solo su área) =====
            ["Prospectos.RevisionLegal"] = new[] { "Admin", "RevisorLegal" },
            ["Prospectos.RevisionFinanciera"] = new[] { "Admin", "RevisorFinanzas" },
            ["Prospectos.RevisionTecnica"] = new[] { "Admin", "RevisorTecnico" },
            ["Prospectos.RevisionCalidad"] = new[] { "Admin", "RevisorCalidad" },

            // ⭐ APROBACIONES FINALES - SOLO GESTORES
            ["Prospectos.AprobarFinal"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.RechazarFinal"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.ConvertirProveedor"] = new[] { "Admin", "GestorProspectos" },

            // ===== GESTIÓN DE DOCUMENTOS =====
            ["Prospectos.VerDocumentos"] = new[] { "Admin", "GestorProspectos", "RevisorProspectos", "RevisorLegal", "RevisorFinanzas" },
            ["Prospectos.SolicitarDocumentos"] = new[] { "Admin", "GestorProspectos", "RevisorLegal", "RevisorFinanzas" },
            ["Prospectos.AprobarDocumentos"] = new[] { "Admin", "GestorProspectos", "RevisorLegal" },

            // ===== REPORTES Y ESTADÍSTICAS =====
            ["Prospectos.VerEstadisticas"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.ExportarDatos"] = new[] { "Admin", "GestorProspectos" },

            // ===== CONFIGURACIÓN =====
            ["Prospectos.ConfigurarAreas"] = new[] { "Admin", "GestorProspectos" },
            ["Prospectos.ConfigurarFlujo"] = new[] { "Admin" }
        };
    }
    // ==================== COMPONENTES BLAZOR (NUEVO) ====================

    public List<ModuleComponentInfo> GetComponents()
    {
        return new List<ModuleComponentInfo>
        {
            new ModuleComponentInfo
            {
                Name = "Prospectos",
                Route = "/prospectos",
                ComponentType = typeof(VRM_Plugin.Modules.Prospectos.Components.Prospectos),
                ShowInMenu = true,
                MenuOrder = 10
            }
            // Aquí puedes agregar más componentes del módulo en el futuro:
            // - ListaProspectos
            // - DetalleProspecto
            // - FormularioProspecto, etc.
        };
    }

    // ==================== CONFIGURACIÓN ====================

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Registrar el servicio de prospectos
        services.AddScoped<IProspectoService, ProspectoService>();
    }

    // ==================== HABILITACIÓN POR CLIENTE ====================

    public bool IsEnabledForClient(string clienteId)
    {
        // TODO: Implementar consulta a ConfiguracionNegocio del cliente
        return true;
    }

    // ==================== CICLO DE VIDA ====================

    public async Task OnModuleLoadedAsync()
    {
        Console.WriteLine($"[{ModuleId}] Módulo cargado exitosamente - Versión {Version}");
      Console.WriteLine($"[{ModuleId}] Componentes registrados: {GetComponents().Count}");

        await Task.CompletedTask;
    }
}