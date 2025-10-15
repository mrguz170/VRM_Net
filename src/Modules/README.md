# Modules - Modulos de Negocio

Esta carpeta contiene los modulos de negocio que se cargan dinamicamente.

## Modulos Disponibles

### 1. Modulo Finanzas

**Ubicacion:** `src/Modules/Finanzas/VRM_PluginDemo.Modules.Finanzas/`

**Proposito:** Gestion de facturas, pagos y finanzas.

**Ruta:** `/finanzas`

**Servicios:**
- `IFacturaService` - Gestion de facturas
- `IPagoService` - Gestion de pagos

**Permisos Granulares:**

| Accion | Roles Permitidos |
|--------|------------------|
| `Finanzas.Facturas.Ver` | Admin, GerenteFinanzas, CoordinadorFinanzas, Contador |
| `Finanzas.Facturas.Crear` | Admin, GerenteFinanzas, CoordinadorFinanzas |
| `Finanzas.Facturas.Editar` | Admin, GerenteFinanzas, CoordinadorFinanzas |
| `Finanzas.Facturas.Eliminar` | Admin, GerenteFinanzas |
| `Finanzas.Facturas.TimbrarSAT` | **Admin, GerenteFinanzas** (Granular) |
| `Finanzas.Reportes.VerSensibles` | **Admin, GerenteFinanzas** (Granular) |

---

### 2. Modulo Prospectos

**Ubicacion:** `src/Modules/Onboarding/VRM_PluginDemo.Modules.Prospectos/`

**Proposito:** Gestion de onboarding de clientes potenciales.

**Ruta:** `/prospectos`

**Servicios:**
- `IProspectoService` - Gestion de prospectos

**Permisos Granulares:**

| Accion | Roles Permitidos |
|--------|------------------|
| `Prospectos.Ver` | Admin, GestorProspectos, RevisorLegal, RevisorFinanzas |
| `Prospectos.Crear` | Admin, GestorProspectos |
| `Prospectos.Editar` | Admin, GestorProspectos |
| `Prospectos.Aprobar` | **Admin, GestorProspectos** (Granular) |
| `Prospectos.Rechazar` | **Admin, GestorProspectos** (Granular) |
| `Prospectos.RevisionLegal` | **Admin, RevisorLegal** (Granular) |
| `Prospectos.RevisionFinanciera` | **Admin, RevisorFinanzas** (Granular) |

---

## Como Crear un Nuevo Modulo

### Paso 1: Crear Proyecto

```bash
dotnet new razorclasslib -n VRM_PluginDemo.Modules.Inventario -o src/Modules/Inventario/VRM_PluginDemo.Modules.Inventario
```

### Paso 2: Agregar Referencias

```xml
<!-- Inventario.csproj -->
<ItemGroup>
  <ProjectReference Include="..\..\..\Core\VRM_PluginDemo.Core.Abstractions\VRM_PluginDemo.Core.Abstractions.csproj" />
  <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="8.0.20" />
  <PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="8.0.20" />
</ItemGroup>
```

### Paso 3: Implementar IModule

