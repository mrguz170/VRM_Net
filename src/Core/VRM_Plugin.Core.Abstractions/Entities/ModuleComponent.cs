namespace VRM_Plugin.Core.Abstractions.Entities;

/// <summary>
/// Entidad que representa un componente Blazor (navegación/menú).
/// Mapea directamente a tabla ModuloComponentes en BD.
/// Arquitectura jerárquica usando IdParent (NULL = raíz).
/// </summary>
public class ModuleComponent
{
    // ===== IDs PARA BD =====

    /// <summary>
    /// ID del componente en BD (auto-generado por IDENTITY)
    /// </summary>
    public int IdComponent { get; set; }

    /// <summary>
    /// ID del módulo al que pertenece este componente (FK)
    /// </summary>
    public int IdModule { get; set; }

    /// <summary>
    /// ID del componente padre (NULL = componente raíz, sin padre)
    /// Define la jerarquía del menú
    /// </summary>
    public int? IdParent { get; set; }

    /// <summary>
    /// Nombre del componente para mostrar en la UI
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del componente (opcional)
    /// </summary>
    public string Description { get; set; } = string.Empty;

    // ===== NAVEGACIÓN Y ROUTING =====

    /// <summary>
    /// Ruta de la página Blazor (ej: "/finanzas/facturas")
    /// NULL si es un contenedor de menú sin página propia
    /// </summary>
    public string Route { get; set; } = string.Empty;

    /// <summary>
    /// Tipo del componente Blazor (Type del archivo .razor)
    /// ?? NO se persiste en BD, solo se usa en runtime
    /// </summary>

    // ===== PRESENTACIÓN EN MENÚ =====

    /// <summary>
    /// Indica si debe aparecer en el menú de navegación
    /// </summary>
    public bool ShowInMenu { get; set; } = true;

    /// <summary>
    /// Orden de aparición en el menú
    /// </summary>
    public int MenuOrder { get; set; } = 0;

    /// <summary>
    /// Clase CSS del icono (Remix Icons, Bootstrap Icons, etc.)
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    // ===== PERMISOS DE NAVEGACIÓN =====

    /// <summary>
    /// IDs de permisos requeridos para ACCEDER a este componente
    /// Mapea a tabla ComponentePermisos en BD
    /// 
    /// LÓGICA DE HERENCIA:
    /// - Vacío + IdParent != null ? Hereda del padre
    /// - Vacío + IdParent == null ? Público
    /// - Con valores ? Usa estos permisos
    /// </summary>
    public List<int> RequiredPermissionIds { get; set; } = new();

    // ===== ESTADO (SOFT DELETE) =====

    /// <summary>
    /// Indica si el componente está activo (soft delete pattern)
    /// </summary>
    public bool IsActive { get; set; } = true;

}
