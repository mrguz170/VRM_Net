using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace VRM_PluginDemo.Core.Abstractions;

/// <summary>
/// Interfaz base que todos los módulos/plugins deben implementar.
/// Versión empresarial con soporte para dependencias, permisos y categorización.
/// </summary>
public interface IModule
{
    // ==================== IDENTIFICACIÓN ====================

    /// <summary>
    /// Identificador único del módulo (ej: "Prospectos", "Facturas")
    /// Debe ser único en todo el sistema.
    /// </summary>
    string ModuleId { get; }

    /// <summary>
    /// Nombre para mostrar en la UI (ej: "Gestión de Prospectos")
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Descripción detallada del módulo
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Versión del módulo (formato: "1.0.0")
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Autor o equipo responsable del módulo
    /// </summary>
    string Author { get; }

    // ==================== CATEGORIZACIÓN ====================

    /// <summary>
    /// Categoría del módulo para organización en la UI
    /// Ejemplos: "Fiscal", "Administración", "Prospectos", "Reportes"
    /// </summary>
    string Category { get; }

    // ==================== DEPENDENCIAS ====================

    /// <summary>
    /// Lista de IDs de módulos que este módulo requiere para funcionar.
    /// Ejemplo: El módulo "Facturas" puede requerir "ConfiguracionFiscal"
    /// </summary>
    List<string> Dependencies { get; }

    // ==================== PERMISOS Y SEGURIDAD ====================

    /// <summary>
    /// Lista de permisos/roles que el usuario necesita para acceder a este módulo.
    /// Ejemplo: ["Admin", "GestorProspectos", "Revisor"]
    /// El usuario necesita AL MENOS UNO de estos roles para ver el módulo.
    /// </summary>
    List<string> RequiredPermissions { get; }

    /// <summary>
    /// ⭐ NUEVO: Permisos granulares por acción dentro del módulo.
    /// Permite control fino sobre qué usuarios pueden realizar acciones específicas.
    /// 
    /// Formato de la clave: "{ModuleId}.{Entidad}.{Acción}"
    /// Ejemplos:
    ///   - "Finanzas.Facturas.TimbrarSAT" → Solo gerentes
    ///   - "Finanzas.Pagos.Autorizar" → Solo gerentes
    ///   - "Finanzas.Facturas.Ver" → Gerentes, coordinadores, contadores
    /// 
    /// Valor: Array de roles permitidos para esa acción
    /// </summary>
    /// <returns>Diccionario de acciones y roles permitidos</returns>
    Dictionary<string, string[]> GetActionPermissions();

    // ==================== COMPONENTES BLAZOR ====================

    /// <summary>
    /// Obtiene la información de los componentes Blazor del módulo
    /// </summary>
    List<ModuleComponentInfo> GetComponents();

    // ==================== CONFIGURACIÓN E INYECCIÓN DE DEPENDENCIAS ====================

    /// <summary>
    /// Registra los servicios del módulo en el contenedor de DI.
    /// Aquí cada módulo registra sus repositorios, servicios, validadores, etc.
    /// </summary>
    /// <param name="services">Colección de servicios de ASP.NET Core</param>
    /// <param name="configuration">Configuración de la aplicación</param>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    // ==================== HABILITACIÓN POR CLIENTE ====================

    /// <summary>
    /// Verifica si este módulo está habilitado para un cliente específico
    /// según su ConfiguracionNegocio.
    /// </summary>
    /// <param name="clienteId">ID del cliente</param>
    /// <returns>True si el módulo está habilitado para el cliente</returns>
    bool IsEnabledForClient(string clienteId);

    // ==================== CICLO DE VIDA (Opcional) ====================

    /// <summary>
    /// Se ejecuta cuando el módulo se carga por primera vez en la aplicación.
    /// Útil para inicialización, migraciones de BD, carga de configuración, etc.
    /// Implementación por defecto: no hace nada.
    /// </summary>
    Task OnModuleLoadedAsync() => Task.CompletedTask;
}

/// <summary>
/// Información de un componente Blazor dentro de un módulo
/// </summary>
public class ModuleComponentInfo
{
    /// <summary>
    /// Nombre del componente (ej: "Prospectos", "ListaProspectos")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Ruta de la página (ej: "/prospectos", "/facturas/crear")
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// Tipo del componente (Type del .razor)
    /// </summary>
    public Type ComponentType { get; set; } = null!;

    /// <summary>
    /// Icono para mostrar en menús (clase CSS, ej: "bi-people-fill")
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Indica si debe aparecer en el menú de navegación
    /// </summary>
    public bool ShowInMenu { get; set; } = true;

    /// <summary>
    /// Orden en el menú
    /// </summary>
    public int MenuOrder { get; set; } = 0;
}