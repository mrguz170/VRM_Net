# ?? Ejemplo de Implementación: Facturas.razor con Acciones Granulares

**Fecha:** Enero 10, 2025  
**Módulo:** Finanzas  
**Componente:** Facturas.razor  
**Estado:** ? IMPLEMENTADO

---

## ?? **OBJETIVO**

Demostrar cómo implementar una página completa de Blazor que utilice el sistema de **acciones granulares** definido en `FinanzasModule.GetActions()` para controlar permisos a nivel de botón/operación.

---

## ?? **ACCIONES IMPLEMENTADAS**

### **Acciones del Componente "Facturas" (IdComponent = 2)**

| IdAction | ActionKey | Nombre | Tipo | Permisos | Estado |
|----------|-----------|--------|------|----------|--------|
| 1 | `Finanzas.Facturas.Ver` | Ver Facturas | Lectura | Admin, Gerente, Coordinador, Contador | ? Protege toda la tabla |
| 2 | `Finanzas.Facturas.Crear` | Crear Factura | Escritura | Admin, Gerente, Coordinador | ? Botón "Nueva Factura" |
| 3 | `Finanzas.Facturas.Editar` | Editar Factura | Escritura | Admin, Gerente, Coordinador | ? Botón editar por fila |
| 4 | `Finanzas.Facturas.Eliminar` | Eliminar Factura | Crítica | Admin, Gerente | ? Botón eliminar (solo Borrador) |
| 5 | `Finanzas.Facturas.TimbrarSAT` | Timbrar en SAT | Crítica | Admin, Gerente | ? Botón timbrar (solo Gerente) |
| 6 | `Finanzas.Facturas.CancelarTimbrada` | Cancelar Timbrada | Crítica | Admin, Gerente | ? Botón cancelar SAT (solo Gerente) |

---

## ??? **ARQUITECTURA DE LA PÁGINA**

```
Facturas.razor
?
??? Header
?   ??? Título + Descripción
?
??? Filtros y Acciones
?   ??? Buscador (Número, RFC, Cliente)
?   ??? Filtro por Estado
?   ??? ? Botón "Nueva Factura" (ACCIÓN 2)
?       ??? AuthorizeView Roles="Admin,GerenteFinanzas,CoordinadorFinanzas"
?
??? Tabla de Facturas (ACCIÓN 1)
?   ??? Columnas: Número, Cliente, RFC, Fecha, Total, Estado
?   ??? Columna Acciones (por fila):
?       ??? ? Botón Editar (ACCIÓN 3)
?       ?   ??? AuthorizeView Roles="Admin,GerenteFinanzas,CoordinadorFinanzas"
?       ??? ? Botón Timbrar SAT (ACCIÓN 5) - Condicional
?       ?   ??? AuthorizeView Roles="Admin,GerenteFinanzas"
?       ?   ??? Solo si Estado = Borrador o Pendiente
?       ??? ? Botón Cancelar Timbrada (ACCIÓN 6) - Condicional
?       ?   ??? AuthorizeView Roles="Admin,GerenteFinanzas"
?       ?   ??? Solo si Estado = Timbrada o Pagada
?       ??? ? Botón Eliminar (ACCIÓN 4) - Condicional
?           ??? AuthorizeView Roles="Admin,GerenteFinanzas"
?           ??? Solo si Estado = Borrador
?
??? Estadísticas
?   ??? Total Facturas
?   ??? Timbradas
?   ??? Pendientes
?   ??? Total Facturado
?
??? Modal Crear/Editar
    ??? Formulario de factura (placeholder)
```

---

## ?? **IMPLEMENTACIÓN DE PERMISOS**

### **Enfoque 1: AuthorizeView (Usado en este ejemplo)**

```razor
<!-- ? ACCIÓN: Crear Factura (IdAction = 2) -->
<AuthorizeView Roles="Admin,GerenteFinanzas,CoordinadorFinanzas">
    <Authorized>
        <button class="btn btn-primary" @onclick="AbrirModalCrear">
            <i class="ri-add-line mr-1"></i>
            Nueva Factura
        </button>
    </Authorized>
</AuthorizeView>
```