```csharp
// InventarioModule.cs
using VRM_PluginDemo.Core.Abstractions;

namespace VRM_PluginDemo.Modules.Inventario;

public class InventarioModule : IModule
{
    public string ModuleId => "Inventario";
    public string DisplayName => "Gestion de Inventario";
    public string Description => "Control de productos y almacenes";
    public string Version => "1.0.0";
    public string Author => "Tu Nombre";
    public string Category => "Operaciones";
    
    public List<ComponentInfo> GetComponents()
    {
        return new List<ComponentInfo>
        {
            new ComponentInfo
            {
                Name = "Inventario",
                Route = "/inventario",
                ComponentType = typeof(Components.Inventario),
                ShowInMenu = true,
                MenuOrder = 3,
                Icon = "bi bi-box-seam",
                Description = "Gestion de productos"
            }
        };
    }
    
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IAlmacenService, AlmacenService>();
    }
    
    public Dictionary<string, string[]> GetActionPermissions()
    {
        return new Dictionary<string, string[]>
        {
            ["Inventario.Productos.Ver"] = new[] 
            { 
                "Admin", "GerenteInventario", "Almacenista" 
            },
            
            ["Inventario.Productos.Crear"] = new[] 
            { 
                "Admin", "GerenteInventario" 
            },
            
            ["Inventario.Productos.AjustarStock"] = new[] 
            { 
                "Admin", "GerenteInventario", "Almacenista" 
            },
            
            // Permiso granular: Solo gerentes ven reportes valorizados
            ["Inventario.Reportes.VerValorizado"] = new[] 
            { 
                "Admin", "GerenteInventario" 
            }
        };
    }
    
    public List<string> Dependencies => new List<string>();
    public List<string> RequiredPermissions => new List<string>();
}
```

### Paso 4: Crear Componente Blazor

```razor
<!-- Components/Inventario.razor -->
@page "/inventario"
@rendermode InteractiveServer
@using Microsoft.AspNetCore.Components.Authorization

<PageTitle>Inventario</PageTitle>

<h1>Gestion de Inventario</h1>

<AuthorizeView Roles="Admin,GerenteInventario,Almacenista">
    <Authorized>
        <div class="row">
            <div class="col-md-12">
                <div class="card">
                    <div class="card-header">
                        <h5>Productos en Stock</h5>
                    </div>
                    <div class="card-body">
                        <!-- Lista de productos -->
                        
                        <!-- Boton crear: Solo gerentes -->
                        <AuthorizeView Roles="Admin,GerenteInventario">
                            <Authorized>
                                <button class="btn btn-primary">
                                    Nuevo Producto
                                </button>
                            </Authorized>
                        </AuthorizeView>
                        
                        <!-- Boton ajustar stock: Gerentes y almacenistas -->
                        <AuthorizeView Roles="Admin,GerenteInventario,Almacenista">
                            <Authorized>
                                <button class="btn btn-warning">
                                    Ajustar Stock
                                </button>
                            </Authorized>
                        </AuthorizeView>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- Reportes valorizados: SOLO gerentes -->
        <AuthorizeView Roles="Admin,GerenteInventario">
            <Authorized>
                <div class="card border-danger mt-3">
                    <div class="card-header bg-danger text-white">
                        <h5>Reportes Valorizados</h5>
                    </div>
                    <div class="card-body">
                        <button>Inventario Valorizado</button>
                        <button>Costo Promedio</button>
                    </div>
                </div>
            </Authorized>
            <NotAuthorized>
                <div class="alert alert-warning mt-3">
                    Solo disponible para gerentes de inventario
                </div>
            </NotAuthorized>
        </AuthorizeView>
    </Authorized>
    <NotAuthorized>
        <div class="alert alert-warning">
            No tienes acceso a este modulo
        </div>
    </NotAuthorized>
</AuthorizeView>

@code {
    // Logica del componente
}
```

### Paso 5: Crear Servicios

```csharp
// Services/IProductoService.cs
public interface IProductoService
{
    Task<List<Producto>> ObtenerTodosAsync();
    Task<Producto?> ObtenerPorIdAsync(int id);
    Task CrearAsync(Producto producto);
    Task ActualizarAsync(Producto producto);
    Task EliminarAsync(int id);
}

// Services/ProductoService.cs
public class ProductoService : IProductoService
{
    // Implementacion
}
```

### Paso 6: Compilar y Desplegar

```bash
# Compilar
dotnet build src/Modules/Inventario/VRM_PluginDemo.Modules.Inventario/

# Copiar DLL
copy "src/Modules/Inventario/VRM_PluginDemo.Modules.Inventario/bin/Debug/net8.0/VRM_PluginDemo.Modules.Inventario.dll" "src/Host/VRM_PluginDemo.Blazor.Server/Modules/"

# Reiniciar aplicacion
cd src/Host/VRM_PluginDemo.Blazor.Server
dotnet run
```

