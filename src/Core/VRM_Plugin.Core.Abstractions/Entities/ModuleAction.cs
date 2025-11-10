namespace VRM_Plugin.Core.Abstractions.Entities;

/// <summary>
/// Entidad que representa una acción granular (operación/business logic).
/// Mapea directamente a tabla AccionesGranulares en BD.
/// Cada acción tiene una relación explícita con su componente mediante IdComponent.
/// </summary>
public class ModuleAction
{
    // ===== IDs PARA BD =====

    /// <summary>
    /// ID de la acción en BD (auto-generado por IDENTITY)
    /// </summary>
    public int IdAction { get; set; }

    /// <summary>
    /// ID del componente al que pertenece esta acción (FK)
    /// NULL = Acción global del módulo
    /// </summary>
    public int? IdComponent { get; set; }

    // ===== METADATA DE LA ACCIÓN =====

    /// <summary>
    /// Código de la acción (legacy para compatibilidad)
    /// Formato: "{ModuleName}.{ComponentName}.{ActionName}"
    /// Ej: "Finanzas.Facturas.TimbrarSAT"
    /// </summary>
    public string ActionKey { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo de la acción (para UI)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada de la acción
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// ID del tipo de acción (FK a tabla TiposAccion)
    /// 1 = Lectura, 2 = Escritura, 3 = Crítica
    /// </summary>
    public int IdActionType { get; set; } = 2;  // Default: Escritura

    // ===== PERMISOS DE EJECUCIÓN =====

    /// <summary>
    /// IDs de permisos requeridos para EJECUTAR esta acción
    /// Mapea a tabla AccionPermisos en BD
    /// </summary>
    public List<int> RequiredPermissionIds { get; set; } = new();

    // ===== ESTADO (SOFT DELETE) =====

    /// <summary>
    /// Indica si la acción está activa (soft delete pattern)
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
    /// Componente al que pertenece esta acción
    /// </summary>
    public ModuleComponent? Component { get; set; }

    /// <summary>
    /// Tipo de acción
    /// </summary>
    public ActionType? ActionType { get; set; }
}
