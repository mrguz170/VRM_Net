# Modules - Módulos de Negocio

Esta carpeta contiene los módulos de negocio que se cargan dinámicamente.

---

## ?? Módulos Disponibles

### 1. Módulo Finanzas

**Ubicación:** `src/Modules/Finanzas/VRM_Plugin.Modules.Finanzas/`

**Propósito:** Gestión de facturas, pagos, conciliaciones y operaciones financieras.

**Ruta Principal:** `/finanzas`

**ID del Módulo:** `1`

**Servicios:**
- `IFacturaService` - Gestión de facturas
- `IPagoService` - Gestión de pagos

**Componentes:**
- **Finanzas (Root)** - Categoría raíz (IdComponent: 1, IdParent: null)
  - **Facturas** - Gestión de facturas (IdComponent: 2, IdParent: 1)
  - **Cobros y Pagos** - Gestión de cobros y pagos (IdComponent: 3, IdParent: 1)

**Permisos Requeridos (IDs):**

| Acción | Tipo | IDs de Permisos |
|--------|------|----------------|
| Ver Facturas | Lectura (1) | 1, 2, 3, 4 |
| Crear Factura | Escritura (2) | 1, 2, 3 |
| Editar Factura | Escritura (2) | 1, 2, 3 |
| Eliminar Factura | Crítica (3) | 1, 2 |
| **Timbrar SAT** | **Crítica (3)** | **1, 2** (Solo Admin y Gerente) |
| **Ver Reportes Confidenciales** | **Lectura (1)** | **1, 2** (Solo Admin y Gerente) |

---

### 2. Módulo Prospectos

**Ubicación:** `src/Modules/Onboarding/VRM_Plugin.Modules.Prospectos/`

**Propósito:** Gestión de onboarding de proveedores potenciales.

**Ruta Principal:** `/prospectos`

**ID del Módulo:** `2`

**Servicios:**
- `IProspectoService` - Gestión de prospectos

**Componentes:**
- **Prospectos** - Componente principal (IdComponent: 4, IdParent: null)

**Permisos Requeridos (IDs):**

| Acción | Tipo | IDs de Permisos |
|--------|------|----------------|
| Ver Prospectos | Lectura (1) | 1, 5, 6, 7, 8, 9 |
| Crear Prospecto | Escritura (2) | 1, 5 |
| Editar Prospecto | Escritura (2) | 1, 5 |
| Eliminar Prospecto | Crítica (3) | 1, 5 |
| **Aprobar Prospecto** | **Crítica (3)** | **1, 5** (Solo Admin y Gestor) |
| **Revisión Legal** | **Escritura (2)** | **1, 7** (Solo Admin y Revisor Legal) |
| **Revisión Financiera** | **Escritura (2)** | **1, 8** (Solo Admin y Revisor Finanzas) |

---

## ??? Cómo Crear un Nuevo Módulo

### Paso 1: Crear Proyecto

```bash
dotnet new razorclasslib -n VRM_Plugin.Modules.Inventario -o src/Modules/Inventario/VRM_Plugin.Modules.Inventario
```

### Paso 2: Agregar Referencias

```xml
<!-- Inventario.csproj -->
<ItemGroup>
  <ProjectReference Include="..\..\..\Core\VRM_Plugin.Core.Abstractions\VRM_Plugin.Core.Abstractions.csproj" />
  <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="8.0.20" />
  <PackageReference Include="Microsoft.AspNetCore.Components.Authorization" Version="8.0.20" />
</ItemGroup>
```

### Paso 3: Implementar IModule

