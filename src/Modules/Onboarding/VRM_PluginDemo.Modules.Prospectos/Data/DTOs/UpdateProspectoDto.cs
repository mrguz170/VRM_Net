namespace VRM_Plugin.Modules.Prospectos.Data.DTOs;

/// <summary>
/// DTO para actualizar un prospecto existente
/// ?? Solo campos que se pueden actualizar
/// ? EJEMPLO SIMPLIFICADO PARA DEMOSTRACIÓN
/// </summary>
public class UpdateProspectoDto
{
    /// <summary>
    /// ID del prospecto a actualizar
    /// </summary>
    public int IdProspecto { get; set; }
    
    /// <summary>
    /// Correo electrónico actualizado
    /// </summary>
    public string? CorreoElectronico { get; set; }
    
    /// <summary>
    /// Estado del prospecto actualizado
    /// 1=EnRevision, 2=Aprobado, 3=Rechazado
    /// </summary>
    public int? EstadoProspecto { get; set; }
    
    /// <summary>
    /// Usuario que actualiza el registro
    /// </summary>
    public string ActualizadoPor { get; set; } = string.Empty;
}
