# ?? Guía de Generadores de Plugins VRM

Esta guía explica cuándo y cómo usar cada script para crear módulos en el sistema VRM.

---

## ?? ¿Qué Script Usar?

### Escenario 1: Módulo con Primer Componente Conocido
**Script:** `New-VRMPlugin.ps1` con `-FirstComponent`

**Cuándo usarlo:**
- Sabes exactamente qué componente necesitas desde el inicio
- Quieres crear el módulo completo en un solo paso
- Ejemplo: Módulo Finanzas con componente Facturas

**Ejemplo:**
```powershell
.\New-VRMPlugin.ps1 -ModuleName Finanzas -IdModule 1 -Category Finanzas `
    -StartIdComponent 1 -StartIdAction 1 -IconRoot "ri-money-dollar-circle-line" `
    -FirstComponent "Facturas"
```

**Resultado:**
```
? FinanzasModule.cs (con IFacturaService registrado)
? Domain/Factura.cs
? Services/IFacturaService.cs
? Services/FacturaService.cs
? Components/Facturas.razor
```

---

### Escenario 2: Módulo Vacío (Agregar Componentes Después)
**Script:** `New-VRMPlugin.ps1` sin `-FirstComponent`

**Cuándo usarlo:**
- No sabes qué componentes necesitarás todavía
- Quieres diseñar la estructura antes de implementar
- Prefieres agregar componentes uno por uno

**Ejemplo:**
```powershell
.\New-VRMPlugin.ps1 -ModuleName Finanzas -IdModule 1 -Category Finanzas `
    -StartIdComponent 1 -StartIdAction 1 -IconRoot "ri-money-dollar-circle-line"
```

**Resultado:**
```
? FinanzasModule.cs (SIN servicios, solo contenedor)
? Carpetas vacías: Domain/, Services/, Components/
```

**Luego agregar componentes:**
```powershell
.\Add-VRMComponent.ps1 -ModuleName Finanzas -ComponentName Facturas -CreateEntity -CreateService
.\Add-VRMComponent.ps1 -ModuleName Finanzas -ComponentName CobrosYPagos -CreateEntity -CreateService
```

---

### Escenario 3: Solo Estructura Base (Alternativa Simplificada)
**Script:** `New-VRMPluginBase.ps1`

**Cuándo usarlo:**
- Quieres solo la estructura mínima
- Alternativa más simple que `New-VRMPlugin.ps1` sin parámetros
- Útil para plantillas o pruebas

**Ejemplo:**
```powershell
.\New-VRMPluginBase.ps1 -ModuleName Inventario -IdModule 3 -Category Operaciones `
    -StartIdComponent 100 -IconRoot "ri-box-line"
```

---

## ?? Comparación de Scripts

| Característica | New-VRMPlugin.ps1<br/>(con -FirstComponent) | New-VRMPlugin.ps1<br/>(sin -FirstComponent) | New-VRMPluginBase.ps1 |
|----------------|---------------------------------------------|---------------------------------------------|----------------------|
| **Crea estructura base** | ? | ? | ? |
| **Crea primer componente** | ? | ? | ? |
| **Crea servicio base módulo** | ? | ? | ? |
| **Crea servicio de componente** | ? | ? | ? |
| **Registra servicio en DI** | ? | ? | ? |
| **Complejidad** | Media | Baja | Muy Baja |
| **Pasos siguientes** | Personalizar | Agregar componentes | Agregar componentes |

---

## ??? Arquitectura de Módulos

### ?? Principio Fundamental: NO Servicios Base

```
? MAL: Servicio del módulo raíz
???????????????????????
?   FinanzasModule    ?
?  IFinanzasService   ?  ? ¿Qué hace? No tiene sentido
???????????????????????

? BIEN: Servicios por componente
???????????????????????
?   FinanzasModule    ?  ? Solo contenedor
?   (sin servicio)    ?
???????????????????????
           ?
      ??????????????????????
      ?         ?          ?
????????????? ???????????? ?????????????
? Facturas  ? ?  Pagos   ? ? Reportes  ?
? IFactura  ? ? IPago    ? ? IReporte  ?
? Service   ? ? Service  ? ? Service   ?
????????????? ???????????? ?????????????
```

