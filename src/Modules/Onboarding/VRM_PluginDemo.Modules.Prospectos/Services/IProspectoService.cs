using VRM_PluginDemo.Modules.Prospectos.Domain;

namespace VRM_PluginDemo.Modules.Prospectos.Services;

/// <summary>
/// Contrato de servicio para gestión de prospectos
/// </summary>
public interface IProspectoService
{
    // ==================== CONSULTAS ====================

    /// <summary>
    /// Obtiene todos los prospectos
    /// </summary>
    Task<List<Prospecto>> ObtenerTodosAsync();

    /// <summary>
    /// Obtiene un prospecto por ID
    /// </summary>
    Task<Prospecto?> ObtenerPorIdAsync(Guid id);

    /// <summary>
    /// Obtiene prospectos por estado
    /// </summary>
    Task<List<Prospecto>> ObtenerPorEstadoAsync(EstadoProspecto estado);

    /// <summary>
    /// Obtiene prospectos pendientes de revisión por un área específica
    /// </summary>
    Task<List<Prospecto>> ObtenerPendientesPorAreaAsync(string area);

    // ==================== COMANDOS ====================

    /// <summary>
    /// Crea un nuevo prospecto
    /// </summary>
    Task<Prospecto> CrearProspectoAsync(Prospecto prospecto);

    /// <summary>
    /// Actualiza un prospecto existente
    /// </summary>
    Task<Prospecto> ActualizarProspectoAsync(Prospecto prospecto);

    /// <summary>
    /// Elimina un prospecto
    /// </summary>
    Task<bool> EliminarProspectoAsync(Guid id);

    // ==================== REVISIONES ====================

    /// <summary>
    /// Agrega una revisión de área al prospecto
    /// </summary>
    Task<bool> AgregarRevisionAsync(Guid prospectoId, RevisionArea revision);

    /// <summary>
    /// Actualiza el estado de una revisión
    /// </summary>
    Task<bool> ActualizarRevisionAsync(Guid prospectoId, Guid revisionId, EstadoRevision nuevoEstado, string comentarios);

    // ==================== DOCUMENTOS ====================

    /// <summary>
    /// Agrega un documento al prospecto
    /// </summary>
    Task<bool> AgregarDocumentoAsync(Guid prospectoId, DocumentoProspecto documento);

    /// <summary>
    /// Valida un documento
    /// </summary>
    Task<bool> ValidarDocumentoAsync(Guid prospectoId, Guid documentoId, bool esValido, string? comentarios);

    // ==================== FLUJO DE APROBACIÓN ====================

    /// <summary>
    /// Verifica el estado general del prospecto y actualiza si todas las áreas aprobaron
    /// </summary>
    Task<bool> VerificarYActualizarEstadoAsync(Guid prospectoId);

    /// <summary>
    /// Convierte un prospecto aprobado en proveedor
    /// </summary>
    Task<bool> ConvertirAProveedorAsync(Guid prospectoId);
}