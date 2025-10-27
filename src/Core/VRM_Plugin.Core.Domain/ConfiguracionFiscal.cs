namespace VRM_Plugin.Core.Domain;

/// <summary>
/// Configuración fiscal de un cliente para servicios del SAT
/// </summary>
public class ConfiguracionFiscal
{
    /// <summary>
    /// ID único del cliente (debe coincidir con ConfiguracionNegocio.ClienteId)
    /// </summary>
    public string ClienteId { get; set; } = string.Empty;

    // ==================== DATOS FISCALES ====================

    /// <summary>
    /// RFC del cliente
    /// </summary>
    public string RFC { get; set; } = string.Empty;

    /// <summary>
    /// Razón social registrada ante el SAT
    /// </summary>
    public string RazonSocial { get; set; } = string.Empty;

    /// <summary>
    /// Régimen fiscal (ej: "601 - General de Ley Personas Morales")
    /// </summary>
    public string RegimenFiscal { get; set; } = string.Empty;

    /// <summary>
    /// Código postal fiscal
    /// </summary>
    public string CodigoPostalFiscal { get; set; } = string.Empty;

    // ==================== CERTIFICADOS SAT ====================

    /// <summary>
    /// Certificado digital (.cer) en Base64
    /// </summary>
    public string CertificadoSAT { get; set; } = string.Empty;

    /// <summary>
    /// Llave privada (.key) en Base64 (ENCRIPTADA)
    /// </summary>
    public string LlavePrivada { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña de la llave privada (ENCRIPTADA)
    /// </summary>
    public string PasswordLlave { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de vigencia del certificado
    /// </summary>
    public DateTime FechaVigenciaCertificado { get; set; }

    // ==================== PAC (Proveedor Autorizado de Certificación) ====================

    /// <summary>
    /// Proveedor de timbrado (ej: "Finkok", "SW Sapien")
    /// </summary>
    public string ProveedorPAC { get; set; } = string.Empty;

    /// <summary>
    /// Usuario del PAC
    /// </summary>
    public string UsuarioPAC { get; set; } = string.Empty;

    /// <summary>
    /// Password del PAC (ENCRIPTADO)
    /// </summary>
    public string PasswordPAC { get; set; } = string.Empty;

    /// <summary>
    /// URL del servicio de timbrado
    /// </summary>
    public string UrlServicioPAC { get; set; } = string.Empty;

    // ==================== METADATOS ====================

    /// <summary>
    /// Fecha de creación de la configuración
    /// </summary>
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Última actualización
    /// </summary>
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si la configuración fiscal está completa y válida
    /// </summary>
    public bool EstaConfigurada { get; set; }

    // ==================== MÉTODOS ÚTILES ====================

    /// <summary>
    /// Verifica si el certificado SAT está vigente
    /// </summary>
    public bool CertificadoVigente()
    {
        return FechaVigenciaCertificado > DateTime.UtcNow;
    }

    /// <summary>
    /// Valida que la configuración fiscal esté completa
    /// </summary>
    public bool EsValida()
    {
        return !string.IsNullOrEmpty(RFC) &&
               !string.IsNullOrEmpty(RazonSocial) &&
               !string.IsNullOrEmpty(CertificadoSAT) &&
               !string.IsNullOrEmpty(LlavePrivada) &&
               CertificadoVigente();
    }
}