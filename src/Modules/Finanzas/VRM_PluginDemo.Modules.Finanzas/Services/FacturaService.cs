using VRM_PluginDemo.Modules.Finanzas.Domain;

namespace VRM_PluginDemo.Modules.Finanzas.Services;

/// <summary>
/// Implementación del servicio de facturas.
/// </summary>
public class FacturaService : IFacturaService
{
    // TODO: Reemplazar con acceso a base de datos real
    private static readonly List<Factura> _facturas = new()
    {
        new Factura
        {
            Id = 1,
            Numero = "FAC-2025-001",
            FechaEmision = DateTime.Now.AddDays(-10),
            FechaVencimiento = DateTime.Now.AddDays(20),
            RfcCliente = "ABC123456789",
            NombreCliente = "Acme Corporation",
            Subtotal = 13000,
            Impuestos = 2080,
            Total = 15080,
            Estado = EstadoFactura.Pagada,
            CreadoPor = "Sistema"
        },
        new Factura
        {
            Id = 2,
            Numero = "FAC-2025-002",
            FechaEmision = DateTime.Now.AddDays(-5),
            FechaVencimiento = DateTime.Now.AddDays(25),
            RfcCliente = "XYZ987654321",
            NombreCliente = "Tech Solutions SA",
            Subtotal = 7500,
            Impuestos = 1200,
            Total = 8700,
            Estado = EstadoFactura.Pendiente,
            CreadoPor = "Sistema"
        },
        new Factura
        {
            Id = 3,
            Numero = "FAC-2025-003",
            FechaEmision = DateTime.Now.AddDays(-15),
            FechaVencimiento = DateTime.Now.AddDays(-5),
            RfcCliente = "DEF456789123",
            NombreCliente = "Innovate Inc",
            Subtotal = 19000,
            Impuestos = 3040,
            Total = 22040,
            Estado = EstadoFactura.Vencida,
            CreadoPor = "Sistema"
        }
    };

    public Task<List<Factura>> GetFacturasAsync()
    {
        return Task.FromResult(_facturas);
    }

    public Task<Factura?> GetFacturaByIdAsync(int id)
    {
        var factura = _facturas.FirstOrDefault(f => f.Id == id);
        return Task.FromResult(factura);
    }

    public Task<Factura> CreateFacturaAsync(Factura factura)
    {
        factura.Id = _facturas.Any() ? _facturas.Max(f => f.Id) + 1 : 1;
        factura.FechaCreacion = DateTime.Now;
        _facturas.Add(factura);
        return Task.FromResult(factura);
    }

    public Task<Factura> UpdateFacturaAsync(Factura factura)
    {
        var existente = _facturas.FirstOrDefault(f => f.Id == factura.Id);
        if (existente != null)
        {
            var index = _facturas.IndexOf(existente);
            _facturas[index] = factura;
        }
        return Task.FromResult(factura);
    }

    public Task<bool> CancelarFacturaAsync(int id, string motivo)
    {
        var factura = _facturas.FirstOrDefault(f => f.Id == id);
        if (factura != null)
        {
            factura.Estado = EstadoFactura.Cancelada;
            factura.NotasInternas = $"Cancelada: {motivo}";
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<List<Factura>> GetFacturasByEstadoAsync(EstadoFactura estado)
    {
        var facturas = _facturas.Where(f => f.Estado == estado).ToList();
        return Task.FromResult(facturas);
    }

    public Task<decimal> GetTotalIngresosAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var total = _facturas
            .Where(f => f.FechaEmision >= fechaInicio && f.FechaEmision <= fechaFin)
            .Where(f => f.Estado == EstadoFactura.Pagada)
            .Sum(f => f.Total);
        
        return Task.FromResult(total);
    }
}
