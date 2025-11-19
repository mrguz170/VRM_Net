namespace VRM_Plugin.Modules.Prospectos.Data.DTOs;

/// <summary>
/// DTO para crear un nuevo prospecto
/// ?? Solo campos necesarios para INSERT
/// ? EJEMPLO SIMPLIFICADO PARA DEMOSTRACIÓN
/// </summary>
public class CreateProspectoDto
{
    /// <summary>
    /// Razón social de la empresa
    /// </summary>
    public string RazonSocial { get; set; } = string.Empty;
    
    /// <summary>
    /// RFC de la empresa
    /// </summary>
    public string RFC { get; set; } = string.Empty;
    
    /// <summary>
    /// Correo electrónico principal
    /// </summary>
    public string CorreoElectronico { get; set; } = string.Empty;
    
    /// <summary>
    /// Usuario que crea el prospecto
    /// </summary>
    public string CreadoPor { get; set; } = string.Empty;
}
