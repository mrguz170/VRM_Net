using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Modules.Finanzas.Services;
using VRM_Plugin.Modules.Finanzas.Components;

namespace VRM_Plugin.Modules.Finanzas;

/// <summary>
/// Módulo de gestión financiera.
/// Implementa IModule para integrarse en el sistema de plugins.
/// </summary>
public class FinanzasModule : IModule
{
    // ==================== IDENTIFICACIÓN ====================

    public string ModuleId => "Finanzas";

    public string DisplayName => "Gestión de Finanzas";

    public string Description =>
        "Módulo para gestionar operaciones financieras. " +
        "Incluye facturas, pagos, conciliaciones y cuentas por pagar.";

    public string Version => "1.0.0";

    public string Author => "Equipo de Desarrollo VRM";

    // ==================== PRESENTACIÓN VISUAL ====================

    /// <summary>
    /// Icono Remix: Moneda de dólar en círculo - representa operaciones financieras
    /// </summary>   
    public string Icon => "ri-money-dollar-circle-line";

    // ==================== CATEGORIZACIÓN ====================

    public string Category => "Finanzas";

    // ==================== DEPENDENCIAS ====================

    public List<string> Dependencies => new()
    {
        // Este módulo no tiene dependencias de otros módulos
    };

    // ==================== PERMISOS ====================

    public List<string> RequiredPermissions => new()
    {
        "Admin",
        "GestorFinanzas",
        "Contador"
    };

    // ==================== PERMISOS GRANULARES POR ACCIÓN ====================

    /// <summary>
    /// Define permisos específicos para cada acción dentro del módulo.
    /// Esto permite control fino: un coordinador puede crear facturas pero solo un gerente puede timbrarlas.
    /// </summary>
    public Dictionary<string, string[]> GetActionPermissions()
    {
        return new Dictionary<string, string[]>
        {
            // ===== GESTIÓN DE FACTURAS =====
            ["Finanzas.Facturas.Ver"] = new[] { "Admin", "GerenteFinanzas", "CoordinadorFinanzas", "Contador" },
            ["Finanzas.Facturas.Crear"] = new[] { "Admin", "GerenteFinanzas", "CoordinadorFinanzas" },
            ["Finanzas.Facturas.Editar"] = new[] { "Admin", "GerenteFinanzas", "CoordinadorFinanzas" },
            ["Finanzas.Facturas.Eliminar"] = new[] { "Admin", "GerenteFinanzas" },
            
            // ⭐ ACCIONES CRÍTICAS - SOLO GERENTES
            ["Finanzas.Facturas.TimbrarSAT"] = new[] { "Admin", "GerenteFinanzas" },
            ["Finanzas.Facturas.CancelarTimbrada"] = new[] { "Admin", "GerenteFinanzas" },
            
            // ===== GESTIÓN DE PAGOS =====
            ["Finanzas.Pagos.Ver"] = new[] { "Admin", "GerenteFinanzas", "CoordinadorFinanzas", "Contador" },
            ["Finanzas.Pagos.Crear"] = new[] { "Admin", "GerenteFinanzas", "CoordinadorFinanzas" },
            ["Finanzas.Pagos.Editar"] = new[] { "Admin", "GerenteFinanzas", "CoordinadorFinanzas" },
            
            // ⭐ AUTORIZACIÓN DE PAGOS - SOLO GERENTES
            ["Finanzas.Pagos.Autorizar"] = new[] { "Admin", "GerenteFinanzas" },
            ["Finanzas.Pagos.Cancelar"] = new[] { "Admin", "GerenteFinanzas" },
            
            // ===== CONCILIACIONES BANCARIAS =====
            ["Finanzas.Conciliacion.Ver"] = new[] { "Admin", "GerenteFinanzas", "Contador" },
            ["Finanzas.Conciliacion.Ejecutar"] = new[] { "Admin", "GerenteFinanzas" },
            ["Finanzas.Conciliacion.Aprobar"] = new[] { "Admin", "GerenteFinanzas" },
            
            // ===== REPORTES FINANCIEROS =====
            ["Finanzas.Reportes.VerGenerales"] = new[] { "Admin", "GerenteFinanzas", "CoordinadorFinanzas", "Contador" },
            
            // ⭐ REPORTES CONFIDENCIALES - SOLO GERENTES
            ["Finanzas.Reportes.VerSensibles"] = new[] { "Admin", "GerenteFinanzas" },
            ["Finanzas.Reportes.ExportarSensibles"] = new[] { "Admin", "GerenteFinanzas" },
            
            // ===== CONFIGURACIÓN DEL MÓDULO =====
            ["Finanzas.Configuracion.Ver"] = new[] { "Admin", "GerenteFinanzas" },
            ["Finanzas.Configuracion.Modificar"] = new[] { "Admin" }
        };
    }

    // ==================== COMPONENTES BLAZOR ====================

    public List<ModuleComponentInfo> GetComponents()
    {
        return new List<ModuleComponentInfo>
        {
            new ModuleComponentInfo
            {
                Name = "Finanzas",
                Route = "/finanzas",
                ComponentType = typeof(Components.Finanzas),
                ShowInMenu = true,
                MenuOrder = 20
            }
        };
    }

    // ==================== CONFIGURACIÓN ====================

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Registrar servicios del módulo
        services.AddScoped<IFacturaService, FacturaService>();
        services.AddScoped<IPagoService, PagoService>();
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
