# Módulo Finanzas - Estructura de Datos

Este documento explica la arquitectura de acceso a datos del módulo de Finanzas.

---

## ?? Estructura de Carpetas

```
VRM_PluginDemo.Modules.Finanzas/
?
??? ?? Domain/                          ? Entidades de negocio (RICA)
?   ??? Factura.cs                      ? Lógica de negocio + validaciones
?   ??? ConceptoFactura.cs
?   ??? Pago.cs
?   ??? Enums.cs                        ? EstadoFactura, MetodoPago, EstadoPago
?
??? ?? DTOs/                            ? ? Transferencia de datos (PLANA)
?   ??? FacturaDto.cs                   ? Mapea tabla BD (snake_case)
?   ??? PagoDto.cs
?   ??? CreateFacturaDto.cs             ? Para INSERT
?   ??? CreatePagoDto.cs
?
??? ?? Data/                            ? ? Acceso a datos
?   ??? ?? Repositories/
?       ??? IFacturaRepository.cs       ? Contrato
?       ??? FacturaRepository.cs        ? Usa DatabaseHelper
?       ??? IPagoRepository.cs
?       ??? PagoRepository.cs
?
??? ?? Services/                        ? Lógica de negocio
?   ??? IFacturaService.cs
?   ??? FacturaService.cs               ? Usa Repository + convierte DTO ? Domain
?   ??? IPagoService.cs
?   ??? PagoService.cs
?
??? FinanzasModule.cs                   ? Registro de servicios
```

---

## ?? Flujo de Datos

```
????????????????????????????????????????????????????????????
?                    FLUJO COMPLETO                        ?
????????????????????????????????????????????????????????????

1?? BD (MySQL) - Tabla: facturas
   ? Columnas snake_case: id_factura, fecha_emision, estado_factura
   
2?? Stored Procedure
   CALL sp_Finanzas_Facturas_GetAll()
   ?
   
3?? DatabaseHelper (Mapeo automático)
   - id_factura ? IdFactura
   - fecha_emision ? FechaEmision
   - estado_factura ? EstadoFactura
   ?
   
4?? FacturaDto (Repository)
   {
     IdFactura = 1,
     FechaEmision = "2024-01-15",
     EstadoFactura = 1  // ? int
   }
   ?
   
5?? FacturaService (Conversión DTO ? Domain)
   var factura = MapDtoToDomain(dto);
   // EstadoFactura = (EstadoFactura)1 ? EstadoFactura.Emitida
   ?
   
6?? Factura (Domain) - Componente Blazor
   <h1>@factura.Numero</h1>
   @if (factura.EstaVencida()) { ... }
```

---

## ?? Diferencia: Domain vs DTOs

### **Domain (Factura.cs)**

```csharp
public class Factura
{
    public int Id { get; set; }
    public string Numero { get; set; }
    public EstadoFactura Estado { get; set; }  // ? Enum tipado
    public List<ConceptoFactura> Conceptos { get; set; }  // ? Relaciones
    
    // ? LÓGICA DE NEGOCIO
    public void CalcularTotal() { ... }
    public bool EstaVencida() { ... }
    public void Validar() { ... }
}
```

### **DTO (FacturaDto.cs)**

```csharp
public class FacturaDto
{
    public int IdFactura { get; set; }         // ? Mapea: id_factura
    public string Folio { get; set; }
    public int EstadoFactura { get; set; }     // ? int (no Enum)
    public DateTime FechaCreacion { get; set; } // ? Auditoría
    public string CreadoPor { get; set; }
    
    // ? Sin métodos de negocio
    // ? Sin validaciones
    // ? Sin relaciones complejas
}
```

---

## ?? Uso de DatabaseHelper

### **Ejemplo: GetAllAsync**

```csharp
public async Task<List<FacturaDto>> GetAllAsync()
{
    // ? DatabaseHelper mapea automáticamente:
    // - snake_case (BD) ? PascalCase (DTO)
    // - Tipos compatibles (INT ? int, DECIMAL ? decimal)
    
    return await Task.Run(() => 
        _dbHelper.ExecuteStoredProcedure<FacturaDto>(
            "sp_Finanzas_Facturas_GetAll"));
}
```

### **Ejemplo: GetByIdAsync**

