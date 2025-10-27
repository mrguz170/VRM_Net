using VRM_Plugin.Modules.Finanzas.Domain;

namespace VRM_Plugin.Modules.Finanzas.Services;

/// <summary>
/// Implementación del servicio de pagos.
/// </summary>
public class PagoService : IPagoService
{
    // TODO: Reemplazar con acceso a base de datos real
    private static readonly List<Pago> _pagos = new()
    {
        new Pago
        {
            Id = 1,
            FacturaId = 1,
            NumeroReferencia = "PAG-2025-001",
            FechaPago = DateTime.Now.AddDays(-8),
            Monto = 15080,
            MetodoPago = MetodoPago.Transferencia,
            NumeroTransaccion = "TRF123456",
            Concepto = "Pago factura FAC-2025-001",
            Estado = EstadoPago.Completado,
            RegistradoPor = "Sistema"
        },
        new Pago
        {
            Id = 2,
            FacturaId = 2,
            NumeroReferencia = "PAG-2025-002",
            FechaPago = DateTime.Now,
            Monto = 8700,
            MetodoPago = MetodoPago.TarjetaCredito,
            Concepto = "Pago factura FAC-2025-002",
            Estado = EstadoPago.Pendiente,
            RegistradoPor = "Sistema"
        }
    };

    public Task<List<Pago>> GetPagosAsync()
    {
        return Task.FromResult(_pagos);
    }

    public Task<Pago?> GetPagoByIdAsync(int id)
    {
        var pago = _pagos.FirstOrDefault(p => p.Id == id);
        return Task.FromResult(pago);
    }

    public Task<Pago> RegistrarPagoAsync(Pago pago)
    {
        pago.Id = _pagos.Any() ? _pagos.Max(p => p.Id) + 1 : 1;
        pago.FechaRegistro = DateTime.Now;
        _pagos.Add(pago);
        return Task.FromResult(pago);
    }

    public Task<List<Pago>> GetPagosByFacturaIdAsync(int facturaId)
    {
        var pagos = _pagos.Where(p => p.FacturaId == facturaId).ToList();
        return Task.FromResult(pagos);
    }

    public Task<List<Pago>> GetPagosPendientesAsync()
    {
        var pagos = _pagos.Where(p => p.Estado == EstadoPago.Pendiente).ToList();
        return Task.FromResult(pagos);
    }
}
