using VRM_Plugin.Modules.Finanzas.Domain;
using VRM_Plugin.Modules.Finanzas.Data.DTOs;
using VRM_Plugin.Modules.Finanzas.Data.Repositories;

namespace VRM_Plugin.Modules.Finanzas.Services;

/// <summary>
/// Servicio de lógica de negocio para Facturas
/// ? Usa IFacturaRepository para acceso a datos
/// ? Convierte entre DTOs (BD) y Domain (negocio)
/// ? Aplica validaciones y lógica de negocio
/// </summary>
public class FacturaService : IFacturaService
{
    private readonly IFacturaRepository _repository;
    
    public FacturaService(IFacturaRepository repository)
    {
        _repository = repository;
    }
    
    /// <summary>
    /// Obtiene todas las facturas
    /// Convierte FacturaDto (BD) ? Factura (Domain)
    /// </summary>
    public async Task<List<Factura>> GetFacturasAsync()
    {
        // 1. Obtener DTOs desde BD
        var dtos = await _repository.GetAllAsync();
        
        // 2. Convertir DTO ? Domain
        return dtos.Select(MapDtoToDomain).ToList();
    }
    
    public async Task<Factura?> GetFacturaByIdAsync(int id)
    {
        var dto = await _repository.GetByIdAsync(id);
        return dto != null ? MapDtoToDomain(dto) : null;
    }
    
    public async Task<List<Factura>> GetFacturasByEstadoAsync(EstadoFactura estado)
    {
        var dtos = await _repository.GetByEstadoAsync((int)estado);
        return dtos.Select(MapDtoToDomain).ToList();
    }
    
    /// <summary>
    /// Crea una nueva factura
    /// Aplica validaciones y lógica de negocio antes de guardar
    /// </summary>
    public async Task<Factura> CreateFacturaAsync(Factura factura)
    {
        // ? Validar (lógica de negocio)
        ValidarFactura(factura);
        
        // ? Calcular totales (lógica de negocio)
        factura.Total = factura.Subtotal + factura.Impuestos;
        
        // ? Convertir Domain ? DTO
        var dto = new CreateFacturaDto
        {
            Folio = factura.Numero,
            FechaEmision = factura.FechaEmision,
            FechaVencimiento = factura.FechaVencimiento,
            RfcCliente = factura.RfcCliente,
            NombreCliente = factura.NombreCliente,
            Subtotal = factura.Subtotal,
            Impuestos = factura.Impuestos,
            Total = factura.Total,
            NotasInternas = factura.NotasInternas,
            CreadoPor = factura.CreadoPor
        };
        
        // ? Guardar en BD
        var idGenerado = await _repository.CreateAsync(dto);
        factura.Id = idGenerado;
        
        return factura;
    }
    
    public async Task<Factura> UpdateFacturaAsync(Factura factura)
    {
        ValidarFactura(factura);
        
        var dto = new UpdateFacturaDto
        {
            IdFactura = factura.Id,
            NotasInternas = factura.NotasInternas,
            EstadoFactura = (int)factura.Estado,
            ActualizadoPor = factura.CreadoPor  // TODO: Pasar usuario actual
        };
        
        var exito = await _repository.UpdateAsync(dto);
        
        if (!exito)
            throw new InvalidOperationException($"No se pudo actualizar la factura {factura.Id}");
        
        return factura;
    }
    
    public async Task<bool> CancelarFacturaAsync(int id, string motivo)
    {
        // TODO: Validar que la factura pueda cancelarse
        return await _repository.CancelarAsync(id, motivo, "Sistema");  // TODO: Usuario actual
    }
    
    public async Task<decimal> GetTotalIngresosAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _repository.GetTotalIngresosAsync(fechaInicio, fechaFin);
    }
    
    // ==================== MÉTODOS PRIVADOS ====================
    
    /// <summary>
    /// ? Convierte DTO (BD) ? Domain (negocio)
    /// </summary>
    private Factura MapDtoToDomain(FacturaDto dto)
    {
        return new Factura
        {
            Id = dto.IdFactura,
            Numero = dto.Folio,
            FechaEmision = dto.FechaEmision,
            FechaVencimiento = dto.FechaVencimiento,
            RfcCliente = dto.RfcCliente,
            NombreCliente = dto.NombreCliente,
            Subtotal = dto.Subtotal,
            Impuestos = dto.Impuestos,
            Total = dto.Total,
            Estado = (EstadoFactura)dto.EstadoFactura,  // ? int ? Enum
            NotasInternas = dto.NotasInternas,
            FechaCreacion = dto.FechaCreacion,
            CreadoPor = dto.CreadoPor
        };
    }
    
    /// <summary>
    /// ? Validaciones de negocio
    /// </summary>
    private void ValidarFactura(Factura factura)
    {
        if (string.IsNullOrWhiteSpace(factura.Numero))
            throw new ArgumentException("El número de factura es requerido");
        
        if (string.IsNullOrWhiteSpace(factura.RfcCliente))
            throw new ArgumentException("El RFC del cliente es requerido");
        
        if (factura.RfcCliente.Length < 12 || factura.RfcCliente.Length > 13)
            throw new ArgumentException("RFC inválido");
        
        if (factura.Subtotal < 0)
            throw new ArgumentException("El subtotal no puede ser negativo");
        
        if (factura.FechaVencimiento.HasValue && 
            factura.FechaVencimiento.Value < factura.FechaEmision)
            throw new ArgumentException("La fecha de vencimiento no puede ser anterior a la fecha de emisión");
    }
}
