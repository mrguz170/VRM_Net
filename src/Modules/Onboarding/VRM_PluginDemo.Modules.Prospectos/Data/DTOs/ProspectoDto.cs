namespace VRM_Plugin.Modules.Prospectos.Data.DTOs;

/// <summary>
/// DTO que representa un prospecto desde la base de datos
/// ?? Mapeo automático: snake_case (BD) ? PascalCase (C#)
/// ? EJEMPLO SIMPLIFICADO PARA DEMOSTRACIÓN
/// </summary>
public class ProspectoDto
{
    /// <summary>
    /// ID único del prospecto (BD: id_prospecto)
    /// </summary>
    public int IdProspecto { get; set; }
    
    /// <summary>
    /// Razón social de la empresa (BD: razon_social)
    /// </summary>
    public string RazonSocial { get; set; } = string.Empty;
    
    /// <summary>
    /// RFC de la empresa (BD: rfc)
    /// </summary>
    public string RFC { get; set; } = string.Empty;
    
    /// <summary>
    /// Correo electrónico (BD: correo_electronico)
    /// </summary>
    public string CorreoElectronico { get; set; } = string.Empty;
    
    /// <summary>
    /// Estado del prospecto (BD: estado_prospecto)
    /// 1=EnRevision, 2=Aprobado, 3=Rechazado
    /// </summary>
    public int EstadoProspecto { get; set; }
    
    /// <summary>
    /// Fecha de creación (BD: fecha_creacion)
    /// </summary>
    public DateTime FechaCreacion { get; set; }
}
