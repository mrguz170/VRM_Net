# ??? ARQUITECTURA DE CAPA DE DATOS - VRM_PLUGIN

> **Versión:** 2.0  
> **Fecha:** 2024  
> **Estado:** ? Refactorizado y Validado

---

## ?? TABLA DE CONTENIDOS

1. [Introducción](#introducción)
2. [Principios Fundamentales](#principios-fundamentales)
3. [Estructura de Capas](#estructura-de-capas)
4. [Flujo de Datos Completo](#flujo-de-datos-completo)
5. [Reglas Estrictas de Acceso](#reglas-estrictas-de-acceso)
6. [Implementación Paso a Paso](#implementación-paso-a-paso)
7. [Ejemplos Prácticos](#ejemplos-prácticos)
8. [Anti-Patrones Prohibidos](#anti-patrones-prohibidos)
9. [Checklist de Validación](#checklist-de-validación)
10. [Diagrama Completo](#diagrama-completo)

---

## ?? INTRODUCCIÓN

### Problema Original (Arquitectura Antigua)

```csharp
// ? PROBLEMA: Módulo accediendo directamente a BD
public class FinanzasModule : IModule
{
    private string _connectionString;
    
    public List<ModuleComponent> GetComponents()
    {
        DatabaseHelper db = new DatabaseHelper(_connectionString);  // ? MAL
        var result = db.ExecuteStoredProcedure("sp_get_component");  // ? MAL
        // ...mapeo de DataRow a ModuleComponent
    }
}
```

**Consecuencias:**
- ? Módulos acoplados a la implementación de BD
- ? Imposible cambiar de BD sin recompilar módulos
- ? Difícil de testear (requiere BD real)
- ? Viola principios SOLID (DIP, SRP)
- ? **ROMPE la arquitectura de plugins**

---

### Solución Implementada (Arquitectura Nueva)

```csharp
// ? CORRECTO: Módulo recibe datos inyectados
public class FinanzasModule : IModule
{
    private List<ModuleComponent> _components = new();  // ? Inyectado
    
    public List<ModuleComponent> GetComponents()
    {
        return _components;  // ? Devuelve lo inyectado
    }
    
    public void SetComponents(List<ModuleComponent> components)  // ? NUEVO
    {
        _components = components;  // ? Host inyecta desde BD
    }
}
```

**Beneficios:**
- ? Módulos completamente independientes
- ? Cambiar de BD sin tocar módulos
- ? Fácil de testear (mock de datos)
- ? Cumple SOLID (DIP, SRP, OCP)
- ? **RESPETA la arquitectura de plugins**

---

## ?? PRINCIPIOS FUNDAMENTALES

### 1. Inversión de Dependencias (DIP)

```
? INCORRECTO (Dependencia Directa):
Módulo ? DatabaseHelper ? MySQL

? CORRECTO (Inversión de Dependencias):
Módulo ? IRepository (Abstracción) ? Repository ? DatabaseHelper ? MySQL
```

**Regla de Oro:**  
> **Los módulos NUNCA deben depender de implementaciones concretas de acceso a datos.**

---

### 2. Responsabilidad Única (SRP)

| Capa | Responsabilidad | NO Debe Hacer |
|------|----------------|---------------|
| **Módulo (Plugin)** | Lógica de negocio específica | ? Acceder a BD |
| **Core.Abstractions** | Definir contratos (interfaces) | ? Implementar lógica |
| **VRM_Plugin.Data** | Acceso a datos (implementación) | ? Conocer módulos |
| **Host (Program.cs)** | Orquestar, inyectar dependencias | ? Lógica de negocio |

---

### 3. Abierto/Cerrado (OCP)

```csharp
// ? Agregar nuevo módulo SIN modificar VRM_Plugin.Data
public class NuevoModule : IModule
{
    // ? Usa las mismas interfaces
    // ? Recibe datos inyectados
    // ? NO toca la capa de datos
}
```

---

## ??? ESTRUCTURA DE CAPAS

### Diagrama de Arquitectura

```
???????????????????????????????????????????????????????????????????
?                    HOST (VRM_PluginDemo.Blazor.Server)          ?
?                          Program.cs                              ?
?  - Registra servicios de datos (IUserRepository, etc.)          ?
?  - Carga módulos dinámicamente                                   ?
?  - Inyecta metadata desde BD a módulos                           ?
???????????????????????????????????????????????????????????????????
             ? usa/registra
             ?
???????????????????????????????????????????????????????????????????
?              VRM_PLUGIN.DATA (Capa de Datos)                     ?
?                                                                  ?
?  ?? Repositories/System/                                         ?
?     ??? ModuleMetadataRepository.cs  ? sp_get_module_info       ?
?     ?                                 ? sp_get_component         ?
?     ?                                 ? sp_get_actions           ?
?     ??? UserRepository.cs            ? sp_get_user              ?
?     ??? ... otros repositorios                                   ?
?                                                                  ?
?  ?? Common/                                                      ?
?     ??? DatabaseHelper.cs            ? ExecuteStoredProcedure   ?
?                                                                  ?
?  ? SOLO esta capa accede a BD                                  ?
???????????????????????????????????????????????????????????????????
             ? implementa
             ?
???????????????????????????????????????????????????????????????????
?         CORE.ABSTRACTIONS (Interfaces/Contratos)                 ?
?                                                                  ?
?  ?? Services/                                                    ?
?     ??? IModuleMetadataService.cs  (interface)                  ?
?     ??? IUserRepository.cs         (interface)                  ?
?     ??? ... otras interfaces                                     ?
?                                                                  ?
?  ? Solo define contratos (NO implementa)                       ?
???????????????????????????????????????????????????????????????????
             ? usa (depende de abstracciones)
             ?
???????????????????????????????????????????????????????????????????
?                MÓDULOS (Plugins Independientes)                  ?
?                                                                  ?
?  ?? FinanzasModule                                               ?
?  ?? ProspectosModule                                             ?
?  ?? InventarioModule                                             ?
?                                                                  ?
?  ? Usan interfaces (IFacturaRepository, etc.)                  ?
?  ? Reciben datos inyectados por el Host                        ?
?  ? NUNCA acceden directamente a BD                             ?
???????????????????????????????????????????????????????????????????
```

---

## ?? FLUJO DE DATOS COMPLETO

### Caso de Uso: Cargar Componentes de un Módulo

#### Paso 1: Startup (Program.cs)

```csharp
// src/Host/VRM_PluginDemo.Blazor.Server/Program.cs

// 1?? Registrar servicio de metadata
builder.Services.AddScoped<IModuleMetadataService, ModuleMetadataRepository>();

// 2?? Crear ModuleLoader con metadata service
var moduleLoader = new ModuleLoader(logger, metadataService);

// 3?? Cargar módulos
await moduleLoader.DiscoverAndLoadModulesAsync(modulesPath);
```

---

#### Paso 2: ModuleLoader Inyecta Metadata

```csharp
// src/Host/VRM_PluginDemo.Blazor.Server/Services/ModuleLoader.cs

private async Task LoadModuleMetadataFromDatabase(IModule module)
{
    // 1?? Obtener datos desde BD usando el servicio
    var metadata = _metadataService.GetModuleMetadata(module.IdModule);
    var components = _metadataService.GetComponentsByModuleId(module.IdModule);
    var actions = _metadataService.GetActionsByModuleId(module.IdModule);
    
    // 2?? Inyectar al módulo usando reflexión
    module.GetType().GetMethod("SetMetadata")
        ?.Invoke(module, new object[] { metadata.DisplayName, metadata.Description, metadata.Version });
    
    module.GetType().GetMethod("SetComponents")
        ?.Invoke(module, new object[] { components });
    
    module.GetType().GetMethod("SetActions")
        ?.Invoke(module, new object[] { actions });
}
```

---

#### Paso 3: ModuleMetadataRepository Accede a BD

```csharp
// src/Core/VRM_Plugin.Data/Repositories/System/ModuleMetadataRepository.cs

public class ModuleMetadataRepository : IModuleMetadataService
{
    private readonly DatabaseHelper _db;
    
    public List<ModuleComponent> GetComponentsByModuleId(int moduleId)
    {
        // 1?? ÚNICA capa que accede a BD
        var result = _db.ExecuteStoredProcedure("sp_get_component", 
            new Dictionary<string, object> { { "module_id", moduleId } });
        
        var components = new List<ModuleComponent>();
        
        // 2?? Mapeo de DataRow a objetos del dominio
        foreach (DataRow row in result.Rows)
        {
            components.Add(new ModuleComponent
            {
                IdComponent = Convert.ToInt32(row["component_id"]),
                IdModule = Convert.ToInt32(row["module_id"]),
                // ... resto de propiedades
            });
        }
        
        return components;
    }
}
```

---

#### Paso 4: Módulo Recibe y Devuelve Datos

```csharp
// src/Modules/Finanzas/VRM_PluginDemo.Modules.Finanzas/FinanzasModule.cs

public class FinanzasModule : IModule
{
    private List<ModuleComponent> _components = new();
    
    // ? Método público para obtener componentes
    public List<ModuleComponent> GetComponents()
    {
        return _components.Count > 0 ? _components : GetDefaultComponents();
    }
    
    // ? Método de inyección (llamado por el Host)
    public void SetComponents(List<ModuleComponent> components)
    {
        _components = components ?? new List<ModuleComponent>();
    }
    
    // ? Fallback si no se cargó desde BD
    private List<ModuleComponent> GetDefaultComponents()
    {
        return new List<ModuleComponent>
        {
            new ModuleComponent
            {
                IdComponent = 1,
                IdModule = 1,
                Name = "Finanzas",
                // ... valores por defecto
            }
        };
    }
}
```

---

## ?? REGLAS ESTRICTAS DE ACCESO

### Tabla de Permisos por Capa

| Capa | ? SÍ Puede | ? NO Puede | Razón |
|------|-------------|-------------|-------|
| **Módulos (Plugins)** | - Usar interfaces (`IFacturaRepository`)<br>- Recibir datos inyectados<br>- Registrar servicios de negocio | - Acceder a `DatabaseHelper`<br>- Llamar SPs directamente<br>- Usar `ConnectionString`<br>- Referenciar `VRM_Plugin.Data` | Mantener independencia total |
| **Core.Abstractions** | - Definir interfaces<br>- Definir entidades compartidas<br>- Definir DTOs | - Implementar lógica<br>- Acceder a BD<br>- Conocer implementaciones | Solo contratos, no implementaciones |
| **VRM_Plugin.Data** | - Implementar interfaces<br>- Usar `DatabaseHelper`<br>- Llamar SPs<br>- Mapear `DataRow` a objetos | - Conocer módulos específicos<br>- Contener lógica de negocio | Responsabilidad única: acceso a datos |
| **Host (Program.cs)** | - Registrar servicios<br>- Inyectar dependencias<br>- Orquestar carga de módulos | - Implementar lógica de negocio<br>- Mapear datos | Orquestador, no implementador |

---

### Matriz de Dependencias Permitidas

```
                   ?????????????????????????????????????????????????????????????
                   ?   Módulos    ? Abstractions ?     Data     ?     Host     ?
????????????????????????????????????????????????????????????????????????????????
?   Módulos        ?      -       ?      ?      ?      ?      ?      ?      ?
????????????????????????????????????????????????????????????????????????????????
? Core.Abstractions?      ?      ?      -       ?      ?      ?      ?      ?
????????????????????????????????????????????????????????????????????????????????
? VRM_Plugin.Data  ?      ?      ?      ?      ?      -       ?      ?      ?
????????????????????????????????????????????????????????????????????????????????
?      Host        ?      ?      ?      ?      ?      ?      ?      -       ?
????????????????????????????????????????????????????????????????????????????????
```

**Leyenda:**
- ? = Dependencia permitida
- ? = Dependencia PROHIBIDA
- `-` = No aplica (misma capa)

---

## ??? IMPLEMENTACIÓN PASO A PASO

### Caso Real: Agregar Repositorio de Productos

#### Paso 1: Crear Interfaz en Core.Abstractions

```csharp
// src/Core/VRM_Plugin.Core.Abstractions/Data/IProductoRepository.cs

namespace VRM_Plugin.Core.Abstractions.Data;

/// <summary>
/// ? Interfaz (contrato) para acceso a datos de productos
/// </summary>
public interface IProductoRepository
{
    Task<List<Producto>> GetAllAsync();
    Task<Producto?> GetByIdAsync(int id);
    Task<int> CreateAsync(Producto producto);
    Task<bool> UpdateAsync(Producto producto);
    Task<bool> DeleteAsync(int id);
}
```

---

#### Paso 2: Crear Entidad en Core.Domain (si es compartida) o en Módulo (si es específica)

```csharp
// src/Modules/Inventario/Domain/Producto.cs  ? Específica del módulo

namespace VRM_Plugin.Modules.Inventario.Domain;

/// <summary>
/// ? Entidad específica del módulo Inventario
/// </summary>
public class Producto
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Stock { get; set; }
    // ... propiedades
}
```

---

#### Paso 3: Implementar Repositorio en VRM_Plugin.Data

```csharp
// src/Core/VRM_Plugin.Data/Repositories/ProductoRepository.cs

using VRM_Plugin.Core.Abstractions.Data;
using VRM_Plugin.Modules.Inventario.Domain;
using VRM_Plugin.Data.Common;

namespace VRM_Plugin.Data.Repositories;

/// <summary>
/// ? Implementación que accede a BD
/// </summary>
public class ProductoRepository : IProductoRepository
{
    private readonly DatabaseHelper _db;
    private readonly ILogger<ProductoRepository> _logger;
    
    public ProductoRepository(
        IConfiguration configuration,
        ILogger<ProductoRepository> logger)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _db = new DatabaseHelper(connectionString);
        _logger = logger;
    }
    
    public async Task<List<Producto>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Obteniendo todos los productos desde sp_productos_getall");
            
            // ? ÚNICA capa que llama SPs
            var result = _db.ExecuteStoredProcedure("sp_productos_getall");
            
            var productos = new List<Producto>();
            
            // ? Mapeo de DataRow a entidad
            foreach (DataRow row in result.Rows)
            {
                productos.Add(new Producto
                {
                    Id = Convert.ToInt32(row["id"]),
                    Codigo = row["codigo"]?.ToString() ?? string.Empty,
                    Nombre = row["nombre"]?.ToString() ?? string.Empty,
                    PrecioUnitario = Convert.ToDecimal(row["precio_unitario"]),
                    Stock = Convert.ToInt32(row["stock"])
                });
            }
            
            _logger.LogInformation("Obtenidos {Count} productos", productos.Count);
            
            return await Task.FromResult(productos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos");
            return new List<Producto>();
        }
    }
    
    public async Task<int> CreateAsync(Producto producto)
    {
        try
        {
            _logger.LogDebug("Creando producto: {Nombre}", producto.Nombre);
            
            var parameters = new Dictionary<string, object>
            {
                { "codigo", producto.Codigo },
                { "nombre", producto.Nombre },
                { "precio_unitario", producto.PrecioUnitario },
                { "stock", producto.Stock }
            };
            
            var result = _db.ExecuteStoredProcedure("sp_productos_create", parameters);
            
            if (result.Rows.Count > 0)
            {
                var id = Convert.ToInt32(result.Rows[0]["id"]);
                _logger.LogInformation("Producto creado con ID: {Id}", id);
                return id;
            }
            
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producto");
            return 0;
        }
    }
    
    // ... resto de métodos
}
```

---

#### Paso 4: Registrar en Program.cs

```csharp
// src/Host/VRM_PluginDemo.Blazor.Server/Program.cs

// ==================== SERVICIOS DE DATOS ====================

// ? Registrar repositorio
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();

Log.Information("? ProductoRepository registrado");
```

---

#### Paso 5: Usar en el Módulo (Servicio de Negocio)

```csharp
// src/Modules/Inventario/Services/ProductoService.cs

using VRM_Plugin.Core.Abstractions.Data;
using VRM_Plugin.Modules.Inventario.Domain;

namespace VRM_Plugin.Modules.Inventario.Services;

/// <summary>
/// ? Servicio de negocio que USA el repositorio
/// </summary>
public class ProductoService : IProductoService
{
    private readonly IProductoRepository _repository;  // ? Usa interfaz
    private readonly ILogger<ProductoService> _logger;
    
    public ProductoService(
        IProductoRepository repository,
        ILogger<ProductoService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<List<Producto>> GetProductosDisponiblesAsync()
    {
        _logger.LogDebug("Obteniendo productos disponibles");
        
        // ? Usa el repositorio inyectado
        var productos = await _repository.GetAllAsync();
        
        // ? Lógica de negocio (filtrar productos con stock)
        return productos.Where(p => p.Stock > 0).ToList();
    }
    
    public async Task<bool> ValidarStockAsync(int productoId, int cantidadRequerida)
    {
        var producto = await _repository.GetByIdAsync(productoId);
        
        if (producto == null)
        {
            _logger.LogWarning("Producto no encontrado: {ProductoId}", productoId);
            return false;
        }
        
        // ? Lógica de negocio
        return producto.Stock >= cantidadRequerida;
    }
    
    // ... resto de métodos con lógica de negocio
}
```

---

#### Paso 6: Registrar Servicio en el Módulo

```csharp
// src/Modules/Inventario/InventarioModule.cs

public class InventarioModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // ? El módulo solo registra sus servicios de negocio
        services.AddScoped<IProductoService, ProductoService>();
        
        // ? NO registrar repositorios aquí (lo hace el Host)
    }
}
```

---

## ?? EJEMPLOS PRÁCTICOS

### Ejemplo 1: Autenticación de Usuarios

#### ? INCORRECTO (Antigua Arquitectura)

```csharp
// DummyAuthenticationStateProvider.cs

public class DummyAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IConfiguration _configuration;
    
    private User? GetDummyUserByEmail(string login, string password)
    {
        // ? Acceso directo a BD
        DatabaseHelper db = new DatabaseHelper(_configuration.GetConnectionString("DefaultConnection"));
        
        // ? Llama SP directamente
        var result = db.ExecuteStoredProcedure("sp_get_user", new Dictionary<string, object>
        {
            { "login", login }
        });
        
        // ? Mapeo en el provider
        if (result.Rows.Count > 0)
        {
            var row = result.Rows[0];
            return new User
            {
                Id = row["user_id"].ToString(),
                Username = row["user_name"].ToString(),
                // ...
            };
        }
        
        return null;
    }
}
```

---

#### ? CORRECTO (Nueva Arquitectura)

**1. Interfaz:**

```csharp
// src/Core/VRM_Plugin.Core.Abstractions/Services/IUserRepository.cs

public interface IUserRepository
{
    User? GetUserByLogin(string login, string password);
}

public class User
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
}
```

**2. Implementación:**

```csharp
// src/Core/VRM_Plugin.Data/Repositories/System/UserRepository.cs

public class UserRepository : IUserRepository
{
    private readonly DatabaseHelper _db;
    private readonly ILogger<UserRepository> _logger;
    
    public UserRepository(IConfiguration configuration, ILogger<UserRepository> logger)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _db = new DatabaseHelper(connectionString);
        _logger = logger;
    }
    
    public User? GetUserByLogin(string login, string password)
    {
        try
        {
            var result = _db.ExecuteStoredProcedure("sp_get_user", 
                new Dictionary<string, object> { { "login", login } });
            
            if (result.Rows.Count == 0)
                return null;
            
            var row = result.Rows[0];
            var hashedPassword = row["password"]?.ToString() ?? string.Empty;
            
            if (!PasswordHasher.VerifyPassword(password, hashedPassword))
                return null;
            
            return new User
            {
                Id = row["user_id"]?.ToString() ?? string.Empty,
                Username = row["user_name"]?.ToString() ?? string.Empty,
                Email = row["email"]?.ToString() ?? string.Empty,
                Roles = row["role"]?.ToString() ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario");
            return null;
        }
    }
}
```

**3. Uso en Provider:**

```csharp
// src/Host/VRM_PluginDemo.Blazor.Server/Services/VRMAuthenticationStateProvider.cs

public class VRMAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IUserRepository _userRepository;  // ? Inyectado
    
    public VRMAuthenticationStateProvider(
        ILogger<VRMAuthenticationStateProvider> logger,
        IHttpContextAccessor httpContextAccessor,
        PersistentComponentState persistentState,
        IUserRepository userRepository)  // ? Inyección de dependencias
    {
        _userRepository = userRepository;
        // ...
    }
    
    public async Task<bool> LoginAsync(string username, string password)
    {
        // ? Usa el repositorio inyectado
        var usuario = _userRepository.GetUserByLogin(username, password);
        
        if (usuario == null)
            return false;
        
        // ... crear claims y cookie
        return true;
    }
}
```

**4. Registro:**

```csharp
// Program.cs

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<VRMAuthenticationStateProvider>();
```

---

### Ejemplo 2: Metadata de Módulos

#### ? Flujo Completo

**1. Interfaz:**

```csharp
// src/Core/VRM_Plugin.Core.Abstractions/Services/IModuleMetadataService.cs

public interface IModuleMetadataService
{
    ModuleMetadata GetModuleMetadata(int moduleId);
    List<ModuleComponent> GetComponentsByModuleId(int moduleId);
    List<ModuleAction> GetActionsByModuleId(int moduleId);
}

public class ModuleMetadata
{
    public string ModuleName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
}
```

**2. Implementación:**

```csharp
// src/Core/VRM_Plugin.Data/Repositories/System/ModuleMetadataRepository.cs

public class ModuleMetadataRepository : IModuleMetadataService
{
    private readonly DatabaseHelper _db;
    private readonly ILogger<ModuleMetadataRepository> _logger;
    
    public ModuleMetadataRepository(
        IConfiguration configuration,
        ILogger<ModuleMetadataRepository> logger)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _db = new DatabaseHelper(connectionString);
        _logger = logger;
    }
    
    public ModuleMetadata GetModuleMetadata(int moduleId)
    {
        var result = _db.ExecuteStoredProcedure("sp_get_module_info",
            new Dictionary<string, object> { { "module_id", moduleId } });
        
        if (result.Rows.Count == 0)
            return new ModuleMetadata();
        
        var row = result.Rows[0];
        
        return new ModuleMetadata
        {
            ModuleName = row["module_name"]?.ToString() ?? string.Empty,
            DisplayName = row["display_name"]?.ToString() ?? string.Empty,
            Description = row["description"]?.ToString() ?? string.Empty,
            Version = row["version"]?.ToString() ?? "1.0.0"
        };
    }
    
    public List<ModuleComponent> GetComponentsByModuleId(int moduleId)
    {
        var result = _db.ExecuteStoredProcedure("sp_get_component",
            new Dictionary<string, object> { { "module_id", moduleId } });
        
        var components = new List<ModuleComponent>();
        
        foreach (DataRow row in result.Rows)
        {
            var rolesString = row["roles"]?.ToString() ?? string.Empty;
            var rolesList = string.IsNullOrWhiteSpace(rolesString)
                ? new List<int>()
                : rolesString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(int.Parse)
                             .ToList();
            
            components.Add(new ModuleComponent
            {
                IdComponent = Convert.ToInt32(row["component_id"]),
                IdModule = Convert.ToInt32(row["module_id"]),
                IdParent = row["parent_id"] != DBNull.Value ? Convert.ToInt32(row["parent_id"]) : null,
                Name = row["component_name"]?.ToString() ?? string.Empty,
                Description = row["description"]?.ToString() ?? string.Empty,
                Route = row["route"]?.ToString() ?? string.Empty,
                Icon = row["icon"]?.ToString() ?? string.Empty,
                MenuOrder = Convert.ToInt32(row["menu_order"]),
                ShowInMenu = Convert.ToBoolean(row["show_in_menu"]),
                RequiredPermissionIds = rolesList,
                IsActive = Convert.ToBoolean(row["is_active"])
            });
        }
        
        return components;
    }
    
    // ... GetActionsByModuleId similar
}
```

**3. Uso en ModuleLoader:**

```csharp
// src/Host/VRM_PluginDemo.Blazor.Server/Services/ModuleLoader.cs

public class ModuleLoader : IModuleManager
{
    private readonly IModuleMetadataService _metadataService;
    
    public ModuleLoader(
        ILogger<ModuleLoader> logger,
        IModuleMetadataService metadataService)
    {
        _logger = logger;
        _metadataService = metadataService;
    }
    
    private async Task LoadModuleMetadataFromDatabase(IModule module)
    {
        // ? Usa el servicio inyectado
        var metadata = _metadataService.GetModuleMetadata(module.IdModule);
        var components = _metadataService.GetComponentsByModuleId(module.IdModule);
        var actions = _metadataService.GetActionsByModuleId(module.IdModule);
        
        // ? Inyecta al módulo
        module.GetType().GetMethod("SetMetadata")
            ?.Invoke(module, new object[] { metadata.DisplayName, metadata.Description, metadata.Version });
        
        module.GetType().GetMethod("SetComponents")
            ?.Invoke(module, new object[] { components });
        
        module.GetType().GetMethod("SetActions")
            ?.Invoke(module, new object[] { actions });
    }
}
```

**4. Módulo recibe datos:**

```csharp
// src/Modules/Finanzas/FinanzasModule.cs

public class FinanzasModule : IModule
{
    private List<ModuleComponent> _components = new();
    private List<ModuleAction> _actions = new();
    private string _displayName = "Gestión de Finanzas";
    
    // ? Métodos de inyección
    public void SetComponents(List<ModuleComponent> components)
    {
        _components = components ?? new List<ModuleComponent>();
    }
    
    public void SetActions(List<ModuleAction> actions)
    {
        _actions = actions ?? new List<ModuleAction>();
    }
    
    public void SetMetadata(string displayName, string description, string version)
    {
        _displayName = displayName ?? _displayName;
        _description = description ?? _description;
        _version = version ?? _version;
    }
    
    // ? Métodos públicos devuelven datos inyectados
    public List<ModuleComponent> GetComponents()
    {
        return _components.Count > 0 ? _components : GetDefaultComponents();
    }
    
    public List<ModuleAction> GetActions()
    {
        return _actions.Count > 0 ? _actions : GetDefaultActions();
    }
}
```

---

## ?? ANTI-PATRONES PROHIBIDOS

### ? Anti-Patrón 1: Módulo Accede Directamente a BD

```csharp
// ? NUNCA HACER ESTO

public class MiModule : IModule
{
    private readonly IConfiguration _configuration;
    
    public List<ModuleComponent> GetComponents()
    {
        // ? Acceso directo a DatabaseHelper
        var db = new DatabaseHelper(_configuration.GetConnectionString("DefaultConnection"));
        
        // ? Llama SP desde el módulo
        var result = db.ExecuteStoredProcedure("sp_get_component");
        
        // ? Mapeo en el módulo
        var components = new List<ModuleComponent>();
        foreach (DataRow row in result.Rows)
        {
            components.Add(MapComponent(row));
        }
        
        return components;
    }
}
```

**Por qué está mal:**
1. Módulo acoplado a la implementación de BD
2. Difícil de testear (requiere BD real)
3. Imposible cambiar de BD sin recompilar
4. Viola principios SOLID
5. Rompe la arquitectura de plugins

---

### ? Anti-Patrón 2: Módulo Referencia VRM_Plugin.Data

```xml
<!-- ? NUNCA HACER ESTO -->

<!-- MiModulo.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <ItemGroup>
    <!-- ? Referencia a la capa de datos -->
    <ProjectReference Include="..\..\..\Core\VRM_Plugin.Data\VRM_Plugin.Data.csproj" />
  </ItemGroup>
</Project>
```

**Por qué está mal:**
1. Crea dependencia circular
2. Acopla el módulo a la implementación
3. Rompe la inversión de dependencias

**Solución:**

```xml
<!-- ? CORRECTO -->

<!-- MiModulo.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <ItemGroup>
    <!-- ? SOLO referencia a abstracciones -->
    <ProjectReference Include="..\..\..\Core\VRM_Plugin.Core.Abstractions\VRM_Plugin.Core.Abstractions.csproj" />
  </ItemGroup>
</Project>
```

---

### ? Anti-Patrón 3: Módulo Usa `using VRM_Plugin.Data`

```csharp
// ? NUNCA HACER ESTO

using VRM_Plugin.Data;  // ? Referencia a capa de datos

public class MiModule : IModule
{
    private readonly string _connectionString;
    
    public void AlgunaOperacion()
    {
        var db = new DatabaseHelper(_connectionString);  // ?
        // ...
    }
}
```

**Solución:**

```csharp
// ? CORRECTO

using VRM_Plugin.Core.Abstractions.Data;  // ? Solo abstracciones

public class MiService : IMiService
{
    private readonly IMiRepository _repository;  // ? Interfaz
    
    public MiService(IMiRepository repository)
    {
        _repository = repository;
    }
    
    public async Task<List<MiEntidad>> GetDataAsync()
    {
        return await _repository.GetAllAsync();  // ?
    }
}
```

---

### ? Anti-Patrón 4: Módulo Gestiona ConnectionString

```csharp
// ? NUNCA HACER ESTO

public class MiModule : IModule
{
    private string? _connectionString;  // ?
    
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // ? Módulo maneja ConnectionString
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }
}
```

**Solución:**

```csharp
// ? CORRECTO

public class MiModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // ? Solo registra servicios de negocio
        services.AddScoped<IMiService, MiService>();
        
        // ? NO registrar repositorios aquí (lo hace el Host)
    }
}
```

---

### ? Anti-Patrón 5: Repositorio en el Módulo

```csharp
// ? NUNCA HACER ESTO

// src/Modules/MiModulo/Repositories/MiRepository.cs  ? ? Ubicación incorrecta

public class MiRepository : IMiRepository
{
    private readonly DatabaseHelper _db;
    
    public MiRepository(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        _db = new DatabaseHelper(connectionString);
    }
    
    public List<MiEntidad> GetAll()
    {
        var result = _db.ExecuteStoredProcedure("sp_mi_entidad_getall");
        // ...
    }
}
```

**Solución:**

```csharp
// ? CORRECTO

// src/Core/VRM_Plugin.Data/Repositories/MiRepository.cs  ? ? Ubicación correcta

public class MiRepository : IMiRepository
{
    private readonly DatabaseHelper _db;
    private readonly ILogger<MiRepository> _logger;
    
    public MiRepository(
        IConfiguration configuration,
        ILogger<MiRepository> logger)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;
        _db = new DatabaseHelper(connectionString);
        _logger = logger;
    }
    
    public async Task<List<MiEntidad>> GetAllAsync()
    {
        try
        {
            _logger.LogDebug("Obteniendo entidades desde sp_mi_entidad_getall");
            
            var result = _db.ExecuteStoredProcedure("sp_mi_entidad_getall");
            
            var entidades = new List<MiEntidad>();
            
            foreach (DataRow row in result.Rows)
            {
                entidades.Add(new MiEntidad
                {
                    Id = Convert.ToInt32(row["id"]),
                    // ... mapeo
                });
            }
            
            _logger.LogInformation("Obtenidas {Count} entidades", entidades.Count);
            
            return await Task.FromResult(entidades);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener entidades");
            return new List<MiEntidad>();
        }
    }
}
```

---

## ? CHECKLIST DE VALIDACIÓN

### Para Desarrolladores

Antes de hacer commit, verifica:

#### ? Módulos (Plugins)

```markdown
[ ] ? El módulo NO referencia `VRM_Plugin.Data`
[ ] ? El módulo NO usa `using VRM_Plugin.Data`
[ ] ? El módulo NO instancia `DatabaseHelper`
[ ] ? El módulo NO llama `ExecuteStoredProcedure`
[ ] ? El módulo NO accede a `ConnectionString`
[ ] ? El módulo tiene métodos `Set*()` para inyección
[ ] ? Los servicios del módulo usan interfaces (`I*Repository`)
[ ] ? El `ConfigureServices()` solo registra servicios de negocio
[ ] ? El `.csproj` solo referencia `Core.Abstractions`
```

#### ? Core.Abstractions

```markdown
[ ] ? Solo contiene interfaces y DTOs
[ ] ? NO tiene implementaciones de lógica
[ ] ? NO referencia `VRM_Plugin.Data`
[ ] ? NO tiene acceso a BD
[ ] ? Las interfaces son genéricas (no específicas de módulos)
```

#### ? VRM_Plugin.Data

```markdown
[ ] ? Implementa interfaces de `Core.Abstractions`
[ ] ? Es la ÚNICA capa que usa `DatabaseHelper`
[ ] ? Es la ÚNICA capa que llama SPs
[ ] ? NO referencia módulos específicos
[ ] ? Usa logging para auditoría
[ ] ? Maneja excepciones correctamente
[ ] ? Los repositorios están en `Repositories/` o `Repositories/System/`
```

#### ? Host (Program.cs)

```markdown
[ ] ? Registra todos los repositorios (`I*Repository` ? `*Repository`)
[ ] ? Crea `ModuleLoader` con `IModuleMetadataService`
[ ] ? Inyecta metadata a los módulos
[ ] ? NO contiene lógica de negocio
[ ] ? NO mapea datos (eso es responsabilidad de repositorios)
```

---

### Script de Validación Automática

```powershell
# Validate-PluginArchitecture.ps1

param(
    [string]$WorkspaceRoot = "."
)

$errors = @()

Write-Host "?? Validando arquitectura de plugins..." -ForegroundColor Cyan

# 1. Verificar que módulos NO referencien VRM_Plugin.Data
$modulosPath = Join-Path $WorkspaceRoot "src\Modules"
$csprojFiles = Get-ChildItem -Path $modulosPath -Recurse -Filter "*.csproj"

foreach ($csproj in $csprojFiles) {
    $content = Get-Content $csproj.FullName -Raw
    
    if ($content -match "VRM_Plugin\.Data") {
        $errors += "? $($csproj.Name) referencia VRM_Plugin.Data (PROHIBIDO)"
    }
}

# 2. Verificar que módulos NO usen DatabaseHelper
$csFiles = Get-ChildItem -Path $modulosPath -Recurse -Filter "*.cs"

foreach ($file in $csFiles) {
    $content = Get-Content $file.FullName -Raw
    
    if ($content -match "new DatabaseHelper") {
        $errors += "? $($file.Name) instancia DatabaseHelper (PROHIBIDO)"
    }
    
    if ($content -match "ExecuteStoredProcedure") {
        $errors += "? $($file.Name) llama ExecuteStoredProcedure (PROHIBIDO)"
    }
    
    if ($content -match "GetConnectionString") {
        $errors += "? $($file.Name) accede a ConnectionString (PROHIBIDO)"
    }
}

# 3. Verificar que módulos tengan métodos de inyección
$moduleFiles = Get-ChildItem -Path $modulosPath -Recurse -Filter "*Module.cs"

foreach ($file in $moduleFiles) {
    $content = Get-Content $file.FullName -Raw
    
    if ($content -notmatch "void SetComponents\(List<ModuleComponent>") {
        $errors += "?? $($file.Name) no tiene SetComponents() (RECOMENDADO)"
    }
    
    if ($content -notmatch "void SetActions\(List<ModuleAction>") {
        $errors += "?? $($file.Name) no tiene SetActions() (RECOMENDADO)"
    }
}

# 4. Mostrar resultados
if ($errors.Count -eq 0) {
    Write-Host "`n? VALIDACIÓN EXITOSA: La arquitectura cumple con todos los principios" -ForegroundColor Green
} else {
    Write-Host "`n? ERRORES ENCONTRADOS:" -ForegroundColor Red
    foreach ($error in $errors) {
        Write-Host "  $error" -ForegroundColor Red
    }
    exit 1
}
```

**Uso:**

```powershell
.\Validate-PluginArchitecture.ps1 -WorkspaceRoot "C:\Users\...\VRM_Net"
```

---

## ?? DIAGRAMA COMPLETO DE FLUJO

```
??????????????????????????????????????????????????????????????????????????????
?                         1. STARTUP (Program.cs)                             ?
?                                                                              ?
?  builder.Services.AddScoped<IModuleMetadataService, ModuleMetadataRepository>();?
?  builder.Services.AddScoped<IUserRepository, UserRepository>();            ?
?  builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();      ?
?                                                                              ?
?  var moduleLoader = new ModuleLoader(logger, metadataService);              ?
?  await moduleLoader.DiscoverAndLoadModulesAsync(modulesPath);               ?
??????????????????????????????????????????????????????????????????????????????
             ?
             ?
??????????????????????????????????????????????????????????????????????????????
?                    2. MODULE LOADER (Inyección de Metadata)                 ?
?                                                                              ?
?  foreach (var module in loadedModules)                                      ?
?  {                                                                           ?
?      var metadata = _metadataService.GetModuleMetadata(module.IdModule);    ?
?      var components = _metadataService.GetComponentsByModuleId(...);        ?
?      var actions = _metadataService.GetActionsByModuleId(...);              ?
?                                                                              ?
?      module.SetMetadata(metadata.DisplayName, ...);                         ?
?      module.SetComponents(components);                                      ?
?      module.SetActions(actions);                                            ?
?  }                                                                           ?
??????????????????????????????????????????????????????????????????????????????
             ?
             ?
??????????????????????????????????????????????????????????????????????????????
?              3. REPOSITORY (VRM_Plugin.Data - Acceso a BD)                  ?
?                                                                              ?
?  public List<ModuleComponent> GetComponentsByModuleId(int moduleId)         ?
?  {                                                                           ?
?      // ? ÚNICA capa que accede a BD                                       ?
?      var result = _db.ExecuteStoredProcedure("sp_get_component",            ?
?          new Dictionary<string, object> { { "module_id", moduleId } });     ?
?                                                                              ?
?      var components = new List<ModuleComponent>();                          ?
?                                                                              ?
?      // ? Mapeo de DataRow a objetos                                       ?
?      foreach (DataRow row in result.Rows)                                   ?
?      {                                                                       ?
?          components.Add(new ModuleComponent                                 ?
?          {                                                                   ?
?              IdComponent = Convert.ToInt32(row["component_id"]),            ?
?              // ... resto de propiedades                                    ?
?          });                                                                 ?
?      }                                                                       ?
?                                                                              ?
?      return components;                                                     ?
?  }                                                                           ?
??????????????????????????????????????????????????????????????????????????????
             ?
             ?
??????????????????????????????????????????????????????????????????????????????
?                        4. DATABASE (MySQL/SQL Server)                        ?
?                                                                              ?
?  CALL sp_get_component(1);                                                  ?
?                                                                              ?
?  ????????????????????????????????????????????????????????????????          ?
?  ? component_id ? module_id   ?  name  ?    route    ?  icon    ?          ?
?  ????????????????????????????????????????????????????????????????          ?
?  ?      1       ?      1      ?Finanzas?             ? ri-money ?          ?
?  ?      2       ?      1      ?Facturas? /facturas   ? ri-file  ?          ?
?  ?      3       ?      1      ? Pagos  ? /pagos      ? ri-wallet?          ?
?  ????????????????????????????????????????????????????????????????          ?
??????????????????????????????????????????????????????????????????????????????
             ?
             ?
??????????????????????????????????????????????????????????????????????????????
?                     5. MÓDULO (Recibe Datos Inyectados)                     ?
?                                                                              ?
?  public class FinanzasModule : IModule                                      ?
?  {                                                                           ?
?      private List<ModuleComponent> _components = new();                     ?
?                                                                              ?
?      // ? Método de inyección (llamado por ModuleLoader)                  ?
?      public void SetComponents(List<ModuleComponent> components)            ?
?      {                                                                       ?
?          _components = components ?? new List<ModuleComponent>();           ?
?      }                                                                       ?
?                                                                              ?
?      // ? Método público (llamado por el sistema)                          ?
?      public List<ModuleComponent> GetComponents()                           ?
?      {                                                                       ?
?          return _components;                                                ?
?      }                                                                       ?
?  }                                                                           ?
??????????????????????????????????????????????????????????????????????????????
             ?
             ?
??????????????????????????????????????????????????????????????????????????????
?                         6. UI (Blazor Components)                            ?
?                                                                              ?
?  @foreach (var module in ModuleManager.GetAllModules())                     ?
?  {                                                                           ?
?      @foreach (var component in module.GetComponents())                     ?
?      {                                                                       ?
?          <NavLink href="@component.Route">                                  ?
?              <i class="@component.Icon"></i> @component.Name                ?
?          </NavLink>                                                          ?
?      }                                                                       ?
?  }                                                                           ?
??????????????????????????????????????????????????????????????????????????????
```

---

## ?? GLOSARIO

| Término | Definición | Ejemplo |
|---------|------------|---------|
| **Inversión de Dependencias (DIP)** | Módulos dependen de abstracciones, no de implementaciones | `IProductoRepository` (interfaz) vs `ProductoRepository` (implementación) |
| **Inyección de Dependencias (DI)** | Patrón donde las dependencias se proveen desde el exterior | Constructor: `public MiService(IRepository repo)` |
| **Repositorio** | Capa que abstrae el acceso a datos | `ProductoRepository` implementa `IProductoRepository` |
| **Stored Procedure (SP)** | Procedimiento almacenado en BD | `sp_get_component`, `sp_productos_getall` |
| **DTO (Data Transfer Object)** | Objeto para transferir datos entre capas | `ModuleMetadata`, `UserInfo` |
| **Plugin/Módulo** | Componente independiente cargado dinámicamente | `FinanzasModule`, `InventarioModule` |
| **Host** | Aplicación principal que carga los plugins | `VRM_PluginDemo.Blazor.Server` |
| **Abstracción** | Interfaz o clase abstracta (contrato) | `IModule`, `IUserRepository` |
| **Implementación** | Clase concreta que implementa una interfaz | `UserRepository : IUserRepository` |
| **Acoplamiento** | Grado de dependencia entre componentes | Alto acoplamiento = ?, Bajo acoplamiento = ? |
| **Cohesión** | Grado en que los elementos de un componente están relacionados | Alta cohesión = ? |

---

## ?? REFERENCIAS

### Documentación Interna

- [README Principal](../README.md)
- [Guía de Generación de Plugins](./PLUGIN-GENERATOR-GUIDE.md)
- [Ejemplos de Plugins](./PLUGIN-EXAMPLES.md)
- [Core README](../src/Core/README.md)

### Principios SOLID

- **S**ingle Responsibility Principle (SRP)
- **O**pen/Closed Principle (OCP)
- **L**iskov Substitution Principle (LSP)
- **I**nterface Segregation Principle (ISP)
- **D**ependency Inversion Principle (DIP)

### Patrones de Diseño

- **Repository Pattern**: Abstracción del acceso a datos
- **Dependency Injection**: Inyección de dependencias
- **Plugin Architecture**: Arquitectura basada en plugins
- **Clean Architecture**: Arquitectura limpia por capas

---

## ?? NOTAS FINALES

### Para Nuevos Desarrolladores

1. **NUNCA** accedas a BD desde un módulo
2. **SIEMPRE** usa interfaces (`I*Repository`)
3. **VERIFICA** con el script de validación antes de commit
4. **CONSULTA** este documento ante dudas

### Para Code Reviews

Verificar:
1. ? Módulos NO referencian `VRM_Plugin.Data`
2. ? Módulos tienen métodos `Set*()` para inyección
3. ? Repositorios están en `VRM_Plugin.Data`
4. ? Interfaces están en `Core.Abstractions`
5. ? Program.cs registra todos los servicios

### Contacto

Para preguntas o aclaraciones sobre esta arquitectura:
- Consultar con el equipo de arquitectura
- Abrir issue en el repositorio
- Revisar ejemplos en `PLUGIN-EXAMPLES.md`

---

**Versión del Documento:** 2.0  
**Última Actualización:** 2024  
**Estado:** ? Producción

---

