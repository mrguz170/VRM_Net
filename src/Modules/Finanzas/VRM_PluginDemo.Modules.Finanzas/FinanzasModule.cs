using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_PluginDemo.Core.Abstractions;
using VRM_PluginDemo.Modules.Finanzas.Services;

namespace VRM_PluginDemo.Modules.Finanzas;

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

    // ==================== COMPONENTES BLAZOR ====================

    public List<ModuleComponentInfo> GetComponents()
    {
        return new List<ModuleComponentInfo>
        {
            new ModuleComponentInfo
            {
                Name = "Finanzas",
                Route = "/finanzas",
                ComponentType = typeof(VRM_PluginDemo.Modules.Finanzas.Components.Finanzas),
                Icon = "bi-currency-dollar",
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
