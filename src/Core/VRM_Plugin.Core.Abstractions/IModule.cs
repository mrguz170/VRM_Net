using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_Plugin.Core.Abstractions.Data.DTOs;

namespace VRM_Plugin.Core.Abstractions;

/// <summary>
/// Interfaz base que todos los módulos/plugins deben implementar.
/// ✅ Arquitectura completamente basada en IDs numéricos para BD relacional.
/// ✅ Permisos separados: Navegación (componentes) vs Acciones (business logic).
/// ✅ Organización mediante jerarquía de componentes (IdParent).
/// ✅ Módulos completamente independientes (sin dependencias entre ellos).
/// </summary>
public interface IModule
{
    // ==================== IDENTIFICACIÓN ====================

    /// <summary>
    /// ✅ ID numérico único del módulo (PK en BD).
    /// Se asigna automáticamente al persistir en BD 
    /// </summary>
    int ModuleId { get; set; }

    /// <summary>
    /// ✅ Nombre técnico del módulo 
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Nombre para mostrar en la UI (ej: "Gestión de Finanzas")
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

    // ==================== COMPONENTES Y NAVEGACIÓN ====================

    /// <summary>
    /// Obtiene componentes con IDs numéricos y permisos de navegación.
    /// Los componentes definen la jerarquía del menú (raíz → submenús → páginas).
    /// 
    /// ORGANIZACIÓN JERÁRQUICA:
    /// - Componentes raíz (IdParent = null) actúan como CATEGORÍAS en el menú
    /// - Componentes hijos (IdParent != null) son módulos dentro de la categoría
    /// - Soporta múltiples niveles de anidación
    /// 
    /// </summary>
    List<ModuleComponentDto> GetComponents();

    // ==================== ACCIONES GRANULARES ====================

    /// <summary>
    /// Obtiene acciones con IDs numéricos y permisos de ejecución.
    /// Las acciones representan operaciones específicas dentro de componentes.
    /// </summary>
    List<ModuleActionDto> GetActions();

    // ==================== CONFIGURACIÓN E INYECCIÓN DE DEPENDENCIAS ====================

    /// <summary>
    /// Registra los servicios del módulo en el contenedor de DI.
    /// </summary>
    /// <param name="services">Colección de servicios de ASP.NET Core</param>
    /// <param name="configuration">Configuración de la aplicación</param>
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

  
    /// <summary>
    /// Se ejecuta cuando el módulo se carga por primera vez en la aplicación.
    /// Útil para inicialización, migraciones de BD, carga de configuración, etc.
    /// </summary>
    Task OnModuleLoadedAsync() => Task.CompletedTask;

}