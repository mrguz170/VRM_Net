namespace VRM_Plugin.Core.Abstractions.Data.DTOs;

/// <summary>
/// Entidad que representa un componente Blazor (navegación/menú).
/// Mapea directamente a tabla ModuloComponentes en BD.
/// Arquitectura jerárquica usando IdParent (NULL = raíz). 
/// ? Ubicación: Data/DTOs/ 
/// </summary>
public class ModuleComponentDto
{
    // ===== IDs PARA BD =====

    /// <summary>
    /// ID del componente en BD 
    /// </summary>
    public int ComponentId { get; set; }

    /// <summary>
    /// ID del módulo al que pertenece este componente 
    /// </summary>
    public int ModuleId { get; set; }

    /// <summary>
    /// ID del componente padre (NULL = componente raíz, sin padre)
    /// Define la jerarquía del menú
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Nombre del componente para mostrar en la UI
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción del componente 
    /// </summary>
    public string Description { get; set; } = string.Empty;

    // ===== NAVEGACIÓN Y ROUTING =====

    /// <summary>
    /// Ruta de la página Blazor (ej: "/finanzas/facturas")
    /// NULL si es un contenedor de menú sin página propia
    /// </summary>
    public string Route { get; set; } = string.Empty;

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
    /// Clase CSS del icono (SLICED ICONS)
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    // ===== PERMISOS DE NAVEGACIÓN =====

    /// <summary>
    /// ? Propiedad temporal para recibir "roles" del SP como string "1,2,3"
    /// El parseador genérico mapeará "roles" (BD) ? "RolesString" (propiedad)
    /// Luego se parsea a RequiredPermissionIds en post-procesamiento
    /// </summary>
    public string? RolesString { get; set; }
    
    /// <summary>
    /// Alias para compatibilidad con parseador genérico
    /// Si el SP devuelve "roles", se mapeará aquí automáticamente
    /// </summary>
    public string? Roles 
    { 
        get => RolesString;
        set => RolesString = value;
    }

    /// <summary>
    /// IDs de permisos requeridos para ACCEDER a este componente
    /// Se llena después del parseo desde RolesString
    /// 
    /// LÓGICA DE HERENCIA:
    /// - Vacío + IdParent != null ? Hereda del padre
    /// - Vacío + IdParent == null ? Público
    /// - Con valores ? Usa estos permisos
    /// </summary>
    public List<int> RequiredPermissionIds { get; set; } = new();

    // ===== ESTADO (SOFT DELETE) =====

    /// <summary>
    /// Indica si el componente está activo 
    /// </summary>
    public bool IsActive { get; set; } = true;
}
