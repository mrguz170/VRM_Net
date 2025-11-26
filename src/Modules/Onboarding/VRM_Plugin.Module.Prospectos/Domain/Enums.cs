namespace VRM_Plugin.Modules.Prospectos.Domain;

/// <summary>
/// Estados posibles de un prospecto
/// </summary>
public enum EstadoProspecto
{
    /// <summary>
    /// Solicitud recibida, en proceso de revisión
    /// </summary>
    EnRevision,

    /// <summary>
    /// Aprobado por todas las áreas, listo para convertirse en proveedor
    /// </summary>
    Aprobado,

    /// <summary>
    /// Rechazado por una o más áreas
    /// </summary>
    Rechazado,

    /// <summary>
    /// Requiere documentación adicional
    /// </summary>
    PendienteDocumentacion,

    /// <summary>
    /// Convertido exitosamente a proveedor
    /// </summary>
    ConvertidoProveedor
}

/// <summary>
/// Estados de revisión por área
/// </summary>
public enum EstadoRevision
{
    /// <summary>
    /// Pendiente de revisión
    /// </summary>
    Pendiente,

    /// <summary>
    /// Aprobado por el área
    /// </summary>
    Aprobado,

    /// <summary>
    /// Rechazado por el área
    /// </summary>
    Rechazado,

    /// <summary>
    /// Requiere aclaración o más información
    /// </summary>
    RequiereAclaracion
}

/// <summary>
/// Tipos de documentos que puede adjuntar un prospecto
/// </summary>
public enum TipoDocumento
{
    ActaConstitutiva,
    ComprobantedomicilioFiscal,
    ConstanciaSituacionFiscal,
    IdentificacionRepresentanteLegal,
    EstadosCuentaBancarios,
    OpinionCumplimientoSAT,
    Otro
}