---

## Estructura de un Modulo

```
VRM_PluginDemo.Modules.Inventario/
??? Components/
?   ??? Inventario.razor       # Componente UI con @page
??? Domain/
?   ??? Producto.cs
?   ??? Almacen.cs
??? Services/
?   ??? IProductoService.cs
?   ??? ProductoService.cs
?   ??? IAlmacenService.cs
?   ??? AlmacenService.cs
??? InventarioModule.cs        # Implementa IModule
```

---

## Ejemplo: Permisos Granulares en Accion

### Escenario: Modulo Finanzas

**Coordinador de Finanzas** (`coordinador.finanzas`):
- Puede ver facturas
- Puede crear facturas
- Puede editar facturas
- **NO puede** eliminar facturas
- **NO puede** timbrar en SAT
- **NO puede** ver reportes sensibles

**Gerente de Finanzas** (`gerente.finanzas`):
- Puede hacer TODO lo del coordinador
- **SI puede** eliminar facturas
- **SI puede** timbrar en SAT
- **SI puede** ver reportes sensibles

### Implementacion en el Componente

```razor
<!-- Todos los del modulo pueden ver -->
<AuthorizeView Roles="Admin,GerenteFinanzas,CoordinadorFinanzas,Contador">
    <Authorized>
        <table><!-- Lista de facturas --></table>
    </Authorized>
</AuthorizeView>

<!-- Solo gerentes y coordinadores pueden crear -->
<AuthorizeView Roles="Admin,GerenteFinanzas,CoordinadorFinanzas">
    <Authorized>
        <button>Nueva Factura</button>
    </Authorized>
</AuthorizeView>

<!-- Solo gerentes pueden timbrar -->
<AuthorizeView Roles="Admin,GerenteFinanzas">
    <Authorized>
        <button>Timbrar SAT</button>
    </Authorized>
</AuthorizeView>

<!-- Solo gerentes ven reportes sensibles -->
<AuthorizeView Roles="Admin,GerenteFinanzas">
    <Authorized>
        <div class="card border-danger">
            <div class="card-header bg-danger text-white">
                Reportes Confidenciales
            </div>
        </div>
    </Authorized>
</AuthorizeView>
```

---

## Tips y Mejores Practicas

### 1. Nombrar Acciones de Forma Descriptiva

```csharp
// BIEN
["Finanzas.Facturas.TimbrarSAT"] = new[] { "Admin", "GerenteFinanzas" }

// MAL
["Finanzas.Accion1"] = new[] { "Admin", "GerenteFinanzas" }
```

### 2. Usar AuthorizeView para UI Adaptativa

```razor
<!-- En lugar de ocultar con CSS, usa AuthorizeView -->
<AuthorizeView Roles="Admin,Gerente">
    <Authorized>
        <button>Accion Sensible</button>
    </Authorized>
</AuthorizeView>
```

### 3. Siempre Incluir Admin

```csharp
// Admin debe tener acceso a todo
["MiModulo.MiAccion"] = new[] { "Admin", "OtrosRoles" }
```

### 4. Documentar Permisos

```csharp
public Dictionary<string, string[]> GetActionPermissions()
{
    return new Dictionary<string, string[]>
    {
        // Ver: Todos los usuarios del modulo
        ["Inventario.Ver"] = new[] { "Admin", "Gerente", "Operador" },
        
        // Crear: Solo gerentes (operadores solo consultan)
        ["Inventario.Crear"] = new[] { "Admin", "Gerente" },
        
        // Reportes valorizados: Solo gerentes (informacion sensible de costos)
        ["Inventario.Reportes.Valorizado"] = new[] { "Admin", "Gerente" }
    };
}
```

---

Volver a [README principal](../../README.md)
