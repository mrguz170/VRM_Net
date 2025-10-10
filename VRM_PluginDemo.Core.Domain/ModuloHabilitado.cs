namespace VRM_PluginDemo.Core.Domain;

/// <summary>
/// Representa un módulo habilitado para un cliente específico
/// </summary>
public class ModuloHabilitado
{
    /// <summary>
    /// ID del módulo (debe coincidir con IModule.ModuleId)
    /// Ejemplo: "Prospectos", "Facturas", "Expedientes"
    /// </summary>
    public string ModuloId { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el módulo está actualmente habilitado
    /// </summary>
    public bool Habilitado { get; set; } = true;

    /// <summary>
    /// Fecha en que se activó el módulo para este cliente
    /// </summary>
    public DateTime? FechaActivacion { get; set; }

    /// <summary>
    /// Fecha de expiración de la licencia del módulo (si aplica)
    /// </summary>
    public DateTime? FechaExpiracion { get; set; }

    /// <summary>
    /// Configuración específica del módulo para este cliente.
    /// Cada módulo puede definir sus propios parámetros.
    /// Ejemplo para "Prospectos": { "RequiereAprobacionLegal": true, "DiasRevision": 5 }
    /// </summary>
    public Dictionary<string, object> ConfiguracionEspecifica { get; set; } = new();

    /// <summary>
    /// Orden de visualización en menús/dashboards
    /// </summary>
    public int OrdenVisualizacion { get; set; }

    // ==================== MÉTODOS ÚTILES ====================

    /// <summary>
    /// Verifica si el módulo está activo y no ha expirado
    /// </summary>
    public bool EstaDisponible()
    {
        if (!Habilitado)
            return false;

        if (FechaExpiracion.HasValue && FechaExpiracion.Value < DateTime.UtcNow)
            return false;

        return true;
    }
}