using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VRM_Plugin.Core.Abstractions.Entities;

namespace VRM_Plugin.Core.Abstractions;

/// <summary>
/// Interfaz base que todos los módulos/plugins deben implementar.
/// ✅ Arquitectura completamente basada en IDs numéricos para BD relacional.
/// ✅ Permisos separados: Navegación (componentes) vs Acciones (business logic).
/// ✅ Auditoría completa con CreatedAt, UpdatedAt, CreatedBy, UpdatedBy.
/// ✅ Organización mediante jerarquía de componentes (IdParent) en lugar de Category.
/// ✅ Módulos completamente independientes (sin dependencias entre ellos).
/// </summary>
public interface IModule
{
    // ==================== IDENTIFICACIÓN ====================

    /// <summary>
    /// ✅ ID numérico único del módulo (PK en BD).
    /// Se asigna automáticamente al persistir en BD con IDENTITY.
    /// En código (desarrollo), se simula con valores como 1, 2, 3...
    /// </summary>
    int IdModule { get; set; }

    /// <summary>
    /// ✅ Nombre técnico/código del módulo (para código y logging).
    /// Ej: "Finanzas", "Prospectos", "Inventario"
    /// Se usa para identificación en código pero NO como PK.
    /// En BD, se mapea a columna 'Codigo' en tabla Modulos con constraint UNIQUE.
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
    /// ✅ HOMOLOGADO: Obtiene componentes con IDs numéricos y permisos de navegación.
    /// Los componentes definen la jerarquía del menú (raíz → submenús → páginas).
    /// Los IDs son simulados en código; en producción, vienen de BD.
    /// 
    /// ORGANIZACIÓN JERÁRQUICA:
    /// - Componentes raíz (IdParent = null) actúan como CATEGORÍAS en el menú
    /// - Componentes hijos (IdParent != null) son módulos dentro de la categoría
    /// - Soporta múltiples niveles de anidación
    /// 
    /// Ejemplo:
    ///   Finanzas (IdParent = null) → Categoría
    ///     └─ Contabilidad (IdParent = 1)
    ///     └─ Tesorería (IdParent = 1)
    /// </summary>
    List<ModuleComponent> GetComponents();

    // ==================== ACCIONES GRANULARES ====================

    /// <summary>
    /// ✅ HOMOLOGADO: Obtiene acciones con IDs numéricos y permisos de ejecución.
    /// Las acciones representan operaciones específicas dentro de componentes.
    /// Cada acción tiene una relación explícita con su componente (IdComponent).
    /// </summary>
    List<ModuleAction> GetActions();

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
    /// </summary>
    Task OnModuleLoadedAsync() => Task.CompletedTask;
}