```csharp
public async Task<FacturaDto?> GetByIdAsync(int idFactura)
{
    var parametros = new Dictionary<string, object>
    {
        ["p_id_factura"] = idFactura  // ? Parámetro del SP
    };
    
    return await Task.Run(() => 
        _dbHelper.ExecuteStoredProcedureSingle<FacturaDto>(
            "sp_Finanzas_Facturas_GetById", 
            parametros));
}
```

### **Ejemplo: CreateAsync**

```csharp
public async Task<int> CreateAsync(CreateFacturaDto factura)
{
    var parametros = new Dictionary<string, object>
    {
        ["p_folio"] = factura.Folio,
        ["p_fecha_emision"] = factura.FechaEmision,
        ["p_subtotal"] = factura.Subtotal,
        // ... más parámetros
    };
    
    // ExecuteScalar retorna el ID generado
    var resultado = await Task.Run(() => 
        _dbHelper.ExecuteScalar(
            "sp_Finanzas_Facturas_Insert", 
            parametros));
    
    return Convert.ToInt32(resultado ?? 0);
}
```

---

## ?? Stored Procedures Requeridos

### **Facturas**

| SP | Parámetros | Retorna |
|----|-----------|---------|
| `sp_Finanzas_Facturas_GetAll` | - | Lista de facturas |
| `sp_Finanzas_Facturas_GetById` | `p_id_factura INT` | Factura única |
| `sp_Finanzas_Facturas_GetByEstado` | `p_estado_factura INT` | Lista filtrada |
| `sp_Finanzas_Facturas_GetByFechas` | `p_fecha_inicio DATE`, `p_fecha_fin DATE` | Lista filtrada |
| `sp_Finanzas_Facturas_Insert` | Todos los campos | `LAST_INSERT_ID()` |
| `sp_Finanzas_Facturas_Update` | `p_id_factura INT`, campos | Filas afectadas |
| `sp_Finanzas_Facturas_Timbrar` | `p_id_factura INT`, `p_uuid_sat VARCHAR`, ... | Filas afectadas |
| `sp_Finanzas_Facturas_Cancelar` | `p_id_factura INT`, `p_motivo TEXT`, ... | Filas afectadas |
| `sp_Finanzas_Facturas_GetTotalIngresos` | `p_fecha_inicio DATE`, `p_fecha_fin DATE` | `DECIMAL` |

### **Pagos**

| SP | Parámetros | Retorna |
|----|-----------|---------|
| `sp_Finanzas_Pagos_GetAll` | - | Lista de pagos |
| `sp_Finanzas_Pagos_GetById` | `p_id_pago INT` | Pago único |
| `sp_Finanzas_Pagos_GetByFactura` | `p_id_factura INT` | Lista filtrada |
| `sp_Finanzas_Pagos_GetByFechas` | `p_fecha_inicio DATE`, `p_fecha_fin DATE` | Lista filtrada |
| `sp_Finanzas_Pagos_Insert` | Todos los campos | `LAST_INSERT_ID()` |
| `sp_Finanzas_Pagos_Update` | `p_id_pago INT`, campos | Filas afectadas |
| `sp_Finanzas_Pagos_Cancelar` | `p_id_pago INT`, `p_motivo TEXT`, ... | Filas afectadas |

---

## ? Ventajas de esta Arquitectura

| Ventaja | Descripción |
|---------|-------------|
| **Desacoplamiento** | BD puede cambiar sin afectar Domain |
| **Mapeo automático** | DatabaseHelper convierte snake_case ? PascalCase |
| **Lógica centralizada** | Validaciones solo en Domain/Service |
| **Testeable** | Puedes mockear IFacturaRepository |
| **Mantenible** | Cambios en BD solo afectan DTOs/Repository |

---

## ?? Registro en FinanzasModule

```csharp
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // ? Registrar DatabaseHelper (inyectado por Host)
    // Ya está registrado en Program.cs del Host
    
    // ? Registrar Repositories
    services.AddScoped<IFacturaRepository, FacturaRepository>();
    services.AddScoped<IPagoRepository, PagoRepository>();
    
    // ? Registrar Services (negocio)
    services.AddScoped<IFacturaService, FacturaService>();
    services.AddScoped<IPagoService, PagoService>();
}
```

---

## ?? Referencias

- **DatabaseHelper**: `Core.Abstractions/Infrastructure/Data/DatabaseHelper.cs`
- **Mapeo automático**: snake_case ? PascalCase
- **Tipos soportados**: int, string, decimal, DateTime, bool, Guid, Enum

---

**Última actualización:** 2024
