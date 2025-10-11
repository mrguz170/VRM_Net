using VRM_PluginDemo.Modules.Prospectos.Domain;

namespace VRM_PluginDemo.Modules.Prospectos.Services;

/// <summary>
/// Implementación del servicio de gestión de prospectos
/// NOTA: En producción, esto usaría un repositorio con base de datos real.
/// Por ahora usamos almacenamiento en memoria para la demo.
/// </summary>
public class ProspectoService : IProspectoService
{
    // Almacenamiento en memoria (temporal para demo)
    private static readonly List<Prospecto> _prospectos = new();

    // ==================== CONSULTAS ====================

    public Task<List<Prospecto>> ObtenerTodosAsync()
    {
        return Task.FromResult(_prospectos.ToList());
    }

    public Task<Prospecto?> ObtenerPorIdAsync(Guid id)
    {
        var prospecto = _prospectos.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(prospecto);
    }

    public Task<List<Prospecto>> ObtenerPorEstadoAsync(EstadoProspecto estado)
    {
        var prospectos = _prospectos
            .Where(p => p.Estado == estado)
            .ToList();
        return Task.FromResult(prospectos);
    }

    public Task<List<Prospecto>> ObtenerPendientesPorAreaAsync(string area)
    {
        var prospectos = _prospectos
            .Where(p => p.Revisiones.Any(r =>
                r.Area.Equals(area, StringComparison.OrdinalIgnoreCase) &&
                r.Estado == EstadoRevision.Pendiente))
            .ToList();
        return Task.FromResult(prospectos);
    }

    // ==================== COMANDOS ====================

    public Task<Prospecto> CrearProspectoAsync(Prospecto prospecto)
    {
        prospecto.Id = Guid.NewGuid();
        prospecto.FechaSolicitud = DateTime.UtcNow;
        prospecto.FechaActualizacion = DateTime.UtcNow;
        prospecto.Estado = EstadoProspecto.EnRevision;

        _prospectos.Add(prospecto);
        return Task.FromResult(prospecto);
    }

    public Task<Prospecto> ActualizarProspectoAsync(Prospecto prospecto)
    {
        var existente = _prospectos.FirstOrDefault(p => p.Id == prospecto.Id);
        if (existente != null)
        {
            _prospectos.Remove(existente);
            prospecto.FechaActualizacion = DateTime.UtcNow;
            _prospectos.Add(prospecto);
        }
        return Task.FromResult(prospecto);
    }

    public Task<bool> EliminarProspectoAsync(Guid id)
    {
        var prospecto = _prospectos.FirstOrDefault(p => p.Id == id);
        if (prospecto != null)
        {
            _prospectos.Remove(prospecto);
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    // ==================== REVISIONES ====================

    public Task<bool> AgregarRevisionAsync(Guid prospectoId, RevisionArea revision)
    {
        var prospecto = _prospectos.FirstOrDefault(p => p.Id == prospectoId);
        if (prospecto == null)
            return Task.FromResult(false);

        revision.Id = Guid.NewGuid();
        prospecto.Revisiones.Add(revision);
        prospecto.FechaActualizacion = DateTime.UtcNow;

        return Task.FromResult(true);
    }

    public Task<bool> ActualizarRevisionAsync(Guid prospectoId, Guid revisionId, EstadoRevision nuevoEstado, string comentarios)
    {
        var prospecto = _prospectos.FirstOrDefault(p => p.Id == prospectoId);
        if (prospecto == null)
            return Task.FromResult(false);

        var revision = prospecto.Revisiones.FirstOrDefault(r => r.Id == revisionId);
        if (revision == null)
            return Task.FromResult(false);

        revision.Estado = nuevoEstado;
        revision.Comentarios = comentarios;
        revision.FechaRevision = DateTime.UtcNow;
        prospecto.FechaActualizacion = DateTime.UtcNow;

        // Verificar si todas las áreas aprobaron o si alguna rechazó
        _ = VerificarYActualizarEstadoAsync(prospectoId);

        return Task.FromResult(true);
    }

    // ==================== DOCUMENTOS ====================

    public Task<bool> AgregarDocumentoAsync(Guid prospectoId, DocumentoProspecto documento)
    {
        var prospecto = _prospectos.FirstOrDefault(p => p.Id == prospectoId);
        if (prospecto == null)
            return Task.FromResult(false);

        documento.Id = Guid.NewGuid();
        documento.FechaCarga = DateTime.UtcNow;
        prospecto.Documentos.Add(documento);
        prospecto.FechaActualizacion = DateTime.UtcNow;

        return Task.FromResult(true);
    }

    public Task<bool> ValidarDocumentoAsync(Guid prospectoId, Guid documentoId, bool esValido, string? comentarios)
    {
        var prospecto = _prospectos.FirstOrDefault(p => p.Id == prospectoId);
        if (prospecto == null)
            return Task.FromResult(false);

        var documento = prospecto.Documentos.FirstOrDefault(d => d.Id == documentoId);
        if (documento == null)
            return Task.FromResult(false);

        documento.Validado = esValido;
        documento.Comentarios = comentarios;
        prospecto.FechaActualizacion = DateTime.UtcNow;

        return Task.FromResult(true);
    }

    // ==================== FLUJO DE APROBACIÓN ====================

    public Task<bool> VerificarYActualizarEstadoAsync(Guid prospectoId)
    {
        var prospecto = _prospectos.FirstOrDefault(p => p.Id == prospectoId);
        if (prospecto == null)
            return Task.FromResult(false);

        // Si algún área rechazó, marcar como rechazado
        if (prospecto.FueRechazadoPorAlgunaArea())
        {
            prospecto.Estado = EstadoProspecto.Rechazado;
            prospecto.FechaResolucion = DateTime.UtcNow;
        }
        // Si todas las áreas aprobaron, marcar como aprobado
        else if (prospecto.TodasLasAreasAprobaron())
        {
            prospecto.Estado = EstadoProspecto.Aprobado;
            prospecto.FechaResolucion = DateTime.UtcNow;
        }
        // Si hay áreas que requieren aclaración, marcar como pendiente documentación
        else if (prospecto.Revisiones.Any(r => r.Estado == EstadoRevision.RequiereAclaracion))
        {
            prospecto.Estado = EstadoProspecto.PendienteDocumentacion;
        }

        prospecto.FechaActualizacion = DateTime.UtcNow;
        return Task.FromResult(true);
    }

    public Task<bool> ConvertirAProveedorAsync(Guid prospectoId)
    {
        var prospecto = _prospectos.FirstOrDefault(p => p.Id == prospectoId);
        if (prospecto == null || prospecto.Estado != EstadoProspecto.Aprobado)
            return Task.FromResult(false);

        prospecto.Estado = EstadoProspecto.ConvertidoProveedor;
        prospecto.FechaActualizacion = DateTime.UtcNow;

        // TODO: Aquí se crearía el proveedor en el módulo correspondiente
        // Por ahora solo cambiamos el estado

        return Task.FromResult(true);
    }
}