### Ejemplo Real: Módulo Finanzas

```csharp
// FinanzasModule.cs
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    // ? CORRECTO: Servicios de componentes hijos
    services.AddScoped<IFacturaService, FacturaService>();
    services.AddScoped<IPagoService, PagoService>();
    services.AddScoped<IConciliacionService, ConciliacionService>();
    
    // ? INCORRECTO: Servicio del módulo raíz
    // services.AddScoped<IFinanzasService, FinanzasService>(); // No tiene sentido
}
```

---

## ?? Flujos de Trabajo Recomendados

### Flujo 1: Rápido (Todo en un paso)
```powershell
# 1. Crear módulo con primer componente
.\New-VRMPlugin.ps1 -ModuleName Ventas -IdModule 4 -Category Comercial `
    -StartIdComponent 200 -StartIdAction 200 -IconRoot "ri-shopping-cart-line" `
    -FirstComponent "Pedidos"

# 2. ¡Listo! Ya puedes ejecutar
cd src\Host\VRM_Plugin.Blazor.Server
dotnet run

# 3. Agregar más componentes después
.\Add-VRMComponent.ps1 -ModuleName Ventas -ComponentName Clientes -CreateEntity -CreateService
```

### Flujo 2: Incremental (Planificación primero)
```powershell
# 1. Crear estructura base
.\New-VRMPlugin.ps1 -ModuleName Inventario -IdModule 3 -Category Operaciones `
    -StartIdComponent 100 -StartIdAction 100 -IconRoot "ri-box-line"

# 2. Planificar componentes (diseño)
# - Productos
# - Almacenes
# - Movimientos
# - Reportes

# 3. Agregar componentes uno por uno
.\Add-VRMComponent.ps1 -ModuleName Inventario -ComponentName Productos -CreateEntity -CreateService
.\Add-VRMComponent.ps1 -ModuleName Inventario -ComponentName Almacenes -CreateEntity -CreateService

# 4. Actualizar InventarioModule.cs manualmente
# - Agregar componentes a GetComponents()
# - Agregar acciones a GetActions()
# - Registrar servicios en ConfigureServices()

# 5. Compilar y probar
cd src\Modules\Operaciones\VRM_Plugin.Modules.Inventario
dotnet build
```

---

## ?? Checklist: Después de Crear el Módulo

### Si usaste `-FirstComponent`:
- [ ] ? Revisar `Domain/[Componente].cs` y agregar propiedades necesarias
- [ ] ? Personalizar `Services/I[Componente]Service.cs` con métodos reales
- [ ] ? Implementar lógica en `Services/[Componente]Service.cs`
- [ ] ? Diseñar UI en `Components/[Componente].razor`
- [ ] ? Compilar y probar
- [ ] ?? Agregar más componentes si es necesario

### Si NO usaste `-FirstComponent`:
- [ ] ?? Agregar al menos un componente funcional:
  ```powershell
  .\Add-VRMComponent.ps1 -ModuleName [Módulo] -ComponentName [Componente] -CreateEntity -CreateService
  ```
- [ ] ? Actualizar `[Módulo]Module.cs`:
  - Agregar componente en `GetComponents()`
  - Agregar acciones en `GetActions()`
  - Registrar servicio en `ConfigureServices()`
- [ ] ? Compilar y probar

---

## ?? Errores Comunes

### Error 1: Servicio no registrado
**Síntoma:** `InvalidOperationException: Unable to resolve service for type 'IFacturaService'`

**Causa:** Olvidaste registrar el servicio en `ConfigureServices()`

**Solución:**
```csharp
// En FinanzasModule.cs
public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
{
    services.AddScoped<IFacturaService, FacturaService>(); // ? Agregar esto
}
```

### Error 2: Componente no aparece en menú
**Síntoma:** El componente no se muestra en el NavMenu

**Causas posibles:**
1. No agregaste el componente a `GetComponents()` en `[Módulo]Module.cs`
2. `ShowInMenu = false`
3. `IdParent` incorrecto (no apunta al componente raíz)
4. `RequiredPermissionIds` no incluye el permiso del usuario actual

**Solución:**
```csharp
// En GetComponents()
new ModuleComponent 
{ 
    IdComponent = 2,
    IdModule = 1,
    IdParent = 1,  // ? Debe apuntar al componente raíz
    ComponentCode = "Finanzas.Facturas",
    Name = "Facturas",
    Route = "/finanzas/facturas",
    ShowInMenu = true,  // ? Debe ser true
    ComponentType = typeof(Components.Facturas),
    RequiredPermissionIds = new List<int> { 1 },  // ? Admin puede ver
    IsActive = true
}
```

### Error 3: Namespace incorrecto
**Síntoma:** `The type or namespace name 'X' could not be found`

**Causa:** El namespace en `_Imports.razor` no coincide con el real

**Solución:**
```razor
@* En Components/_Imports.razor *@
@using VRM_Plugin.Modules.Finanzas.Domain  @* ? Verificar que coincida *@
@using VRM_Plugin.Modules.Finanzas.Services

