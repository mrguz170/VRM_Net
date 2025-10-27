namespace VRM_Plugin.Core.Domain;

/// <summary>
/// Configuración de negocio de un cliente.
/// Define qué módulos están habilitados y su configuración específica.
/// </summary>
public class ConfiguracionNegocio
{
    /// <summary>
    /// ID único del cliente
    /// </summary>
    public string ClienteId { get; set; } = string.Empty;

    /// <summary>
    /// Nombre o razón social del cliente
    /// </summary>
    public string NombreCliente { get; set; } = string.Empty;

    /// <summary>
    /// Módulos habilitados para este cliente
    /// </summary>
    public List<ModuloHabilitado> ModulosHabilitados { get; set; } = new();

    /// <summary>
    /// Parámetros globales de configuración del cliente
    /// Ejemplo: { "MaxUsuarios": 50, "AlmacenamientoGB": 100 }
    /// </summary>
    public Dictionary<string, object> ParametrosGlobales { get; set; } = new();

    /// <summary>
    /// Fecha de creación de la configuración
    /// </summary>
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Última actualización de la configuración
    /// </summary>
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si el cliente está activo
    /// </summary>
    public bool EstaActivo { get; set; } = true;

    // ==================== MÉTODOS ÚTILES ====================

    /// <summary>
    /// Verifica si un módulo específico está habilitado para este cliente
    /// </summary>
    public bool TieneModuloHabilitado(string moduloId)
    {
        return ModulosHabilitados.Any(m =>
            m.ModuloId == moduloId &&
            m.Habilitado &&
            EstaActivo);
    }

    /// <summary>
    /// Obtiene la configuración específica de un módulo
    /// </summary>
    public Dictionary<string, object>? ObtenerConfiguracionModulo(string moduloId)
    {
        return ModulosHabilitados
            .FirstOrDefault(m => m.ModuloId == moduloId)
            ?.ConfiguracionEspecifica;
    }
}