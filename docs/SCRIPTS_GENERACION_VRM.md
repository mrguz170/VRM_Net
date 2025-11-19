# ?? Scripts de Generación VRM - Guía de Uso

## ?? Índice

1. [Scripts Disponibles](#scripts-disponibles)
2. [New-VRMPluginBase.ps1](#new-vrmpluginbaseps1)
3. [Add-VRMComponent.ps1](#add-vrmcomponentps1)
4. [Flujo de Trabajo Recomendado](#flujo-de-trabajo-recomendado)
5. [IDs desde Base de Datos](#ids-desde-base-de-datos)
6. [Ejemplos Completos](#ejemplos-completos)

---

## ?? Scripts Disponibles

| Script | Propósito | Cuándo Usarlo |
|--------|-----------|---------------|
| **New-VRMPluginBase.ps1** | Crea estructura base del módulo | Al iniciar un nuevo módulo |
| **Add-VRMComponent.ps1** | Agrega componentes a módulo existente | Al agregar nuevas funcionalidades |
| ~~New-VRMPlugin.ps1~~ | ? Obsoleto (usa IDs hardcodeados) | **NO USAR** |

---

## ?? New-VRMPluginBase.ps1

### **¿Qué Hace?**

Crea la **estructura base** de un módulo VRM **SIN datos dummy**:

? Proyecto Razor Class Library  
? Carpetas: `Domain/`, `Services/`, `Components/`  
? Clase principal `${ModuleName}Module.cs` con métodos **vacíos**  
? Archivos de configuración (`_Imports.razor`, `.csproj`)  
? Documentación (`README.md`)  

? **NO genera IDs** (deben venir de BD)  
? **NO incluye componentes/acciones dummy**  

---

### **Parámetros**

```powershell
.\New-VRMPluginBase.ps1 `
    -ModuleName <string>    # Nombre del módulo (ej: Inventario)
    -Category <string>      # Categoría (ej: Operaciones)
    -IconRoot <string>      # Icono Remix Icon (ej: ri-box-line)
```

| Parámetro | Tipo | Obligatorio | Descripción |
|-----------|------|-------------|-------------|
| `ModuleName` | string | ? Sí | Nombre del módulo en PascalCase |
| `Category` | string | ? Sí | Categoría para organizar módulos |
| `IconRoot` | string | ? Sí | Icono de RemixIcon para el menú |

---

### **Ejemplo de Uso**

```powershell
# Crear módulo base de Inventario
.\New-VRMPluginBase.ps1 `
    -ModuleName Inventario `
    -Category Operaciones `
    -IconRoot "ri-box-line"
```

**Resultado**:
```
src/Modules/Operaciones/VRM_Plugin.Modules.Inventario/
??? InventarioModule.cs          # Clase principal (métodos vacíos)
??? Domain/                       # Carpeta vacía
??? Services/                     # Carpeta vacía
??? Components/
?   ??? _Imports.razor
??? README.md                     # Documentación con TODOs
```

---

### **Contenido de `InventarioModule.cs`**

```csharp
public class InventarioModule : IModule
{
    // TODO: Este ID se debe obtener de la BD al registrar el módulo
    public int IdModule { get; set; }
    
    public string ModuleName => "Inventario";
    public string DisplayName => "Gestión de Inventario";
    public string Version => "1.0.0";

    public List<ModuleComponent> GetComponents()
    {
        // TODO: Implementar lógica para cargar componentes desde BD
        return new List<ModuleComponent>();
    }

    public List<ModuleAction> GetActions()
    {
        // TODO: Implementar lógica para cargar acciones desde BD
        return new List<ModuleAction>();
    }

    public void ConfigureServices(IServiceCollection services, IConfiguration config)
    {
        // TODO: Registrar servicios del módulo
    }
    
    // ...
}
```

**?? IMPORTANTE**: Los métodos están **vacíos** intencionalmente. Debes implementarlos después de registrar el módulo en BD.

---

## ? Add-VRMComponent.ps1

### **¿Qué Hace?**

Agrega un **nuevo componente** a un módulo existente:

? Componente Blazor (`.razor`)  
? Entidad de dominio (`.cs`) - opcional  
? Interfaz de servicio (`IService.cs`) - opcional  
? Implementación de servicio (`Service.cs`) - opcional  

---

### **Parámetros**

```powershell
.\Add-VRMComponent.ps1 `
    -ModuleName <string>      # Módulo existente
    -ComponentName <string>   # Nombre del nuevo componente
    [-CreateEntity]           # Crear entidad de dominio
    [-CreateService]          # Crear servicio completo
```

| Parámetro | Tipo | Obligatorio | Descripción |
|-----------|------|-------------|-------------|
| `ModuleName` | string | ? Sí | Módulo existente donde agregar |
| `ComponentName` | string | ? Sí | Nombre del nuevo componente |
| `CreateEntity` | switch | ? No | Si se incluye, crea entidad en `Domain/` |
| `CreateService` | switch | ? No | Si se incluye, crea interfaz e implementación |

---

### **Ejemplo 1: Solo Componente UI**

```powershell
# Agregar solo un componente Blazor
.\Add-VRMComponent.ps1 `
    -ModuleName Inventario `
    -ComponentName Productos
```

**Resultado**:
```
Components/Productos.razor   # Componente básico
```

---

### **Ejemplo 2: Componente + Entidad + Servicio**

```powershell
# Agregar componente completo con toda la estructura
.\Add-VRMComponent.ps1 `
    -ModuleName Inventario `
    -ComponentName Productos `
    -CreateEntity `
    -CreateService
```

**Resultado**:
```
Domain/Productos.cs                    # Entidad
Services/IProductosService.cs          # Interfaz
Services/ProductosService.cs           # Implementación
Components/Productos.razor             # UI
```

---

## ?? Flujo de Trabajo Recomendado

### **Escenario: Crear Módulo de Inventario**

#### **Paso 1: Generar Estructura Base**

```powershell
.\New-VRMPluginBase.ps1 `
    -ModuleName Inventario `
    -Category Operaciones `
    -IconRoot "ri-box-line"
```

---

#### **Paso 2: Registrar Módulo en BD**

```sql
-- Insertar módulo y obtener ID
INSERT INTO Modulos (Nombre, Categoria, Activo)
VALUES ('Inventario', 'Operaciones', 1);

-- Obtener el ID asignado
SELECT SCOPE_IDENTITY(); -- Ejemplo: retorna 5
```

---

#### **Paso 3: Actualizar `IdModule`**

```csharp
// InventarioModule.cs
public int IdModule { get; set; } = 5; // ID obtenido de BD
```

---

#### **Paso 4: Agregar Primer Componente**

```powershell
.\Add-VRMComponent.ps1 `
    -ModuleName Inventario `
    -ComponentName Productos `
    -CreateEntity `
    -CreateService
```

---

#### **Paso 5: Registrar Componente en BD**

```sql
-- Insertar componente raíz (categoría)
INSERT INTO ModulosComponentes (IdModulo, IdParent, Nombre, Ruta, Icono)
VALUES (5, NULL, 'Inventario', NULL, 'ri-box-line');
-- Retorna IdComponent = 100

-- Insertar componente funcional
INSERT INTO ModulosComponentes (IdModulo, IdParent, Nombre, Ruta, Icono)
VALUES (5, 100, 'Productos', '/inventario/productos', 'ri-product-hunt-line');
-- Retorna IdComponent = 101
```

---

#### **Paso 6: Actualizar `GetComponents()`**

```csharp
public List<ModuleComponent> GetComponents()
{
    return new List<ModuleComponent>
    {
        // Categoría raíz
        new ModuleComponent 
        { 
            IdComponent = 100,  // ID de BD
            IdModule = 5,       // ID del módulo
            IdParent = null,
            Name = "Inventario",
            Icon = "ri-box-line",
            ShowInMenu = true,
            ComponentType = null  // Solo categoría
        },
        
        // Componente funcional
        new ModuleComponent 
        { 
            IdComponent = 101,  // ID de BD
            IdModule = 5,
            IdParent = 100,     // Hijo de categoría
            Name = "Productos",
            Route = "/inventario/productos",
            Icon = "ri-product-hunt-line",
            ComponentType = typeof(Components.Productos),
            ShowInMenu = true,
            RequiredPermissionIds = new List<int> { 1, 2 }  // Admin, Gerente
        }
    };
}
```

---

#### **Paso 7: Registrar Acciones en BD**

```sql
-- Acción: Ver Productos
INSERT INTO ModulosAcciones (IdComponent, Clave, Nombre, IdTipoAccion)
VALUES (101, 'Inventario.Productos.Ver', 'Ver Productos', 1);
-- Retorna IdAction = 500

-- Acción: Crear Producto
INSERT INTO ModulosAcciones (IdComponent, Clave, Nombre, IdTipoAccion)
VALUES (101, 'Inventario.Productos.Crear', 'Crear Producto', 2);
-- Retorna IdAction = 501
```

---

#### **Paso 8: Actualizar `GetActions()`**

```csharp
public List<ModuleAction> GetActions()
{
    return new List<ModuleAction>
    {
        new ModuleAction 
        { 
            IdAction = 500,
            IdComponent = 101,
            ActionKey = "Inventario.Productos.Ver",
            Name = "Ver Productos",
            IdActionType = 1,  // Lectura
            RequiredPermissionIds = new List<int> { 1, 2, 3 }
        },
        
        new ModuleAction 
        { 
            IdAction = 501,
            IdComponent = 101,
            ActionKey = "Inventario.Productos.Crear",
            Name = "Crear Producto",
            IdActionType = 2,  // Escritura
            RequiredPermissionIds = new List<int> { 1, 2 }
        }
    };
}
```

---

#### **Paso 9: Registrar Servicio**

```csharp
public void ConfigureServices(IServiceCollection services, IConfiguration config)
{
    services.AddScoped<IProductosService, ProductosService>();
}
```

---

#### **Paso 10: Compilar y Probar**

```powershell
# Compilar módulo
cd src\Modules\Operaciones\VRM_Plugin.Modules.Inventario
dotnet build --configuration Release

# Copiar DLL al Host
copy bin\Release\net8.0\VRM_Plugin.Modules.Inventario.dll ^
     ..\..\..\..\Host\VRM_Plugin.Blazor.Server\bin\Debug\net8.0\Modules\

# Ejecutar aplicación
cd ..\..\..\..\Host\VRM_Plugin.Blazor.Server
dotnet run

# Navegar a: http://localhost:5000/inventario/productos
```

---

## ?? IDs desde Base de Datos

### **¿Por Qué NO Usar IDs Hardcodeados?**

| ? Problema con IDs Hardcodeados | ? Solución con IDs de BD |
|----------------------------------|---------------------------|
| Colisiones entre módulos | IDs únicos generados automáticamente |
| Difícil de escalar | Nuevos módulos sin conflictos |
| No configurable por cliente | Configuración flexible en BD |
| Duplicación de código | Single source of truth |

---

### **Tablas de BD Requeridas**

```sql
-- Tabla de módulos
CREATE TABLE Modulos (
    IdModulo INT PRIMARY KEY IDENTITY,
    Nombre NVARCHAR(100) NOT NULL,
    Categoria NVARCHAR(50),
    Activo BIT DEFAULT 1
);

-- Tabla de componentes
CREATE TABLE ModulosComponentes (
    IdComponent INT PRIMARY KEY IDENTITY,
    IdModulo INT FOREIGN KEY REFERENCES Modulos(IdModulo),
    IdParent INT NULL,  -- Para jerarquía
    Nombre NVARCHAR(100),
    Ruta NVARCHAR(200),
    Icono NVARCHAR(50)
);

-- Tabla de acciones
CREATE TABLE ModulosAcciones (
    IdAction INT PRIMARY KEY IDENTITY,
    IdComponent INT FOREIGN KEY REFERENCES ModulosComponentes(IdComponent),
    Clave NVARCHAR(200) UNIQUE,  -- ActionKey
    Nombre NVARCHAR(100),
    IdTipoAccion INT  -- 1=Lectura, 2=Escritura, 3=Crítica
);
```

---

### **Flujo de IDs**

```
1. Desarrollador ejecuta: New-VRMPluginBase.ps1
   ?
2. Se genera estructura base CON TODO VACÍO
   ?
3. Desarrollador registra módulo en BD
   ?
4. BD genera IdModulo automáticamente (ej: 5)
   ?
5. Desarrollador actualiza InventarioModule.cs con IdModulo = 5
   ?
6. Desarrollador ejecuta: Add-VRMComponent.ps1
   ?
7. Se crea componente UI (sin IDs)
   ?
8. Desarrollador registra componente en BD
   ?
9. BD genera IdComponent automáticamente (ej: 101)
   ?
10. Desarrollador actualiza GetComponents() con IdComponent = 101
    ?
11. Desarrollador registra acciones en BD
    ?
12. BD genera IdAction automáticamente (ej: 500, 501)
    ?
13. Desarrollador actualiza GetActions() con IDs de BD
```

---

## ?? Ejemplos Completos

### **Ejemplo 1: Módulo Ventas**

```powershell
# Paso 1: Crear estructura base
.\New-VRMPluginBase.ps1 `
    -ModuleName Ventas `
    -Category Comercial `
    -IconRoot "ri-shopping-cart-line"

# Paso 2: Registrar en BD (obtener IdModulo = 6)

# Paso 3: Agregar componente Clientes
.\Add-VRMComponent.ps1 `
    -ModuleName Ventas `
    -ComponentName Clientes `
    -CreateEntity `
    -CreateService

# Paso 4: Agregar componente Pedidos
.\Add-VRMComponent.ps1 `
    -ModuleName Ventas `
    -ComponentName Pedidos `
    -CreateEntity `
    -CreateService

# Paso 5: Registrar componentes y acciones en BD

# Paso 6: Actualizar VentasModule.cs con IDs de BD

# Paso 7: Compilar y probar
```

---

### **Ejemplo 2: Módulo RRHH**

```powershell
# Crear módulo base
.\New-VRMPluginBase.ps1 `
    -ModuleName RRHH `
    -Category Administracion `
    -IconRoot "ri-team-line"

# Agregar componentes
.\Add-VRMComponent.ps1 -ModuleName RRHH -ComponentName Empleados -CreateEntity -CreateService
.\Add-VRMComponent.ps1 -ModuleName RRHH -ComponentName Nominas -CreateEntity -CreateService
.\Add-VRMComponent.ps1 -ModuleName RRHH -ComponentName Vacaciones -CreateEntity -CreateService
```

---

## ? Checklist de Verificación

Antes de considerar completo un módulo:

- [ ] ? Estructura base generada con `New-VRMPluginBase.ps1`
- [ ] ? Módulo registrado en BD (IdModulo obtenido)
- [ ] ? `IdModule` actualizado en clase principal
- [ ] ? Componentes agregados con `Add-VRMComponent.ps1`
- [ ] ? Entidades de dominio definidas en `Domain/`
- [ ] ? Servicios implementados en `Services/`
- [ ] ? Componentes registrados en BD (IdComponent obtenidos)
- [ ] ? `GetComponents()` implementado con IDs de BD
- [ ] ? Acciones registradas en BD (IdAction obtenidos)
- [ ] ? `GetActions()` implementado con IDs de BD
- [ ] ? Servicios registrados en `ConfigureServices()`
- [ ] ? Módulo compilado sin errores
- [ ] ? DLL copiada al Host
- [ ] ? Módulo probado en aplicación

---

## ?? Comparación de Scripts

| Característica | New-VRMPluginBase.ps1 (NUEVO) | New-VRMPlugin.ps1 (ANTIGUO) |
|----------------|-------------------------------|----------------------------|
| **Genera IDs** | ? No (deben venir de BD) | ? Sí (hardcodeados) |
| **Datos dummy** | ? No | ? Sí (componentes y acciones) |
| **Listo para uso** | ?? Requiere configuración | ? Inmediatamente |
| **Escalabilidad** | ? Excelente | ? Limitada |
| **Colisiones de IDs** | ? Imposibles | ?? Posibles |
| **Recomendado** | ? **SÍ** | ? **NO** |

---

## ?? Soporte

Si tienes dudas:

1. Consulta el `README.md` generado en cada módulo
2. Revisa la documentación en `docs/`
3. Contacta al equipo de desarrollo

---

**Última actualización**: $(Get-Date -Format "yyyy-MM-dd")  
**Versión**: 2.0 - Scripts con IDs de BD