@namespace VRM_Plugin.Modules.Finanzas.Components  @* ? Verificar que coincida *@
```

---

## ?? Mejores Prácticas

### 1. Nomenclatura Consistente
```
Módulo:     Finanzas
Componente: Facturas
Entidad:    Factura
Servicio:   IFacturaService ? FacturaService
Archivo:    Facturas.razor
```

### 2. IDs Organizados
```
Módulo Finanzas:
  IdModule: 1
  Componentes: 1-10
  Acciones: 1-50

Módulo Prospectos:
  IdModule: 2
  Componentes: 11-20
  Acciones: 51-100

Módulo Inventario:
  IdModule: 3
  Componentes: 100-110
  Acciones: 100-150
```

### 3. Estructura de Carpetas
```
src/Modules/[Categoría]/VRM_Plugin.Modules.[Módulo]/
??? Domain/
?   ??? [Entidad].cs
?   ??? [OtraEntidad].cs
?   ??? Enums.cs
??? Services/
?   ??? I[Componente]Service.cs
?   ??? [Componente]Service.cs
??? Components/
?   ??? [Componente].razor
?   ??? _Imports.razor
??? [Módulo]Module.cs
```

---

## ?? Tips y Trucos

### Tip 1: Crear múltiples componentes rápidamente
```powershell
# Script batch para crear varios componentes
$componentes = @("Productos", "Categorias", "Proveedores", "Movimientos")

foreach ($comp in $componentes) {
    .\Add-VRMComponent.ps1 -ModuleName Inventario -ComponentName $comp -CreateEntity -CreateService
}
```

### Tip 2: Verificar módulo antes de compilar
```powershell
# Ver estructura del módulo
tree src\Modules\Finanzas\VRM_Plugin.Modules.Finanzas /F

# Buscar TODOs pendientes
Get-ChildItem -Path "src\Modules\Finanzas" -Recurse -Include *.cs,*.razor | 
    Select-String -Pattern "TODO" | 
    Format-Table Path, LineNumber, Line -AutoSize
```

### Tip 3: Plantilla para nuevos servicios
```csharp
// Plantilla estándar para servicios
public interface I[Entidad]Service
{
    // Consultas
    Task<List<[Entidad]>> GetAllAsync();
    Task<[Entidad]?> GetByIdAsync(int id);
    
    // Comandos
    Task<[Entidad]> CreateAsync([Entidad] entity);
    Task<[Entidad]> UpdateAsync([Entidad] entity);
    Task<bool> DeleteAsync(int id);
}
```

---

## ?? Recursos Adicionales

- **IModule.cs**: Interfaz base que todos los módulos deben implementar
- **ModuleLoader.cs**: Carga dinámicamente los módulos en el Host
- **Add-VRMComponent.ps1**: Documentación detallada del script

---

**Última actualización:** $(Get-Date -Format "yyyy-MM-dd")
**Versión:** 2.0