**Ventajas:**
- ? Estándar de ASP.NET Core
- ? No requiere componentes custom
- ? Funciona con roles del usuario autenticado

**Desventajas:**
- ?? Menos granular (roles en lugar de IDs de permiso)
- ?? Requiere mapeo manual de roles

---

### **Enfoque 2: AuthorizeAction (Componente Custom)**

```razor
<!-- ? Usando componente custom (requiere estar en el Host) -->
<AuthorizeAction ActionKey="Finanzas.Facturas.Crear">
    <button class="btn btn-primary" @onclick="AbrirModalCrear">
        <i class="ri-add-line mr-1"></i>
        Nueva Factura
    </button>
</AuthorizeAction>
```

**Ventajas:**
- ? Más granular (usa `ActionKey` directo de BD)
- ? Centraliza lógica de permisos
- ? Fácil de mantener

**Desventajas:**
- ?? Requiere que los componentes estén en el Host
- ?? No disponible en módulos (cross-assembly)

**?? Nota:** En este ejemplo usamos `AuthorizeView` porque los componentes de Auth (`AuthorizeAction`, `AuthorizeModule`) están en el proyecto Host y no son accesibles desde los módulos. Para usar `AuthorizeAction` en módulos, se necesitaría crear una librería compartida de componentes.

---

## ?? **ESTADOS DE FACTURA**

```csharp
public enum EstadoFactura
{
    Borrador = 0,      // Creada pero no enviada
    Pendiente = 1,     // Enviada, pendiente de pago
    Timbrada = 2,      // Timbrada en SAT
    PagoParcial = 3,   // Pagada parcialmente
    Pagada = 4,        // Pagada completamente
    Vencida = 5,       // Vencida sin pago
    Cancelada = 6      // Cancelada
}
```

**Badges de Estado:**

| Estado | Badge | CSS Class |
|--------|-------|-----------|
| Borrador | Gris | `bg-secondary` |
| Pendiente | Azul | `bg-info` |
| **Timbrada** | Verde | `bg-success` |
| Pagada | Azul Oscuro | `bg-primary` |
| Cancelada | Rojo | `bg-danger` |
| Vencida | Amarillo | `bg-warning` |

---

## ?? **LÓGICA CONDICIONAL DE BOTONES**

### **Timbrar SAT (ACCIÓN 5)**

```razor
@if (factura.Estado == EstadoFactura.Borrador || factura.Estado == EstadoFactura.Pendiente)
{
    <AuthorizeView Roles="Admin,GerenteFinanzas">
        <Authorized>
            <button class="btn btn-outline-success" 
                    title="Timbrar en SAT (Solo Gerente)"
                    @onclick="() => TimbrarFactura(factura)">
                <i class="ri-check-double-line"></i>
            </button>
        </Authorized>
    </AuthorizeView>
}
```

**Condiciones:**
1. ? Solo si `Estado = Borrador` o `Pendiente`
2. ? Solo si rol = `Admin` o `GerenteFinanzas`

---

### **Cancelar Timbrada (ACCIÓN 6)**

```razor
@if (factura.Estado == EstadoFactura.Timbrada || factura.Estado == EstadoFactura.Pagada)
{
    <AuthorizeView Roles="Admin,GerenteFinanzas">
        <Authorized>
            <button class="btn btn-outline-warning" 
                    title="Cancelar en SAT (Solo Gerente)"
                    @onclick="() => CancelarFacturaTimbrada(factura)">
                <i class="ri-close-circle-line"></i>
            </button>
        </Authorized>
    </AuthorizeView>
}
```

**Condiciones:**
1. ? Solo si `Estado = Timbrada` o `Pagada`
2. ? Solo si rol = `Admin` o `GerenteFinanzas`

---

### **Eliminar (ACCIÓN 4)**

