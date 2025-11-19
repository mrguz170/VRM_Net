# ?? Ejemplos Prácticos de Uso de Generadores VRM

Este documento contiene ejemplos reales y casos de uso comunes para los scripts de generación de plugins.

---

## ?? Índice de Ejemplos

1. [Crear Módulo de Finanzas Completo](#ejemplo-1-módulo-de-finanzas-completo)
2. [Crear Módulo de Inventario Incremental](#ejemplo-2-módulo-de-inventario-incremental)
3. [Módulo Multi-Componente (RRHH)](#ejemplo-3-módulo-multi-componente-rrhh)
4. [Agregar Componente a Módulo Existente](#ejemplo-4-agregar-componente-a-módulo-existente)
5. [Módulo con Componentes Relacionados](#ejemplo-5-módulo-con-componentes-relacionados)

---

## Ejemplo 1: Módulo de Finanzas Completo

### Objetivo
Crear módulo de Finanzas con componente Facturas, luego agregar CobrosYPagos.

### Paso 1: Crear módulo con primer componente
```powershell
.\New-VRMPlugin.ps1 `
    -ModuleName "Finanzas" `
    -IdModule 1 `
    -Category "Finanzas" `
    -StartIdComponent 1 `
    -StartIdAction 1 `
    -IconRoot "ri-money-dollar-circle-line" `
    -FirstComponent "Facturas"
```

**Resultado:**
```
? Módulo Finanzas creado
? Componente Facturas creado
? IFacturaService registrado
? Ruta: /finanzas/facturas
```

### Paso 2: Personalizar Factura.cs
```csharp
// src/Modules/Finanzas/VRM_Plugin.Modules.Finanzas/Domain/Factura.cs
namespace VRM_Plugin.Modules.Finanzas.Domain;

public class Factura
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string RfcCliente { get; set; } = string.Empty;
    public string NombreCliente { get; set; } = string.Empty;
    public decimal Subtotal { get; set; }
    public decimal Impuestos { get; set; }
    public decimal Total { get; set; }
    public EstadoFactura Estado { get; set; }
    public string CreadoPor { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}

public enum EstadoFactura
{
    Borrador,
    Pendiente,
    Pagada,
    Vencida,
    Cancelada
}
```

### Paso 3: Agregar segundo componente
```powershell
.\Add-VRMComponent.ps1 `
    -ModuleName "Finanzas" `
    -ComponentName "CobrosYPagos" `
    -CreateEntity `
    -CreateService
```

### Paso 4: Actualizar FinanzasModule.cs
```csharp
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // ? Servicios de componentes
    services.AddScoped<IFacturaService, FacturaService>();
    services.AddScoped<ICobrosYPagosService, CobrosYPagosService>();  // ? Agregar
}

public List<ModuleComponent> GetComponents()
{
    return new List<ModuleComponent>
    {
        // Raíz
        new ModuleComponent { IdComponent = 1, IdParent = null, Name = "Finanzas", ... },
        
        // Facturas
        new ModuleComponent { IdComponent = 2, IdParent = 1, Name = "Facturas", Route = "/finanzas/facturas", ... },
        
        // ? Agregar CobrosYPagos
        new ModuleComponent 
        { 
            IdComponent = 3, 
            IdParent = 1, 
            ComponentCode = "Finanzas.CobrosYPagos",
            Name = "Cobros y Pagos", 
            Route = "/finanzas/cobrosypagos",
            Icon = "ri-hand-coin-line",
            ComponentType = typeof(Components.CobrosYPagos),
            ComponentTypeName = "VRM_Plugin.Modules.Finanzas.Components.CobrosYPagos",
            RequiredPermissionIds = new List<int> { 1 },
            IsActive = true
        }
    };
}
```

### Paso 5: Compilar y probar
```powershell
cd src\Modules\Finanzas\VRM_Plugin.Modules.Finanzas
dotnet build --configuration Release

# Copiar DLL al Host (si no se hizo automáticamente)
copy "bin\Release\net8.0\VRM_Plugin.Modules.Finanzas.dll" "..\..\..\..\Host\VRM_Plugin.Blazor.Server\bin\Debug\net8.0\Modules\"

# Ejecutar
cd ..\..\..\..\Host\VRM_Plugin.Blazor.Server
dotnet run
```

---

## Ejemplo 2: Módulo de Inventario Incremental

### Objetivo
Crear módulo vacío primero, luego agregar componentes uno por uno.

### Paso 1: Crear estructura base
```powershell
.\New-VRMPlugin.ps1 `
    -ModuleName "Inventario" `
    -IdModule 3 `
    -Category "Operaciones" `
    -StartIdComponent 100 `
    -StartIdAction 100 `
    -IconRoot "ri-box-line"
```

**Resultado:**
```
? Módulo Inventario creado (vacío)
?? Sin componentes funcionales todavía
```

### Paso 2: Planificar componentes
```
Inventario/
??? Productos      ? Catálogo de productos
??? Almacenes      ? Ubicaciones de stock
??? Movimientos    ? Entradas/Salidas
??? Reportes       ? Inventario valorizado
```

### Paso 3: Agregar Productos
```powershell
.\Add-VRMComponent.ps1 `
    -ModuleName "Inventario" `
    -ComponentName "Productos" `
    -CreateEntity `
    -CreateService
```

**Editar Domain/Producto.cs:**
```csharp
public class Producto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int IdCategoria { get; set; }
    public decimal PrecioCompra { get; set; }
    public decimal PrecioVenta { get; set; }
    public int StockMinimo { get; set; }
    public int StockActual { get; set; }
    public UnidadMedida Unidad { get; set; }
    public bool Activo { get; set; } = true;
}

public enum UnidadMedida
{
    Pieza,
    Caja,
    Kilogramo,
    Litro,
    Metro
}
```

### Paso 4: Agregar Almacenes
```powershell
.\Add-VRMComponent.ps1 `
    -ModuleName "Inventario" `
    -ComponentName "Almacenes" `
    -CreateEntity `
    -CreateService
```

### Paso 5: Actualizar InventarioModule.cs
```csharp
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddScoped<IProductoService, ProductoService>();
    services.AddScoped<IAlmacenService, AlmacenService>();
}

public List<ModuleComponent> GetComponents()
{
    return new List<ModuleComponent>
    {
        // Raíz
        new ModuleComponent { IdComponent = 100, IdParent = null, Name = "Inventario", ... },
        
        // Productos
        new ModuleComponent { IdComponent = 101, IdParent = 100, Name = "Productos", Route = "/inventario/productos", ... },
        
        // Almacenes
        new ModuleComponent { IdComponent = 102, IdParent = 100, Name = "Almacenes", Route = "/inventario/almacenes", ... }
    };
}

public List<ModuleAction> GetActions()
{
    return new List<ModuleAction>
    {
        // Acciones de Productos
        new ModuleAction { IdAction = 100, IdComponent = 101, ActionKey = "Inventario.Productos.Ver", ... },
        new ModuleAction { IdAction = 101, IdComponent = 101, ActionKey = "Inventario.Productos.Crear", ... },
        
        // Acciones de Almacenes
        new ModuleAction { IdAction = 110, IdComponent = 102, ActionKey = "Inventario.Almacenes.Ver", ... },
        new ModuleAction { IdAction = 111, IdComponent = 102, ActionKey = "Inventario.Almacenes.Crear", ... }
    };
}
```

---

## Ejemplo 3: Módulo Multi-Componente (RRHH)

### Objetivo
Crear módulo de Recursos Humanos con múltiples componentes relacionados.

### Paso 1: Crear módulo con primer componente
```powershell
.\New-VRMPlugin.ps1 `
    -ModuleName "RRHH" `
    -IdModule 5 `
    -Category "Administracion" `
    -StartIdComponent 200 `
    -StartIdAction 200 `
    -IconRoot "ri-team-line" `
    -FirstComponent "Empleados"
```

### Paso 2: Agregar componentes adicionales
```powershell
# Nóminas
.\Add-VRMComponent.ps1 -ModuleName "RRHH" -ComponentName "Nominas" -CreateEntity -CreateService

# Asistencias
.\Add-VRMComponent.ps1 -ModuleName "RRHH" -ComponentName "Asistencias" -CreateEntity -CreateService

# Vacaciones
.\Add-VRMComponent.ps1 -ModuleName "RRHH" -ComponentName "Vacaciones" -CreateEntity -CreateService

# Incidencias
.\Add-VRMComponent.ps1 -ModuleName "RRHH" -ComponentName "Incidencias" -CreateEntity -CreateService
```

### Paso 3: Estructura final
```
RRHH/
??? Domain/
?   ??? Empleado.cs
?   ??? Nomina.cs
?   ??? Asistencia.cs
?   ??? Vacacion.cs
?   ??? Incidencia.cs
??? Services/
?   ??? IEmpleadoService.cs / EmpleadoService.cs
?   ??? INominaService.cs / NominaService.cs
?   ??? IAsistenciaService.cs / AsistenciaService.cs
?   ??? IVacacionService.cs / VacacionService.cs
?   ??? IIncidenciaService.cs / IncidenciaService.cs
??? Components/
    ??? Empleados.razor
    ??? Nominas.razor
    ??? Asistencias.razor
    ??? Vacaciones.razor
    ??? Incidencias.razor
```

### Paso 4: Configurar RRHHModule.cs
```csharp
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddScoped<IEmpleadoService, EmpleadoService>();
    services.AddScoped<INominaService, NominaService>();
    services.AddScoped<IAsistenciaService, AsistenciaService>();
    services.AddScoped<IVacacionService, VacacionService>();
    services.AddScoped<IIncidenciaService, IncidenciaService>();
}

public List<ModuleComponent> GetComponents()
{
    return new List<ModuleComponent>
    {
        new ModuleComponent { IdComponent = 200, IdParent = null, Name = "RRHH", Icon = "ri-team-line", ... },
        new ModuleComponent { IdComponent = 201, IdParent = 200, Name = "Empleados", Route = "/rrhh/empleados", ... },
        new ModuleComponent { IdComponent = 202, IdParent = 200, Name = "Nóminas", Route = "/rrhh/nominas", ... },
        new ModuleComponent { IdComponent = 203, IdParent = 200, Name = "Asistencias", Route = "/rrhh/asistencias", ... },
        new ModuleComponent { IdComponent = 204, IdParent = 200, Name = "Vacaciones", Route = "/rrhh/vacaciones", ... },
        new ModuleComponent { IdComponent = 205, IdParent = 200, Name = "Incidencias", Route = "/rrhh/incidencias", ... }
    };
}
```

---

## Ejemplo 4: Agregar Componente a Módulo Existente

### Objetivo
Agregar componente "Reportes" al módulo de Finanzas existente.

### Paso 1: Agregar componente
```powershell
.\Add-VRMComponent.ps1 `
    -ModuleName "Finanzas" `
    -ComponentName "Reportes" `
    -CreateEntity `
    -CreateService
```

### Paso 2: Personalizar Reporte.cs
```csharp
namespace VRM_Plugin.Modules.Finanzas.Domain;

public class Reporte
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public TipoReporte Tipo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string GeneradoPor { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
    public string? RutaArchivo { get; set; }
}

public enum TipoReporte
{
    IngresosMensuales,
    FacturasPendientes,
    EstadoCuenta,
    Conciliacion,
    Impuestos
}
```

### Paso 3: Implementar IReporteService
```csharp
public interface IReporteService
{
    Task<List<Reporte>> GetReportesAsync();
    Task<byte[]> GenerarReporteIngresosAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<byte[]> GenerarReporteFacturasPendientesAsync();
    Task<Reporte> GuardarReporteAsync(Reporte reporte);
}
```

### Paso 4: Actualizar FinanzasModule.cs
```csharp
// ConfigureServices
services.AddScoped<IReporteService, ReporteService>();

// GetComponents
new ModuleComponent 
{ 
    IdComponent = 4,
    IdParent = 1,
    ComponentCode = "Finanzas.Reportes",
    Name = "Reportes",
    Route = "/finanzas/reportes",
    Icon = "ri-file-chart-line",
    ComponentType = typeof(Components.Reportes),
    RequiredPermissionIds = new List<int> { 1, 2 },  // Solo Admin y Gerente
    IsActive = true
}

// GetActions
new ModuleAction { IdAction = 20, IdComponent = 4, ActionKey = "Finanzas.Reportes.Ver", IdActionType = 1, ... },
new ModuleAction { IdAction = 21, IdComponent = 4, ActionKey = "Finanzas.Reportes.Generar", IdActionType = 2, ... },
new ModuleAction { IdAction = 22, IdComponent = 4, ActionKey = "Finanzas.Reportes.Exportar", IdActionType = 2, ... }
```

---

## Ejemplo 5: Módulo con Componentes Relacionados

### Objetivo
Crear módulo de Ventas donde Pedidos y Clientes están relacionados.

### Paso 1: Crear módulo con Pedidos
```powershell
.\New-VRMPlugin.ps1 `
    -ModuleName "Ventas" `
    -IdModule 4 `
    -Category "Comercial" `
    -StartIdComponent 150 `
    -StartIdAction 150 `
    -IconRoot "ri-shopping-cart-line" `
    -FirstComponent "Pedidos"
```

### Paso 2: Agregar Clientes
```powershell
.\Add-VRMComponent.ps1 `
    -ModuleName "Ventas" `
    -ComponentName "Clientes" `
    -CreateEntity `
    -CreateService
```

### Paso 3: Definir entidades relacionadas
```csharp
// Domain/Cliente.cs
public class Cliente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string RFC { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public List<Pedido> Pedidos { get; set; } = new();  // ? Relación
}

// Domain/Pedido.cs
public class Pedido
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public int IdCliente { get; set; }
    public Cliente? Cliente { get; set; }  // ? Relación
    public DateTime FechaPedido { get; set; }
    public decimal Total { get; set; }
    public EstadoPedido Estado { get; set; }
}
```

### Paso 4: Servicio con relaciones
```csharp
// Services/IPedidoService.cs
public interface IPedidoService
{
    Task<List<Pedido>> GetPedidosByClienteAsync(int idCliente);
    Task<Pedido?> GetPedidoConClienteAsync(int id);
    Task<List<Pedido>> GetPedidosPendientesAsync();
}

// Services/IClienteService.cs
public interface IClienteService
{
    Task<Cliente?> GetClienteConPedidosAsync(int id);
    Task<List<Cliente>> GetClientesActivosAsync();
}
```

### Paso 5: UI con navegación entre componentes
```razor
@* Components/Pedidos.razor *@
@page "/ventas/pedidos"
@inject IPedidoService PedidoService
@inject NavigationManager Navigation

<h1>Pedidos</h1>

<table class="table">
    <thead>
        <tr>
            <th>Número</th>
            <th>Cliente</th>
            <th>Fecha</th>
            <th>Total</th>
            <th>Acciones</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var pedido in pedidos)
        {
            <tr>
                <td>@pedido.Numero</td>
                <td>
                    @* ? Navegación a componente relacionado *@
                    <a href="/ventas/clientes?id=@pedido.IdCliente">
                        @pedido.Cliente?.Nombre
                    </a>
                </td>
                <td>@pedido.FechaPedido.ToShortDateString()</td>
                <td>@pedido.Total.ToString("C")</td>
                <td>
                    <button @onclick="() => VerDetalle(pedido.Id)">Ver</button>
                </td>
            </tr>
        }
    </tbody>
