namespace VRM_Plugin.Modules.Prospectos.Domain;

/// <summary>
/// Representa la revisión de un prospecto por un área específica
/// </summary>
public class RevisionArea
{
    /// <summary>
    /// ID único de la revisión
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Nombre del área que realiza la revisión
    /// Ejemplos: "Legal", "Fiscal", "Comercial", "Crédito", "Operaciones"
    /// </summary>
    public string Area { get; set; } = string.Empty;

    /// <summary>
    /// Estado actual de la revisión
    /// </summary>
    public EstadoRevision Estado { get; set; } = EstadoRevision.Pendiente;

    /// <summary>
    /// Comentarios del revisor
    /// </summary>
    public string Comentarios { get; set; } = string.Empty;

    /// <summary>
    /// Usuario que realizó la revisión
    /// </summary>
    public string RevisadoPor { get; set; } = string.Empty;

    /// <summary>
    /// Fecha en que se realizó la revisión
    /// </summary>
    public DateTime? FechaRevision { get; set; }

    /// <summary>
    /// Calificación numérica (opcional, 1-10)
    /// </summary>
    public int? Calificacion { get; set; }

    /// <summary>
    /// Documentos específicos requeridos por esta área
    /// </summary>
    public List<string> DocumentosRequeridos { get; set; } = new();
}