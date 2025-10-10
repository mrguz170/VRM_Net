using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_PluginDemo.Core.Abstractions;
using VRM_PluginDemo.Modules.Prospectos.Services;
using VRM_PluginDemo.Modules.Prospectos.Components;

namespace VRM_PluginDemo.Modules.Prospectos;

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

    // ==================== COMPONENTES BLAZOR (NUEVO) ====================

    public List<ModuleComponentInfo> GetComponents()
    {
        return new List<ModuleComponentInfo>
        {
            new ModuleComponentInfo
            {
                Name = "Prospectos",
                Route = "/prospectos",
                ComponentType = typeof(VRM_PluginDemo.Modules.Prospectos.Components.Prospectos),
                Icon = "bi-people-fill",
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