</table>
```

---

## ?? Script Batch para Múltiples Componentes

Si necesitas crear muchos componentes a la vez:

```powershell
# batch-create-components.ps1

param(
    [Parameter(Mandatory=$true)]
    [string]$ModuleName,
    
    [Parameter(Mandatory=$true)]
    [string[]]$ComponentNames
)

foreach ($component in $ComponentNames) {
    Write-Host "`n?? Creando componente: $component..." -ForegroundColor Cyan
    
    .\Add-VRMComponent.ps1 `
        -ModuleName $ModuleName `
        -ComponentName $component `
        -CreateEntity `
        -CreateService
    
    Write-Host "? $component creado" -ForegroundColor Green
}

Write-Host "`n? Todos los componentes creados!" -ForegroundColor Green
Write-Host "?? No olvides actualizar ${ModuleName}Module.cs" -ForegroundColor Yellow
```

**Uso:**
```powershell
.\batch-create-components.ps1 -ModuleName "RRHH" -ComponentNames @("Empleados", "Nominas", "Asistencias", "Vacaciones")
```

---

## ?? Tabla de Referencia Rápida

| Escenario | Comando | Resultado |
|-----------|---------|-----------|
| **Módulo vacío** | `New-VRMPlugin.ps1 -ModuleName X ...` | Solo estructura |
| **Módulo + componente** | `New-VRMPlugin.ps1 ... -FirstComponent Y` | Módulo listo |
| **Agregar componente** | `Add-VRMComponent.ps1 ...` | Componente nuevo |
| **Solo estructura** | `New-VRMPluginBase.ps1 ...` | Base mínima |

---

**Última actualización:** 2025-01-21
