namespace VRM_Plugin.Core.Abstractions.Entities;

/// <summary>
/// Entidad que representa un tipo de acción del sistema.
/// Mapea a tabla TiposAccion en BD.
/// Catálogo de tipos de acción disponibles.
/// </summary>
public class ActionType
{
    /// <summary>
    /// ID del tipo de acción (PK en BD)
    /// </summary>
    public int IdActionType { get; set; }

    /// <summary>
    /// Código técnico del tipo (UNIQUE en BD)
    /// Ej: "Lectura", "Escritura", "Critica"
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo para UI
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada del tipo de acción
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Nivel de severidad para auditoría
    /// 1 = Baja (Lectura), 2 = Media (Escritura), 3 = Alta (Crítica)
    /// </summary>
    public int SeverityLevel { get; set; }

    /// <summary>
    /// Indica si las acciones de este tipo requieren auditoría obligatoria
    /// </summary>
    public bool RequiresAudit { get; set; }

    /// <summary>
    /// Color para badges en UI
    /// Valores: "primary", "success", "warning", "danger"
    /// </summary>
    public string ColorUI { get; set; } = "primary";

    /// <summary>
    /// Indica si el tipo está activo (soft delete)
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ===== AUDITORÍA =====

    /// <summary>
    /// ? Fecha de creación del registro (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ? Fecha de última actualización (UTC)
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// ? ID del usuario que creó el registro
    /// </summary>
    public int? CreatedBy { get; set; }

    /// <summary>
    /// ? ID del usuario que actualizó el registro
    /// </summary>
    public int? UpdatedBy { get; set; }

    // ===== NAVEGACIÓN (Entity Framework) =====

    /// <summary>
    /// Acciones de este tipo
    /// </summary>
    public List<ModuleAction> Actions { get; set; } = new();
}
