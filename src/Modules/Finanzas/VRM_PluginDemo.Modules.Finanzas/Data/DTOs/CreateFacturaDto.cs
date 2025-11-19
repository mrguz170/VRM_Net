namespace VRM_Plugin.Modules.Finanzas.Data.DTOs;

/// <summary>
/// DTO para crear una nueva factura
/// Contiene solo los campos necesarios para la creación (INSERT)
/// </summary>
public class CreateFacturaDto
{
    public string Folio { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; } = DateTime.Now;
    public DateTime? FechaVencimiento { get; set; }
    public string RfcCliente { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public string? NotasInternas { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
}

/// <summary>
/// DTO para actualizar una factura existente
/// </summary>
public class UpdateFacturaDto
{
    public int IdFactura { get; set; }
    public string? NotasInternas { get; set; }
    public int EstadoFactura { get; set; }
    public string ActualizadoPor { get; set; } = string.Empty;
}

/// <summary>
/// DTO para timbrar una factura en el SAT
/// </summary>
public class TimbrarFacturaDto
{
    public int IdFactura { get; set; }
    public string UuidSat { get; set; } = string.Empty;
    public DateTime FechaTimbrado { get; set; } = DateTime.Now;
    public string TimbradoPor { get; set; } = string.Empty;
}