```csharp
using VRM_Plugin.Core.Abstractions;
using VRM_Plugin.Core.Abstractions.Entities;

namespace VRM_Plugin.Modules.Inventario;

public class InventarioModule : IModule
{
    public int IdModule { get; set; } = 3;  // ? ID numérico único
    public string ModuleName => "Inventario";
    public string DisplayName => "Gestión de Inventario";
    public string Description => "Control de productos y almacenes";
    public string Version => "1.0.0";
    
    public List<ModuleComponent> GetComponents()
    {
        return new List<ModuleComponent>
        {
            // Componente raíz (categoría)
            new ModuleComponent
            {
                IdComponent = 100,  // ? ID único
                IdModule = 3,
                IdParent = null,  // ? NULL = Categoría raíz
                ComponentCode = "Inventario.Root",
                Name = "Inventario",
                Route = "",  // Sin ruta, solo contenedor
                Icon = "ri-box-line",
                MenuOrder = 30,
                ShowInMenu = true,
                ComponentType = null,  // Sin componente, solo categoría
                RequiredPermissionIds = new List<int> { 1, 10, 11 },  // ? IDs de permisos
                IsActive = true
            },
            
            // Submódulo: Productos
            new ModuleComponent
            {
                IdComponent = 101,
                IdModule = 3,
                IdParent = 100,  // ? Hijo de Inventario (categoría)
                ComponentCode = "Inventario.Productos",
                Name = "Productos",
                Route = "/inventario/productos",
                Icon = "ri-product-hunt-line",
                MenuOrder = 1,
                ShowInMenu = true,
                ComponentType = typeof(Components.Productos),
                RequiredPermissionIds = new List<int> { 1, 10, 11 },  // Admin, Gerente, Almacenista
                IsActive = true
            },
            
            // Submódulo: Almacenes
            new ModuleComponent
            {
                IdComponent = 102,
                IdModule = 3,
                IdParent = 100,  // ? Hijo de Inventario
                ComponentCode = "Inventario.Almacenes",
                Name = "Almacenes",
                Route = "/inventario/almacenes",
                Icon = "ri-store-line",
                MenuOrder = 2,
                ShowInMenu = true,
                ComponentType = typeof(Components.Almacenes),
                RequiredPermissionIds = new List<int> { 1, 10 },  // Solo Admin y Gerente Inventario
                IsActive = true
            }
        };
    }
    
    public List<ModuleAction> GetActions()
    {
        return new List<ModuleAction>
        {
            // Acciones de Productos
            new ModuleAction 
            { 
                IdAction = 100, 
                IdComponent = 101,
                IdActionType = 1,  // Lectura
                ActionKey = "Inventario.Productos.Ver", 
                Name = "Ver Productos", 
                Description = "Permite visualizar el catálogo de productos",
                RequiredPermissionIds = new List<int> { 1, 10, 11 },  // Admin, Gerente, Almacenista
                IsActive = true 
            },
            
            new ModuleAction 
            { 
                IdAction = 101, 
                IdComponent = 101,
                IdActionType = 2,  // Escritura
                ActionKey = "Inventario.Productos.Crear", 
                Name = "Crear Producto", 
                Description = "Permite agregar nuevos productos",
                RequiredPermissionIds = new List<int> { 1, 10 },  // Solo Admin y Gerente
                IsActive = true 
            },
            
            // Acción crítica: Solo gerentes
            new ModuleAction 
            { 
                IdAction = 102, 
                IdComponent = 101,
                IdActionType = 3,  // Crítica
                ActionKey = "Inventario.Reportes.VerValorizado", 
                Name = "Ver Reporte Valorizado", 
                Description = "Reporte de inventario con costos (información sensible)",
                RequiredPermissionIds = new List<int> { 1, 10 },  // Solo Admin y Gerente
                IsActive = true 
            }
        };
    }
    
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IAlmacenService, AlmacenService>();
    }
    
    public bool IsEnabledForClient(string clienteId) => true;
    
    public async Task OnModuleLoadedAsync()
    {
        Console.WriteLine($"[{ModuleName}] Módulo cargado - IdModule: {IdModule}");
        await Task.CompletedTask;
    }
}
```

### Paso 4: Crear Componente Blazor

```razor
<!-- Components/Productos.razor -->
@page "/inventario/productos"
@rendermode InteractiveServer
@using VRM_Plugin.Blazor.Server.Components.Auth

<PageTitle>Productos</PageTitle>

<!-- ? NUEVO: Proteger con IdComponent -->
<AuthorizeModule IdComponent="101">
    
    <h1>?? Gestión de Productos</h1>

    <div class="card">
        <div class="card-header">
            <h5>Catálogo de Productos</h5>
        </div>
        <div class="card-body">
            <!-- Lista de productos (todos pueden ver) -->
            <table class="table">
                <!-- ... -->
            </table>
            
            <!-- ? Proteger acción específica -->
            <AuthorizeAction ActionKey="Inventario.Productos.Crear">
                <button class="btn btn-primary">
                    Nuevo Producto
                </button>
            </AuthorizeAction>
        </div>
    </div>
    
    <!-- ? Reportes sensibles: Solo gerentes -->
    <AuthorizeAction ActionKey="Inventario.Reportes.VerValorizado">
        <div class="card border-danger mt-3">
            <div class="card-header bg-danger text-white">
                <h5>?? Reportes Valorizados (Confidencial)</h5>
            </div>
            <div class="card-body">
                <button class="btn btn-danger">Ver Inventario Valorizado</button>
                <button class="btn btn-danger">Reporte de Costos</button>
            </div>
        </div>
    </AuthorizeAction>
    
</AuthorizeModule>

@code {
    // Lógica del componente
}
```

### Paso 5: Compilar y Desplegar

