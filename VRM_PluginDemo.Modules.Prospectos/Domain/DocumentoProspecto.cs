namespace VRM_PluginDemo.Modules.Prospectos.Domain;

/// <summary>
/// Representa un documento adjunto por un prospecto
/// </summary>
public class DocumentoProspecto
{
    /// <summary>
    /// ID único del documento
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Tipo de documento
    /// </summary>
    public TipoDocumento TipoDocumento { get; set; }

    /// <summary>
    /// Nombre del archivo
    /// </summary>
    public string NombreArchivo { get; set; } = string.Empty;

    /// <summary>
    /// Ruta del archivo en MinIO o sistema de almacenamiento
    /// </summary>
    public string RutaAlmacenamiento { get; set; } = string.Empty;

    /// <summary>
    /// Tamaño del archivo en bytes
    /// </summary>
    public long TamañoBytes { get; set; }

    /// <summary>
    /// Tipo MIME del archivo (ej: "application/pdf")
    /// </summary>
    public string TipoMIME { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de carga del documento
    /// </summary>
    public DateTime FechaCarga { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Usuario que cargó el documento
    /// </summary>
    public string CargadoPor { get; set; } = string.Empty;

    /// <summary>
    /// Indica si el documento fue validado
    /// </summary>
    public bool Validado { get; set; }

    /// <summary>
    /// Comentarios sobre el documento
    /// </summary>
    public string? Comentarios { get; set; }
}