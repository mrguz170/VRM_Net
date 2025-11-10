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

    // ===== METADATA DEL COMPONENTE =====

    /// <summary>
    /// Código técnico del componente (UNIQUE en BD)
    /// Ej: "Finanzas.Root", "Finanzas.Facturas"
    /// </summary>
    public string ComponentCode { get; set; } = string.Empty;

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
    public Type? ComponentType { get; set; }

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

    // ===== AUDITORÍA =====

    /// <summary>
    /// ? Fecha y hora de creación del registro (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ? Fecha y hora de última actualización (UTC)
    /// NULL si nunca se ha actualizado
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// ? ID del usuario que creó el registro
    /// NULL si fue creado por el sistema o migración
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// ? ID del usuario que realizó la última actualización
    /// NULL si nunca se ha actualizado
    /// </summary>
    public int? UpdatedBy { get; set; }

    // ===== NAVEGACIÓN (Entity Framework) =====

    /// <summary>
    /// Módulo al que pertenece este componente
    /// </summary>
    public Module? Module { get; set; }

    /// <summary>
    /// Componente padre (si tiene)
    /// </summary>
    public ModuleComponent? Parent { get; set; }

    /// <summary>
    /// Componentes hijos
    /// </summary>
    public List<ModuleComponent> Children { get; set; } = new();

    /// <summary>
    /// Acciones asociadas a este componente
    /// </summary>
    public List<ModuleAction> Actions { get; set; } = new();
}