```bash
# Compilar
dotnet build src/Modules/Inventario/VRM_Plugin.Modules.Inventario/

# Copiar DLL a carpeta Modules
copy "src/Modules/Inventario/VRM_Plugin.Modules.Inventario/bin/Debug/net8.0/VRM_Plugin.Modules.Inventario.dll" "src/Host/VRM_Plugin.Blazor.Server/Modules/"

# Reiniciar aplicación
cd src/Host/VRM_Plugin.Blazor.Server
dotnet run
```

---

## ?? Estructura de un Módulo

```
VRM_Plugin.Modules.Inventario/
??? Components/
?   ??? Productos.razor           # Componente UI con @page
?   ??? Almacenes.razor
??? Domain/
?   ??? Producto.cs
?   ??? Almacen.cs
??? Services/
?   ??? IProductoService.cs
?   ??? ProductoService.cs
?   ??? IAlmacenService.cs
?   ??? AlmacenService.cs
??? InventarioModule.cs            # Implementa IModule
```

---

## ?? Tips y Mejores Prácticas

### 1. IDs de Permisos Consistentes

```csharp
// ? BIEN: Usar IDs consistentes
RequiredPermissionIds = new List<int> { 1, 10 }  // Admin, Gerente Inventario

// ? MAL: IDs inventados sin estructura
RequiredPermissionIds = new List<int> { 999, 123 }
```

### 2. Jerarquía de Componentes Clara

```csharp
// ? BIEN: Jerarquía de 2-3 niveles
// Nivel 1: Categoría (IdParent = null)
// Nivel 2: Módulos principales (IdParent = IdCategoria)
// Nivel 3: Submódulos (IdParent = IdModuloPrincipal)

// ? MAL: Más de 4 niveles (confuso para usuarios)
```

### 3. Tipos de Acción Apropiados

```csharp
// IdActionType = 1: Lectura (ver, listar, consultar)
new ModuleAction { IdActionType = 1, ActionKey = "Inventario.Productos.Ver" }

// IdActionType = 2: Escritura (crear, editar, actualizar)
new ModuleAction { IdActionType = 2, ActionKey = "Inventario.Productos.Crear" }

// IdActionType = 3: Crítica (eliminar, aprobar, operaciones sensibles)
new ModuleAction { IdActionType = 3, ActionKey = "Inventario.Productos.Eliminar" }
```

### 4. Documentar Permisos en Código

```csharp
public List<ModuleAction> GetActions()
{
    return new List<ModuleAction>
    {
        // Ver: Todos los usuarios del módulo (Admin + Gerente + Almacenista)
        new ModuleAction 
        { 
            ActionKey = "Inventario.Ver", 
            RequiredPermissionIds = new List<int> { 1, 10, 11 } 
        },
        
        // Crear: Solo gerentes (Admin + Gerente)
        // Operadores solo consultan, no pueden crear
        new ModuleAction 
        { 
            ActionKey = "Inventario.Crear", 
            RequiredPermissionIds = new List<int> { 1, 10 } 
        },
        
        // Reportes valorizados: Solo gerentes
        // Información sensible de costos y márgenes
        new ModuleAction 
        { 
            ActionKey = "Inventario.Reportes.Valorizado", 
            RequiredPermissionIds = new List<int> { 1, 10 } 
        }
    };
}
```

### 5. Usar Componentes de Autorización

```razor
<!-- ? BIEN: Usar AuthorizeModule y AuthorizeAction -->
<AuthorizeModule IdComponent="101">
    <h1>Productos</h1>
    
    <AuthorizeAction ActionKey="Inventario.Productos.Crear">
        <button>Nuevo Producto</button>
    </AuthorizeAction>
</AuthorizeModule>

<!-- ? MAL: Usar solo AuthorizeView con roles (menos flexible) -->
<AuthorizeView Roles="Admin,Gerente">
    <!-- ... -->
</AuthorizeView>
```

---

## ?? Mapeo de IDs de Permisos (Ejemplo)

| ID | Permiso | Descripción |
|----|---------|-------------|
| 1 | Admin | Acceso completo al sistema |
| 2 | Gerente Finanzas | Gerente del módulo de finanzas |
| 3 | Coordinador Finanzas | Coordinador de finanzas |
| 4 | Contador | Contador |
| 5 | Gestor Prospectos | Gestor de prospectos |
| 6 | Coordinador Prospectos | Coordinador de prospectos |
| 7 | Revisor Legal | Revisor legal |
| 8 | Revisor Finanzas | Revisor financiero |
| 9 | Revisor Técnico | Revisor técnico |
| 10 | Gerente Inventario | Gerente de inventario |
| 11 | Almacenista | Personal de almacén |

---

Volver a [README principal](../../README.md)