```razor
@if (factura.Estado == EstadoFactura.Borrador)
{
    <AuthorizeView Roles="Admin,GerenteFinanzas">
        <Authorized>
            <button class="btn btn-outline-danger" 
                    title="Eliminar (Solo Gerente)"
                    @onclick="() => EliminarFactura(factura)">
                <i class="ri-delete-bin-line"></i>
            </button>
        </Authorized>
    </AuthorizeView>
}
```

**Condiciones:**
1. ? Solo si `Estado = Borrador` (no se pueden eliminar facturas timbradas)
2. ? Solo si rol = `Admin` o `GerenteFinanzas`

---

## ?? **DATOS DE EJEMPLO**

```csharp
private void CargarDatosEjemplo()
{
    _facturas = new List<Factura>
    {
        new Factura 
        { 
            Id = 1, 
            Numero = "FAC-2025-001", 
            FechaEmision = DateTime.Now.AddDays(-30), 
            FechaVencimiento = DateTime.Now.AddDays(-15), 
            RfcCliente = "ABC123456789", 
            NombreCliente = "Empresa Demo S.A. de C.V.", 
            Total = 11600m, 
            Estado = EstadoFactura.Timbrada 
        },
        // ... más facturas
    };
}
```

**Incluye:**
- ? 5 facturas de ejemplo
- ? Diferentes estados (Borrador, Timbrada, Pagada, Vencida, Cancelada)
- ? Fechas variadas (vencidas, próximas, sin vencimiento)
- ? Diferentes clientes y montos

---

## ?? **FILTROS IMPLEMENTADOS**

### **1. Búsqueda por Texto**

```csharp
private void AplicarFiltros()
{
    _facturasFiltradas = _facturas.Where(f =>
        (string.IsNullOrEmpty(_searchTerm) ||
         f.Numero.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
         f.NombreCliente.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
         f.RfcCliente.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase))
    ).ToList();
}
```

**Busca en:**
- Número de factura
- Nombre del cliente
- RFC del cliente

### **2. Filtro por Estado**

```razor
<select class="form-select" @bind="_filterEstado" @bind:after="AplicarFiltros">
    <option value="">Todos los estados</option>
    <option value="Borrador">Borrador</option>
    <option value="Pendiente">Pendiente</option>
    <option value="Timbrada">Timbrada</option>
    <option value="Pagada">Pagada</option>
    <option value="Cancelada">Cancelada</option>
    <option value="Vencida">Vencida</option>
</select>
```

---

## ?? **ESTADÍSTICAS**

```csharp
// Total de facturas
@_facturas.Count

// Timbradas
@_facturas.Count(f => f.Estado == EstadoFactura.Timbrada)

// Pendientes (Borrador + Pendiente)
@_facturas.Count(f => f.Estado == EstadoFactura.Borrador || f.Estado == EstadoFactura.Pendiente)

// Total Facturado
@_facturas.Sum(f => f.Total).ToString("C2")
```

---

## ?? **ESTILOS Y UI**

### **Iconos (Remix Icons)**

- ?? `ri-file-list-3-line` - Lista de facturas
- ? `ri-add-line` - Crear
- ?? `ri-edit-line` - Editar
- ? `ri-check-double-line` - Timbrar SAT
- ? `ri-close-circle-line` - Cancelar
- ??? `ri-delete-bin-line` - Eliminar

### **Cards de Estadísticas**

```html
<div class="card bg-primary text-white">
    <div class="card-body">
        <h6 class="text-uppercase mb-1">Total Facturas</h6>
        <h3 class="mb-0">@_facturas.Count</h3>
    </div>
</div>
```

**Colores:**
- ?? `bg-primary` - Total
- ?? `bg-success` - Timbradas
- ?? `bg-warning` - Pendientes
- ?? `bg-info` - Total Facturado

---

## ?? **PRÓXIMOS PASOS**

### **1. Integración con Backend Real**

Reemplazar `CargarDatosEjemplo()` con:

```csharp
@inject IFacturaService FacturaService

protected override async Task OnInitializedAsync()
{
    _facturas = await FacturaService.GetFacturasAsync();
    _facturasFiltradas = _facturas;
    _isLoading = false;
}
```

