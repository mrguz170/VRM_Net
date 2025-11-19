# ??? Arquitectura Basada en Plugins - VRM System

## ?? Tabla de Contenidos
1. [Introducción](#introducción)
2. [Paradigma de Arquitectura](#paradigma-de-arquitectura)
3. [Estructura de Carpetas](#estructura-de-carpetas)
4. [Componentes Principales](#componentes-principales)
5. [Flujo de Carga de Módulos](#flujo-de-carga-de-módulos)
6. [Ventajas del Sistema](#ventajas-del-sistema)

---

## ?? Introducción

VRM es un sistema empresarial modular construido sobre **Blazor Server (.NET 8)** que utiliza una **arquitectura basada en plugins dinámicos**. Esto permite:

- ? **Extensibilidad**: Agregar nuevos módulos sin modificar el core
- ? **Hot-Deployment**: Desplegar módulos sin recompilar toda la aplicación
- ? **Separación de Responsabilidades**: Cada módulo es independiente
- ? **Permisos Granulares**: Control fino sobre acciones y componentes

---

## ?? Paradigma de Arquitectura

### **Plugin-Based Architecture (Arquitectura Basada en Plugins)**

Este sistema sigue el patrón de **descubrimiento dinámico de módulos** mediante reflexión:

```
???????????????????????????????????????????????????????????
?                    HOST APPLICATION                     ?
?              (VRM_Plugin.Blazor.Server)                 ?
?                                                         ?
?  ????????????????????????????????????????????????????  ?
?  ?          ModuleLoader (IModuleManager)           ?  ?
?  ?  - Descubre DLLs en carpeta "Modules"           ?  ?
?  ?  - Carga ensamblados dinámicamente               ?  ?
?  ?  - Registra componentes Blazor                   ?  ?
?  ?  - Configura servicios de cada módulo            ?  ?
?  ????????????????????????????????????????????????????  ?
?                          ?                              ?
?                          ?                              ?
?                 Carga dinámica                          ?
?                          ?                              ?
???????????????????????????????????????????????????????????
                           ?
       ??????????????????????????????????????????
       ?                                        ?
???????????????????                  ????????????????????
?  Plugin Module  ?                  ?  Plugin Module   ?
?    FINANZAS     ?                  ?   PROSPECTOS     ?
?                 ?                  ?                  ?
? • Facturas      ?                  ? • Onboarding     ?
? • Pagos         ?                  ? • Revisiones     ?
? • Conciliación  ?                  ? • Documentos     ?
???????????????????                  ????????????????????
```

### **Principios Fundamentales**

1. **Descubrimiento Automático**: El sistema escanea la carpeta `Modules/` buscando DLLs que implementen `IModule`
2. **Carga Dinámica**: Los módulos se cargan en tiempo de ejecución usando `Assembly.LoadFrom()`
3. **Registro de Servicios**: Cada módulo registra sus propios servicios DI (`IFacturaService`, etc.)
4. **Routing Dinámico**: Los componentes Blazor se registran automáticamente con sus rutas
5. **Permisos Granulares**: Control por componente (`IdComponent`) y acción (`ActionKey`)

---

## ?? Estructura de Carpetas

```
VRM_Net/
?
??? src/
?   ?
?   ??? Core/                                    # ?? Núcleo del sistema
?   ?   ??? VRM_Plugin.Core.Abstractions/        # Interfaces base
?   ?   ?   ??? IModule.cs                       # ? Interfaz principal de módulo
?   ?   ?   ??? Entities/
?   ?   ?   ?   ??? ModuleComponent.cs           # Entidad para componentes UI
?   ?   ?   ?   ??? ModuleAction.cs              # Entidad para acciones granulares
?   ?   ?   ?   ??? ActionType.cs                # Tipos: Lectura, Escritura, Crítica
?   ?   ?   ??? ...
?   ?   ?
?   ?   ??? VRM_Plugin.Core.Domain/              # Modelos compartidos
?   ?       ??? ConfiguracionFiscal.cs
?   ?       ??? ModuloHabilitado.cs
?   ?
?   ??? Host/                                    # ??? Aplicación host
?   ?   ??? VRM_Plugin.Blazor.Server/
?   ?       ??? Program.cs                       # ? Punto de entrada
?   ?       ??? Services/
?   ?       ?   ??? ModuleLoader.cs              # ? Carga dinámica de módulos
?   ?       ?   ??? IModuleManager.cs
?   ?       ?   ??? ModuleAuthorizationService.cs # Verificación de permisos
?   ?       ?   ??? DummyAuthenticationStateProvider.cs
?   ?       ??? Components/
?   ?       ?   ??? Layout/
?   ?       ?   ?   ??? MainLayout.razor
?   ?       ?   ?   ??? Sidebar.razor            # Menú generado dinámicamente
?   ?       ?   ?   ??? Topbar.razor
?   ?       ?   ??? Auth/
?   ?       ?   ?   ??? AuthorizeModule.razor    # Protección por IdComponent
?   ?       ?   ?   ??? AuthorizeAction.razor    # Protección por ActionKey
?   ?       ?   ??? Pages/
?   ?       ?       ??? Login.razor
?   ?       ?       ??? Index.razor
?   ?       ??? Modules/                         # ? Carpeta de plugins (DLLs)
?   ?           ??? VRM_Plugin.Modules.Finanzas.dll
?   ?           ??? VRM_Plugin.Modules.Prospectos.dll
?   ?
?   ??? Modules/                                 # ?? Módulos de negocio
?       ??? Finanzas/
?       ?   ??? VRM_Plugin.Modules.Finanzas/
?       ?       ??? FinanzasModule.cs            # ? Implementa IModule
?       ?       ??? Components/
?       ?       ?   ??? Facturas.razor           # @page "/finanzas/facturas"
?       ?       ?   ??? CobrosYPagos.razor
?       ?       ??? Domain/
?       ?       ?   ??? Factura.cs
?       ?       ?   ??? Pago.cs
?       ?       ??? Services/
?       ?           ??? IFacturaService.cs
?       ?           ??? FacturaService.cs
?       ?
?       ??? Onboarding/
?           ??? VRM_Plugin.Modules.Prospectos/
?               ??? ProspectosModule.cs
?               ??? Components/
?               ?   ??? Prospectos.razor
?               ??? Domain/
?               ?   ??? Prospecto.cs
?               ??? Services/
?                   ??? IProspectoService.cs
?
??? docs/                                        # ?? Documentación
    ??? ARQUITECTURA_PLUGINS.md                  # Este archivo
    ??? ...
```

### **?? Carpetas Clave**

| Carpeta | Propósito | Contenido |
|---------|-----------|-----------|
| **`Core/`** | Contratos y abstracciones | Interfaces, entidades base |
| **`Host/`** | Aplicación principal | ModuleLoader, autenticación, UI host |
| **`Modules/`** | Plugins de negocio | Cada módulo es un proyecto independiente |
| **`Host/Modules/`** | DLLs compiladas | Carpeta donde se copian los plugins |

---

## ?? Componentes Principales

### **1. IModule - Contrato Base**

Todo módulo debe implementar esta interfaz:

```csharp
public interface IModule
{
    // Identificación
    int IdModule { get; set; }                   // ID numérico único
    string ModuleName { get; }                   // Nombre técnico
    string DisplayName { get; }                  // Nombre para UI
    string Description { get; }                  // Descripción del módulo
    string Version { get; }                      // Versión semántica
    
    // Componentes UI (páginas Blazor)
    List<ModuleComponent> GetComponents();
    
    // Acciones granulares (operaciones específicas)
    List<ModuleAction> GetActions();
    
    // Inyección de dependencias
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    
    // Habilitación condicional (extensible para futura implementación)
    bool IsEnabledForClient(string clienteId);
    
    // Hook de inicialización
    Task OnModuleLoadedAsync();
}
```

### **2. ModuleComponent - Componentes UI**

Representa una página o sección Blazor:

```csharp
public class ModuleComponent
{
    public int IdComponent { get; set; }         // ID único del componente
    public int IdModule { get; set; }            // FK al módulo padre
    public int? IdParent { get; set; }           // Jerarquía (null = raíz)
    
    public string ComponentCode { get; set; }    // Código único
    public string Name { get; set; }             // Nombre para menú
    public string Route { get; set; }            // Ruta Blazor (/finanzas/facturas)
    public string Icon { get; set; }             // Icono RemixIcon
    
    public Type? ComponentType { get; set; }     // Tipo del componente Razor
    public string? ComponentTypeName { get; set; } // Nombre para BD (ej: "Finanzas.Components.Facturas")
    
    public bool ShowInMenu { get; set; }         // Mostrar en sidebar
    public int MenuOrder { get; set; }           // Orden de visualización
    
    public List<int> RequiredPermissionIds { get; set; } // Permisos de navegación
    public bool IsActive { get; set; }           // Soft delete
}
```

### **3. ModuleAction - Acciones Granulares**

Representa operaciones específicas dentro de un componente:

```csharp
public class ModuleAction
{
    public int IdAction { get; set; }            // ID único
    public int? IdComponent { get; set; }        // FK al componente (null = global)
    
    public string ActionKey { get; set; }        // Clave única (ej: "Finanzas.Facturas.TimbrarSAT")
    public string Name { get; set; }             // Nombre descriptivo
    public string Description { get; set; }      // Descripción de la acción
    
    public int IdActionType { get; set; }        // 1=Lectura, 2=Escritura, 3=Crítica
    
    public List<int> RequiredPermissionIds { get; set; } // Permisos para ejecutar
    public bool IsActive { get; set; }           // Soft delete
}
```

### **4. ModuleLoader - Cargador Dinámico**

Servicio que descubre y carga módulos:

```csharp
public class ModuleLoader : IModuleManager
{
    private readonly List<IModule> _loadedModules = new();
    
    public async Task<int> DiscoverAndLoadModulesAsync(string modulesPath)
    {
        // 1. Buscar DLLs en la carpeta
        var dllFiles = Directory.GetFiles(modulesPath, "VRM_Plugin.Modules.*.dll");
        
        foreach (var dllPath in dllFiles)
        {
            // 2. Cargar ensamblado
            var assembly = Assembly.LoadFrom(dllPath);
            
            // 3. Buscar tipos que implementen IModule
            var moduleTypes = assembly.GetTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface);
            
            // 4. Crear instancias
            foreach (var moduleType in moduleTypes)
            {
                var module = (IModule)Activator.CreateInstance(moduleType)!;
                _loadedModules.Add(module);
                await module.OnModuleLoadedAsync();
            }
        }
        
        return _loadedModules.Count;
    }
    
    public IReadOnlyList<IModule> GetAllModules() => _loadedModules.AsReadOnly();
}
```

---

## ?? Flujo de Carga de Módulos

```
???????????????????????????????????????????????????????????????
? 1. INICIO DE APLICACIÓN (Program.cs)                       ?
???????????????????????????????????????????????????????????????
                         ?
                         ?
???????????????????????????????????????????????????????????????
? 2. CREAR ModuleLoader                                       ?
?    var moduleLoader = new ModuleLoader(logger);             ?
?    builder.Services.AddSingleton<IModuleManager>(loader);   ?
???????????????????????????????????????????????????????????????
                         ?
                         ?
???????????????????????????????????????????????????????????????
? 3. DESCUBRIR Y CARGAR MÓDULOS                               ?
?    var modulePath = Path.Combine(baseDir, "Modules");       ?
?    var count = await moduleLoader.DiscoverAndLoadAsync(...);?
?                                                             ?
?    • Escanea archivos "VRM_Plugin.Modules.*.dll"           ?
?    • Carga cada ensamblado con Assembly.LoadFrom()         ?
?    • Busca tipos que implementen IModule                   ?
?    • Crea instancias y llama OnModuleLoadedAsync()         ?
???????????????????????????????????????????????????????????????
                         ?
                         ?
???????????????????????????????????????????????????????????????
? 4. REGISTRAR SERVICIOS DE CADA MÓDULO                       ?
?    foreach (var modulo in moduleLoader.GetAllModules())     ?
?    {                                                        ?
?        modulo.ConfigureServices(builder.Services, config);  ?
?    }                                                        ?
?                                                             ?
?    • Cada módulo registra sus servicios (IFacturaService)  ?
?    • Los servicios están disponibles para DI               ?
???????????????????????????????????????????????????????????????
                         ?
                         ?
???????????????????????????????????????????????????????????????
? 5. REGISTRAR ENSAMBLADOS PARA ROUTING                       ?
?    var assemblies = moduleLoader.GetAllModules()            ?
?        .Select(m => m.GetType().Assembly).ToArray();        ?
?                                                             ?
?    app.MapRazorComponents<App>()                            ?
?       .AddInteractiveServerRenderMode()                     ?
?       .AddAdditionalAssemblies(assemblies);                 ?
?                                                             ?
?    • Registra componentes Razor de cada módulo             ?
?    • Habilita rutas dinámicas (@page "/finanzas/facturas") ?
???????????????????????????????????????????????????????????????
                         ?
                         ?
???????????????????????????????????????????????????????????????
? 6. GENERAR MENÚ DINÁMICO (Sidebar.razor)                   ?
?    @foreach (var module in ModuleManager.GetAllModules())   ?
?    {                                                        ?
?        foreach (var component in module.GetComponents())    ?
?        {                                                    ?
?            if (component.ShowInMenu)                        ?
?            {                                                ?
?                <NavLink href="@component.Route">            ?
?                    @component.Name                          ?
?                </NavLink>                                   ?
?            }                                                ?
?        }                                                    ?
?    }                                                        ?
???????????????????????????????????????????????????????????????
```

---

## ? Ventajas del Sistema

### **1. Extensibilidad sin Modificar el Core**

```csharp
// Agregar nuevo módulo sin tocar código existente
// 1. Compilar módulo
dotnet build src/Modules/Inventario/VRM_Plugin.Modules.Inventario/

// 2. Copiar DLL
copy bin/Debug/net8.0/VRM_Plugin.Modules.Inventario.dll src/Host/.../Modules/

// 3. Reiniciar app ? Módulo cargado automáticamente ?
```

### **2. Separación de Responsabilidades**

Cada módulo es **completamente independiente**:

- ? Tiene su propia carpeta
- ? Define sus propias entidades (Domain/)
- ? Implementa sus propios servicios (Services/)
- ? Crea sus propios componentes UI (Components/)
- ? No depende de otros módulos

### **3. Permisos Granulares**

**Dos niveles de autorización:**

```razor
<!-- Nivel 1: Proteger TODO el componente -->
<AuthorizeModule IdComponent="2">
    <!-- Solo usuarios con permiso al componente pueden ver esto -->
</AuthorizeModule>

<!-- Nivel 2: Proteger ACCIONES específicas -->
<AuthorizeAction ActionKey="Finanzas.Facturas.TimbrarSAT">
    <button>Timbrar SAT (Solo Gerentes)</button>
</AuthorizeAction>
```

### **4. Configuración Flexible**

Cada módulo puede implementar su propia lógica de activación/desactivación según necesidades del negocio.

### **5. Hot-Deployment**

- ? **No requiere recompilar la aplicación completa**
- ? **Agregar/quitar módulos con recarga de aplicación**
- ? **Actualizar módulos individualmente**

---

## ?? Ejemplo Práctico: Módulo Finanzas

```csharp
public class FinanzasModule : IModule
{
    public int IdModule { get; set; } = 1;
    public string ModuleName => "Finanzas";
    public string DisplayName => "Gestión de Finanzas";
    public string Version => "1.0.0";
    
    public List<ModuleComponent> GetComponents()
    {
        return new List<ModuleComponent>
        {
            // Categoría raíz
            new ModuleComponent 
            { 
                IdComponent = 1, 
                IdParent = null,            // NULL = raíz del menú
                Name = "Finanzas", 
                Icon = "ri-money-dollar-circle-line",
                ComponentType = null,       // Solo contenedor
                ShowInMenu = true 
            },
            
            // Submenú: Facturas
            new ModuleComponent 
            { 
                IdComponent = 2, 
                IdParent = 1,               // Hijo de "Finanzas"
                Name = "Facturas", 
                Route = "/finanzas/facturas",
                ComponentType = typeof(Components.Facturas),
                ComponentTypeName = "Finanzas.Components.Facturas",
                ShowInMenu = true 
            }
        };
    }
    
    public List<ModuleAction> GetActions()
    {
        return new List<ModuleAction>
        {
            new ModuleAction 
            { 
                IdAction = 1,
                IdComponent = 2,
                ActionKey = "Finanzas.Facturas.TimbrarSAT",
                IdActionType = 3,           // Crítica
                RequiredPermissionIds = new List<int> { 1, 2 } // Solo Admin y Gerente
            }
        };
    }
    
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IFacturaService, FacturaService>();
    }
}
```

---

## ?? Comparación con Arquitecturas Tradicionales

| Aspecto | Monolítico | Microservicios | **Plugin-Based (VRM)** |
|---------|-----------|----------------|----------------------|
| **Despliegue** | Todo junto | Servicios independientes | **DLLs individuales** |
| **Complejidad** | Baja | Alta (orquestación) | **Media** |
| **Escalabilidad** | Vertical | Horizontal | **Vertical + Modular** |
| **Desarrollo** | Un equipo | Múltiples equipos | **Equipos por módulo** |
| **Testing** | Todo o nada | Por servicio | **Por módulo** |
| **Overhead** | Bajo | Alto (red) | **Bajo (misma app)** |

---

## ?? Conclusión

La arquitectura de plugins de VRM ofrece:

1. ? **Modularidad real**: Cada módulo es independiente
2. ? **Facilidad de mantenimiento**: Cambios aislados por módulo
3. ? **Extensibilidad**: Agregar funcionalidad sin tocar el core
4. ? **Permisos granulares**: Control fino sobre acciones
5. ? **Simplicidad**: Sin complejidad de microservicios

Es ideal para **sistemas empresariales** donde:
- Se necesita personalizar funcionalidad por proyecto
- Los módulos evolucionan independientemente
- Se requiere control granular de permisos
- Se busca simplicidad operacional y mantenibilidad
