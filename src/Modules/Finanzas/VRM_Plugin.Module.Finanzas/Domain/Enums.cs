namespace VRM_Plugin.Modules.Finanzas.Domain;

/// <summary>
/// Métodos de pago disponibles.
/// </summary>
public enum MetodoPago
{
    Efectivo = 1,
    Transferencia = 2,
    TarjetaCredito = 3,
    TarjetaDebito = 4,
    Cheque = 5,
    Otro = 99
}

/// <summary>
/// Estados de un pago.
/// </summary>
public enum EstadoPago
{
    Pendiente = 0,
    Procesando = 1,
    Completado = 2,
    Rechazado = 3,
    Cancelado = 4
}
