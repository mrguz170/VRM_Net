namespace VRM_Plugin.Modules.Prospectos.Domain;

/// <summary>
/// Representa una empresa que solicita ser proveedor
/// </summary>
public class Prospecto
{
    /// <summary>
    /// ID único del prospecto
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    // ==================== INFORMACIÓN BÁSICA ====================

    /// <summary>
    /// Razón social de la empresa
    /// </summary>
    public string RazonSocial { get; set; } = string.Empty;

    /// <summary>
    /// RFC de la empresa
    /// </summary>
    public string RFC { get; set; } = string.Empty;

    /// <summary>
    /// Nombre comercial (si es diferente a razón social)
    /// </summary>
    public string? NombreComercial { get; set; }

    /// <summary>
    /// Giro o actividad principal
    /// </summary>
    public string GiroEmpresarial { get; set; } = string.Empty;

    // ==================== CONTACTO ====================

    /// <summary>
    /// Correo electrónico principal
    /// </summary>
    public string CorreoElectronico { get; set; } = string.Empty;

    /// <summary>
    /// Teléfono de contacto
    /// </summary>
    public string Telefono { get; set; } = string.Empty;

    /// <summary>
    /// Sitio web (opcional)
    /// </summary>
    public string? SitioWeb { get; set; }

    // ==================== REPRESENTANTE LEGAL ====================

    /// <summary>
    /// Nombre completo del representante legal
    /// </summary>
    public string RepresentanteLegal { get; set; } = string.Empty;

    /// <summary>
    /// Correo del representante
    /// </summary>
    public string CorreoRepresentante { get; set; } = string.Empty;

    // ==================== ESTADO Y FECHAS ====================

    /// <summary>
    /// Estado actual del prospecto
    /// </summary>
    public EstadoProspecto Estado { get; set; } = EstadoProspecto.EnRevision;

    /// <summary>
    /// Fecha en que se recibió la solicitud
    /// </summary>
    public DateTime FechaSolicitud { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de última actualización
    /// </summary>
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha en que se aprobó o rechazó
    /// </summary>
    public DateTime? FechaResolucion { get; set; }

    // ==================== REVISIONES Y DOCUMENTOS ====================

    /// <summary>
    /// Revisiones realizadas por diferentes áreas
    /// </summary>
    public List<RevisionArea> Revisiones { get; set; } = new();

    /// <summary>
    /// Documentos adjuntos
    /// </summary>
    public List<DocumentoProspecto> Documentos { get; set; } = new();

    // ==================== METADATOS ====================

    /// <summary>
    /// Usuario que creó el prospecto
    /// </summary>
    public string CreadoPor { get; set; } = string.Empty;

    /// <summary>
    /// Notas adicionales
    /// </summary>
    public string? NotasAdicionales { get; set; }

    // ==================== MÉTODOS ÚTILES ====================

    /// <summary>
    /// Verifica si todas las áreas han dado su visto bueno
    /// </summary>
    public bool TodasLasAreasAprobaron()
    {
        return Revisiones.Any() &&
               Revisiones.All(r => r.Estado == EstadoRevision.Aprobado);
    }

    /// <summary>
    /// Verifica si algún área rechazó
    /// </summary>
    public bool FueRechazadoPorAlgunaArea()
    {
        return Revisiones.Any(r => r.Estado == EstadoRevision.Rechazado);
    }

    /// <summary>
    /// Obtiene las áreas que faltan por revisar
    /// </summary>
    public List<RevisionArea> AreasPendientes()
    {
        return Revisiones
            .Where(r => r.Estado == EstadoRevision.Pendiente)
            .ToList();
    }
}