### **2. Formulario Completo de Factura**

Implementar modal con:
- Datos del cliente (RFC, Nombre, Dirección)
- Conceptos/Partidas (descripción, cantidad, precio)
- Cálculo automático de subtotal, IVA, total
- Validaciones (RFC válido, campos requeridos)
- Subida de archivos (comprobantes, contratos)

### **3. Integración con SAT**

```csharp
private async Task TimbrarFactura(Factura factura)
{
    // Llamar servicio de timbrado
    var resultado = await FacturaService.TimbrarEnSATAsync(factura.Id);
    
    if (resultado.Exito)
    {
        factura.Estado = EstadoFactura.Timbrada;
        factura.UUIDFiscal = resultado.UUID;
        // Mostrar notificación de éxito
    }
    else
    {
        // Mostrar error
    }
}
```

### **4. Paginación**

Agregar componente de paginación:

```razor
<Pagination 
    CurrentPage="@_currentPage" 
    TotalPages="@_totalPages" 
    OnPageChanged="@OnPageChanged" />
```

### **5. Exportación**

Agregar botones de exportación:

```razor
<button class="btn btn-success btn-sm" @onclick="ExportarExcel">
    <i class="ri-file-excel-line mr-1"></i>
    Exportar Excel
</button>

<button class="btn btn-danger btn-sm" @onclick="ExportarPDF">
    <i class="ri-file-pdf-line mr-1"></i>
    Exportar PDF
</button>
```

---

## ? **CHECKLIST DE IMPLEMENTACIÓN**

- [x] Definir acciones en `FinanzasModule.GetActions()`
- [x] Agregar estado `Timbrada` al enum `EstadoFactura`
- [x] Crear página `Facturas.razor`
- [x] Implementar búsqueda y filtros
- [x] Proteger botones con `AuthorizeView`
- [x] Lógica condicional de botones por estado
- [x] Estadísticas dinámicas
- [x] Datos de ejemplo
- [ ] Integración con backend real
- [ ] Formulario completo de factura
- [ ] Integración con SAT
- [ ] Paginación
- [ ] Exportación (Excel, PDF)

---

## ?? **ARCHIVOS RELACIONADOS**

- ?? `Facturas.razor` - Página principal (este archivo)
- ?? `FinanzasModule.cs` - Definición de módulo y acciones
- ?? `EstadoFactura.cs` - Enum de estados
- ?? `Factura.cs` - Entidad de dominio
- ?? `IFacturaService.cs` - Interfaz de servicio
- ?? `FacturaService.cs` - Implementación de servicio (TODO)

---

## ?? **LECCIONES APRENDIDAS**

### **1. Componentes de Auth en Módulos**

? **No funcionan** porque están en el proyecto Host:
```razor
<!-- ? NO funciona en módulos -->
<AuthorizeAction ActionKey="Finanzas.Facturas.Crear">
```

? **Usar `AuthorizeView` estándar:**
```razor
<!-- ? Funciona en cualquier parte -->
<AuthorizeView Roles="Admin,GerenteFinanzas">
```

**Alternativa:** Crear librería compartida de componentes de UI que pueda ser referenciada por módulos.

### **2. Permisos por Estado**

? **Combinar** autorizacion de rol + estado de entidad:

```razor
@if (factura.Estado == EstadoFactura.Borrador)
{
    <AuthorizeView Roles="Admin,GerenteFinanzas">
        <button @onclick="() => EliminarFactura(factura)">
            Eliminar
        </button>
    </AuthorizeView>
}
```

### **3. Enum Extensible**

? Agregar estados conforme al flujo de negocio:

```csharp
public enum EstadoFactura
{
    Borrador = 0,
    Pendiente = 1,
    Timbrada = 2,     // ? Agregado para SAT
    PagoParcial = 3,
    Pagada = 4,
    Vencida = 5,
    Cancelada = 6
}
```

---

**Última actualización:** Enero 10, 2025  
**Autor:** Equipo VRM_Net  
**Estado:** ? IMPLEMENTACIÓN COMPLETA Y FUNCIONAL
