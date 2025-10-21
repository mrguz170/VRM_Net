# GUIA DE DISEÑO Y ARQUITECTURA

**Proyecto:** VRM Plugin Demo  
**Version:** 1.0  
**Fecha:** Enero 2025

---

## Tabla de Contenido

1. [Vision General](#1-vision-general)
2. [Principios de Diseño](#2-principios-de-diseno)
3. [Arquitectura del Sistema](#3-arquitectura-del-sistema)
4. [Patrones de Diseño](#4-patrones-de-diseno)
5. [Capas de la Aplicacion](#5-capas-de-la-aplicacion)
6. [Sistema de Plugins](#6-sistema-de-plugins)
7. [Gestion de Estado](#7-gestion-de-estado)
8. [Seguridad y Autorizacion](#8-seguridad-y-autorizacion)
9. [Persistencia de Datos](#9-persistencia-de-datos)
10. [Multi-Tenancy](#10-multi-tenancy)
11. [Decisiones Tecnicas](#11-decisiones-tecnicas)
12. [Diagramas](#12-diagramas)

---

## 1. Vision General

### ¿Que es VRM Plugin Demo?

VRM Plugin Demo es una **plataforma modular empresarial** que permite:

1. **Cargar modulos de negocio dinamicamente** sin recompilar la aplicacion
2. **Aislar funcionalidades** en modulos independientes
3. **Gestionar permisos granulares** por accion dentro de cada modulo
4. **Soportar multiples clientes** (multi-tenancy) con datos aislados
5. **Escalar horizontalmente** agregando nuevos modulos

### Casos de Uso

**Para el Negocio:**
- Habilitar/deshabilitar modulos por cliente
- Personalizar funcionalidades sin cambiar el core
- Vender modulos por separado
- Desplegar actualizaciones de modulos sin afectar el sistema base

**Para Desarrollo:**
- Equipos independientes pueden desarrollar modulos
- Pruebas aisladas por modulo
- Deployment independiente de modulos
- Reutilizacion de modulos entre proyectos

---

## 2. Principios de Diseño

### 2.1 SOLID

#### Single Responsibility Principle (SRP)
- Cada clase tiene una unica razon para cambiar
- `ModuleLoader` solo carga modulos
- `ModuleAuthorizationService` solo verifica permisos
- `TenantService` solo gestiona el contexto del tenant

#### Open/Closed Principle (OCP)
- Sistema abierto para extension (nuevos modulos)
- Cerrado para modificacion (core no cambia)
- Interfaz `IModule` define el contrato
- Nuevos modulos implementan `IModule` sin tocar el core

#### Liskov Substitution Principle (LSP)
- Todos los modulos son intercambiables
- Cualquier implementacion de `IModule` funciona
- Repositorios base pueden ser reemplazados

#### Interface Segregation Principle (ISP)
- Interfaces pequeñas y especificas
- `IModuleManager` solo gestiona modulos
- `IModuleAuthorizationService` solo permisos
- No interfaces "gordas" con muchos metodos

#### Dependency Inversion Principle (DIP)
- Dependencias en abstracciones, no concreciones
- `ModuleLoader` depende de `IModule`, no de modulos especificos
- Servicios dependen de interfaces, no implementaciones

### 2.2 Clean Architecture

```
???????????????????????????????????????????????????????
?                  UI Layer (Blazor)                   ?
?  - Components                                        ?
?  - Pages                                             ?
?  - Layout                                            ?
???????????????????????????????????????????????????????
                   ?
???????????????????????????????????????????????????????
?            Application Layer (CQRS)                  ?
?  - Commands/Queries                                  ?
?  - Handlers                                          ?
?  - DTOs                                              ?
???????????????????????????????????????????????????????
                   ?
???????????????????????????????????????????????????????
?              Domain Layer (Core)                     ?
?  - Entities                                          ?
?  - Value Objects                                     ?
?  - Domain Services                                   ?
?  - Interfaces                                        ?
???????????????????????????????????????????????????????
                   ?
???????????????????????????????????????????????????????
?         Infrastructure Layer (Persistence)           ?
?  - DbContext                                         ?
?  - Repositories                                      ?
?  - External Services                                 ?
???????????????????????????????????????????????????????
```

**Reglas de dependencia:**
- UI depende de Application
- Application depende de Domain
- Infrastructure depende de Domain
- Domain NO depende de nadie

### 2.3 DRY (Don't Repeat Yourself)

- Logica compartida en clases base
- `TenantAwareRepository<T>` base para todos los repositorios
- `AuthorizeAction` component reutilizable
- Configuraciones en `appsettings.json`

### 2.4 KISS (Keep It Simple, Stupid)

- Codigo simple y legible
- Nombres descriptivos
- Metodos pequeños (< 20 lineas)
- Clases enfocadas

### 2.5 YAGNI (You Aren't Gonna Need It)

- No implementar funcionalidades "por si acaso"
- Features cuando se necesiten, no antes
- Evitar sobre-ingenieria

---

## 3. Arquitectura del Sistema

### 3.1 Arquitectura de Alto Nivel

```
???????????????????????????????????????????????????????????????
?                        INTERNET                              ?
???????????????????????????????????????????????????????????????
                       ?
???????????????????????????????????????????????????????????????
?                   LOAD BALANCER                              ?
?              (Azure App Gateway)                             ?
???????????????????????????????????????????????????????????????
                       ?
      ???????????????????????????????????
      ?                ?                ?
?????????????    ????????????    ????????????
?  BLAZOR   ?    ?  BLAZOR  ?    ?  BLAZOR  ?
? SERVER 1  ?    ? SERVER 2 ?    ? SERVER 3 ?
?           ?    ?          ?    ?          ?
? ????????? ?    ? ???????? ?    ? ???????? ?
? ?Module ? ?    ? ?Module? ?    ? ?Module? ?
? ?Loader ? ?    ? ?Loader? ?    ? ?Loader? ?
? ????????? ?    ? ???????? ?    ? ???????? ?
?     ?     ?    ?    ?     ?    ?    ?     ?
? ????????? ?    ? ???????? ?    ? ???????? ?
? ?Modulos? ?    ? ?Modulos?   ? ?Modulos? ?
? ?DLL    ? ?    ? ?DLL   ? ?    ? ?DLL   ? ?
? ????????? ?    ? ???????? ?    ? ???????? ?
?????????????    ????????????    ????????????
      ?               ?               ?
      ?????????????????????????????????
                      ?
      ?????????????????????????????????
      ?               ?               ?
????????????????? ????????????? ????????????????
?  SQL SERVER   ? ?   REDIS   ? ? BLOB STORAGE ?
? (Multi-tenant)? ?  (Cache)  ? ?   (Files)    ?
????????????????? ????????????? ????????????????
      ?
??????????????????????????????????????????????????
?         APPLICATION INSIGHTS                    ?
?    (Monitoring, Logging, Analytics)            ?
??????????????????????????????????????????????????
```

### 3.2 Flujo de Una Peticion

```
1. Usuario hace login
   ?
   ?
2. Blazor Server recibe request
   ?
   ?
3. AuthenticationStateProvider valida credenciales
   ?
   ?? Consulta BD (Identity)
   ?? Genera ClaimsPrincipal con roles y ClienteId
   ?? NotifyAuthenticationStateChanged()
   ?
   ?
4. Usuario navega a /finanzas
   ?
   ?
5. Router de Blazor
   ?
   ?? Verifica que /finanzas existe en modulos cargados
   ?? Resuelve componente Finanzas.razor
   ?? Valida rol con AuthorizeView
   ?
   ?
6. Finanzas.razor se renderiza
   ?
   ?? OnInitializedAsync()
   ?? Inyecta IFacturaService
   ?? Llama a GetFacturasAsync()
   ?
   ?
7. FacturaService.GetFacturasAsync()
   ?
   ?? Inyecta ITenantService
   ?? Obtiene ClienteId del usuario actual
   ?? Query con filtro: WHERE ClienteId = @clienteId
   ?? Retorna facturas del cliente
   ?
   ?
8. Component renderiza lista de facturas
   ?
   ?? AuthorizeAction verifica permisos granulares
      ?
      ?? Para boton "Timbrar SAT"
      ?? Consulta ModuleAuthorizationService
      ?? Verifica en BD si usuario tiene permiso
      ?? Muestra/oculta boton segun permiso
```

---

## 4. Patrones de Diseño

### 4.1 Plugin Pattern

**Problema:** Necesitamos agregar funcionalidades sin modificar el core.

**Solucion:** Sistema de plugins con interfaz comun.

```csharp
// Interfaz comun
public interface IModule
{
    string ModuleId { get; }
    void ConfigureServices(IServiceCollection services);
}

// Plugin concreto
public class FinanzasModule : IModule
{
    public string ModuleId => "Finanzas";
    
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IFacturaService, FacturaService>();
    }
}

// Host carga plugins
public class ModuleLoader
{
    public void LoadModules(string path)
    {
        var dlls = Directory.GetFiles(path, "*.dll");
        
        foreach (var dll in dlls)
        {
            var assembly = Assembly.LoadFrom(dll);
            var moduleTypes = assembly.GetTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t));
            
            foreach (var type in moduleTypes)
            {
                var module = (IModule)Activator.CreateInstance(type);
                _modules.Add(module);
            }
        }
    }
}
```

**Beneficios:**
- ? Modulos independientes
- ? Hot deployment
- ? Equipos separados

### 4.2 Repository Pattern

**Problema:** Abstraer acceso a datos del resto de la aplicacion.

**Solucion:** Interfaces de repositorio.

```csharp
// Interfaz
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<List<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
}

// Implementacion base
public class Repository<T> : IRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    
    public Repository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public virtual async Task<T?> GetByIdAsync(int id)
    {
        return await _context.Set<T>().FindAsync(id);
    }
    
    // Otros metodos...
}

// Repositorio especifico
public interface IFacturaRepository : IRepository<Factura>
{
    Task<List<Factura>> GetFacturasPendientesAsync();
}

public class FacturaRepository : Repository<Factura>, IFacturaRepository
{
    public FacturaRepository(ApplicationDbContext context) : base(context) { }
    
    public async Task<List<Factura>> GetFacturasPendientesAsync()
    {
        return await _context.Facturas
            .Where(f => f.Estado == EstadoFactura.Pendiente)
            .ToListAsync();
    }
}
```

**Beneficios:**
- ? Testeable (mock del repositorio)
- ? Cambiar BD facilmente
- ? Logica centralizada

### 4.3 Dependency Injection

**Problema:** Acoplamiento entre clases.

**Solucion:** Inyeccion de dependencias.

```csharp
// Interfaz
public interface IFacturaService
{
    Task<List<Factura>> GetFacturasAsync();
}

// Implementacion
public class FacturaService : IFacturaService
{
    private readonly IFacturaRepository _repository;
    private readonly ITenantService _tenantService;
    private readonly ILogger<FacturaService> _logger;
    
    // Constructor injection
    public FacturaService(
        IFacturaRepository repository,
        ITenantService tenantService,
        ILogger<FacturaService> logger)
    {
        _repository = repository;
        _tenantService = tenantService;
        _logger = logger;
    }
    
    public async Task<List<Factura>> GetFacturasAsync()
    {
        var clienteId = await _tenantService.GetCurrentTenantIdAsync();
        _logger.LogInformation("Obteniendo facturas para cliente {ClienteId}", clienteId);
        return await _repository.GetAllAsync();
    }
}

// Registro en DI
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<IFacturaService, FacturaService>();
```

**Beneficios:**
- ? Bajo acoplamiento
- ? Testeable
- ? Flexibilidad

### 4.4 Singleton Pattern

**Problema:** Necesitamos una sola instancia compartida (AuthenticationStateProvider).

**Solucion:** Singleton en DI.

```csharp
// Clase que debe ser singleton
public class DummyAuthenticationStateProvider : AuthenticationStateProvider
{
    // Cache en memoria compartido
    private DummyUser? _cachedUser;
    
    // Metodos...
}

// Registro como Singleton
builder.Services.AddSingleton<DummyAuthenticationStateProvider>();
builder.Services.AddSingleton<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<DummyAuthenticationStateProvider>());
```

**?? CRITICO:** AuthenticationStateProvider **debe** ser Singleton en Blazor Server para mantener el estado entre circuitos SignalR.

### 4.5 Strategy Pattern

**Problema:** Diferentes algoritmos para calcular permisos.

**Solucion:** Estrategias intercambiables.

```csharp
// Interfaz
public interface IAuthorizationStrategy
{
    Task<bool> IsAuthorizedAsync(ClaimsPrincipal user, string actionKey);
}

// Estrategia por roles
public class RoleBasedStrategy : IAuthorizationStrategy
{
    public async Task<bool> IsAuthorizedAsync(ClaimsPrincipal user, string actionKey)
    {
        // Verificar en BD por roles
    }
}

// Estrategia por claims
public class ClaimBasedStrategy : IAuthorizationStrategy
{
    public async Task<bool> IsAuthorizedAsync(ClaimsPrincipal user, string actionKey)
    {
        // Verificar claims especificos
    }
}

// Uso
public class ModuleAuthorizationService
{
    private readonly IAuthorizationStrategy _strategy;
    
    public ModuleAuthorizationService(IAuthorizationStrategy strategy)
    {
        _strategy = strategy;
    }
    
    public Task<bool> CanUserPerformActionAsync(ClaimsPrincipal user, string actionKey)
    {
        return _strategy.IsAuthorizedAsync(user, actionKey);
    }
}
```

### 4.6 Factory Pattern

**Problema:** Crear instancias de modulos dinamicamente.

**Solucion:** Factory para instanciar modulos.

```csharp
public class ModuleFactory
{
    public IModule CreateModule(Type moduleType)
    {
        if (!typeof(IModule).IsAssignableFrom(moduleType))
            throw new ArgumentException("Type must implement IModule");
        
        return (IModule)Activator.CreateInstance(moduleType)!;
    }
}
```

---

## 5. Capas de la Aplicacion

### 5.1 Presentation Layer (Blazor)

**Responsabilidad:** UI y user experience.

**Componentes:**
- `Pages/` - Paginas con routing (@page)
- `Layout/` - Layouts compartidos
- `Components/` - Componentes reutilizables
- `Services/` - Servicios de UI (estado local)

**Reglas:**
- ? NO acceder a DbContext directamente
- ? Inyectar servicios de Application layer
- ? Manejar estado local con `@code`
- ? Usar `@rendermode InteractiveServer`

### 5.2 Application Layer

**Responsabilidad:** Logica de aplicacion y casos de uso.

**Componentes:**
- `Commands/` - CQRS commands (Write)
- `Queries/` - CQRS queries (Read)
- `Handlers/` - Command/Query handlers
- `DTOs/` - Data Transfer Objects
- `Validators/` - Validaciones

**Ejemplo:**

```csharp
// Command
public record CrearFacturaCommand(
    string ClienteId,
    string NombreCliente,
    decimal Total);

// Handler
public class CrearFacturaHandler
{
    private readonly IFacturaRepository _repository;
    
    public async Task<int> HandleAsync(CrearFacturaCommand command)
    {
        var factura = new Factura
        {
            ClienteId = command.ClienteId,
            NombreCliente = command.NombreCliente,
            Total = command.Total,
            FechaEmision = DateTime.UtcNow,
            Estado = EstadoFactura.Pendiente
        };
        
        await _repository.AddAsync(factura);
        return factura.Id;
    }
}
```

### 5.3 Domain Layer

**Responsabilidad:** Logica de negocio y reglas del dominio.

**Componentes:**
- `Entities/` - Entidades del dominio
- `ValueObjects/` - Objetos de valor
- `DomainServices/` - Servicios de dominio
- `Interfaces/` - Contratos

**Ejemplo:**

```csharp
// Entidad
public class Factura
{
    public int Id { get; set; }
    public string ClienteId { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public EstadoFactura Estado { get; set; }
    
    // Logica de dominio
    public void Timbrar()
    {
        if (Estado != EstadoFactura.Pendiente)
            throw new InvalidOperationException("Solo se pueden timbrar facturas pendientes");
        
        Estado = EstadoFactura.Timbrada;
    }
    
    public bool PuedeEditarse()
    {
        return Estado == EstadoFactura.Pendiente;
    }
}
```

### 5.4 Infrastructure Layer

**Responsabilidad:** Implementaciones concretas de infraestructura.

**Componentes:**
- `Persistence/` - DbContext, Repositories
- `Identity/` - Autenticacion y autorizacion
- `ExternalServices/` - APIs externas (PAC, bancos)
- `FileStorage/` - Almacenamiento de archivos

**Ejemplo:**

```csharp
// DbContext
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Factura> Facturas { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configuraciones
        builder.Entity<Factura>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)");
            entity.HasQueryFilter(f => f.ClienteId == _tenantService.GetCurrentTenantId());
        });
    }
}
```

---

## 6. Sistema de Plugins

### 6.1 Arquitectura de Plugins

```
????????????????????????????????????????????????????????
?                   HOST (Core)                         ?
?  ??????????????????????????????????????????????????  ?
?  ?           ModuleLoader (Singleton)             ?  ?
?  ?  - DiscoverAndLoadModulesAsync(path)          ?  ?
?  ?  - GetAllModules()                            ?  ?
?  ?  - GetModule(moduleId)                        ?  ?
?  ??????????????????????????????????????????????????  ?
????????????????????????????????????????????????????????
                           ?
                           ? Carga
                           ?
      ???????????????????????????????????????????
      ?                    ?                    ?
?????????????????    ?????????????????   ????????????????
?  Finanzas.dll ?    ?Prospectos.dll ?   ?Inventario.dll?
?               ?    ?               ?   ?              ?
? ????????????? ?    ? ????????????? ?   ? ???????????? ?
? ?IModule    ? ?    ? ?IModule    ? ?   ? ?IModule   ? ?
? ?Impl       ? ?    ? ?Impl       ? ?   ? ?Impl      ? ?
? ????????????? ?    ? ????????????? ?   ? ???????????? ?
?               ?    ?               ?   ?              ?
? Components/   ?    ? Components/   ?   ? Components/  ?
? Services/     ?    ? Services/     ?   ? Services/    ?
? Domain/       ?    ? Domain/       ?   ? Domain/      ?
?????????????????    ?????????????????   ????????????????
```

### 6.2 Contrato IModule

```csharp
public interface IModule
{
    // Identificacion
    string ModuleId { get; }
    string DisplayName { get; }
    string Description { get; }
    string Version { get; }
    string Author { get; }
    string Category { get; }
    
    // Componentes Blazor que expone el modulo
    List<ComponentInfo> GetComponents();
    
    // Configurar servicios en DI
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
    
    // Dependencias con otros modulos
    List<string> Dependencies { get; }
    
    // Permisos que requiere el modulo
    List<string> RequiredPermissions { get; }
    
    // Permisos granulares por accion
    Dictionary<string, string[]> GetActionPermissions();
}
```

### 6.3 Ciclo de Vida de un Modulo

```
1. Compilacion
   ?
   ?? Modulo se compila a DLL
   ?? DLL contiene: Componentes, Servicios, Domain
   ?? Referencia a Core.Abstractions
   ?
   ?
2. Deployment
   ?
   ?? DLL se copia a carpeta /Modules del host
   ?? Puede ser manual o automatico (CI/CD)
   ?
   ?
3. Descubrimiento
   ?
   ?? Al iniciar aplicacion, ModuleLoader escanea /Modules
   ?? Busca archivos *.dll
   ?? Carga ensamblados con Assembly.LoadFrom()
   ?? Busca tipos que implementen IModule
   ?
   ?
4. Registro
   ?
   ?? ModuleLoader instancia cada IModule
   ?? Llama a ConfigureServices() de cada modulo
   ?? Servicios del modulo se registran en DI
   ?? Componentes se agregan al router
   ?
   ?
5. Ejecucion
   ?
   ?? Usuario navega a ruta del modulo
   ?? Router resuelve componente del modulo
   ?? Componente se inyecta con sus servicios
   ?? Modulo funciona como parte de la app
```

### 6.4 Ejemplo Completo de Modulo

**Estructura:**

```
VRM_PluginDemo.Modules.Finanzas/
??? FinanzasModule.cs            # Implementa IModule
??? Components/
?   ??? Finanzas.razor           # UI del modulo
??? Services/
?   ??? IFacturaService.cs
?   ??? FacturaService.cs
?   ??? IPagoService.cs
?   ??? PagoService.cs
??? Domain/
?   ??? Factura.cs
?   ??? Pago.cs
?   ??? Enums.cs
??? VRM_PluginDemo.Modules.Finanzas.csproj
```

**Implementacion:**

```csharp
// FinanzasModule.cs
public class FinanzasModule : IModule
{
    public string ModuleId => "Finanzas";
    public string DisplayName => "Modulo de Finanzas";
    public string Description => "Gestion de facturas y pagos";
    public string Version => "1.0.0";
    public string Author => "Equipo VRM";
    public string Category => "Financiero";
    
    public List<ComponentInfo> GetComponents()
    {
        return new List<ComponentInfo>
        {
            new ComponentInfo
            {
                Name = "Finanzas",
                Route = "/finanzas",
                ComponentType = typeof(Components.Finanzas),
                ShowInMenu = true,
                MenuOrder = 2,
                Icon = "bi bi-cash-coin",
                Description = "Gestion financiera"
            }
        };
    }
    
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Registrar servicios del modulo
        services.AddScoped<IFacturaService, FacturaService>();
        services.AddScoped<IPagoService, PagoService>();
        
        // Si el modulo necesita repositorios
        services.AddScoped<IFacturaRepository, FacturaRepository>();
    }
    
    public Dictionary<string, string[]> GetActionPermissions()
    {
        return new Dictionary<string, string[]>
        {
            ["Finanzas.Facturas.Ver"] = new[] { "Admin", "GerenteFinanzas", "Contador" },
            ["Finanzas.Facturas.Crear"] = new[] { "Admin", "GerenteFinanzas" },
            ["Finanzas.Facturas.TimbrarSAT"] = new[] { "Admin", "GerenteFinanzas" }
        };
    }
    
    public List<string> Dependencies => new List<string>();
    public List<string> RequiredPermissions => new List<string> { "ModuloFinanzas" };
}
```

---

## 7. Gestion de Estado

### 7.1 Estado de Autenticacion

**Problema:** Mantener el estado del usuario entre navegaciones en Blazor Server.

**Solucion:** AuthenticationStateProvider como Singleton con cache en memoria.

```csharp
public class DummyAuthenticationStateProvider : AuthenticationStateProvider
{
    // ? Cache en memoria (persiste entre navegaciones)
    private DummyUser? _cachedUser;
    private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
    
    public Task<bool> LoginAsync(string username, string password)
    {
        var usuario = GetDummyUserByUsername(username);
        if (usuario == null) return Task.FromResult(false);
        
        // Cachear usuario
        _cachedUser = usuario;
        _currentUser = CreateClaimsPrincipal(usuario);
        
        // Notificar cambio de estado
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(_currentUser)));
        
        return Task.FromResult(true);
    }
    
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_cachedUser != null)
        {
            return Task.FromResult(
                new AuthenticationState(CreateClaimsPrincipal(_cachedUser)));
        }
        
        return Task.FromResult(new AuthenticationState(_currentUser));
    }
}
```

**Registro:**

```csharp
// ? CORRECTO: Singleton
builder.Services.AddSingleton<DummyAuthenticationStateProvider>();
builder.Services.AddSingleton<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<DummyAuthenticationStateProvider>());

// ? INCORRECTO: Scoped (pierde cache entre navegaciones)
// builder.Services.AddScoped<AuthenticationStateProvider, DummyAuthenticationStateProvider>();
```

### 7.2 Estado de Componentes

**Cascading Parameters:**

```razor
<!-- App.razor -->
<CascadingAuthenticationState>
    <Router AppAssembly="@typeof(Program).Assembly">
        <!-- Router content -->
    </Router>
</CascadingAuthenticationState>

<!-- Cualquier componente hijo -->
@code {
    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateTask!;
        var user = authState.User;
        // Usar user...
    }
}
```

### 7.3 Estado de Modulos

**Estado compartido entre componentes:**

```csharp
// Servicio de estado (Scoped)
public class FacturaState
{
    private List<Factura> _facturas = new();
    public event Action? OnChange;
    
    public List<Factura> Facturas => _facturas;
    
    public void SetFacturas(List<Factura> facturas)
    {
        _facturas = facturas;
        NotifyStateChanged();
    }
    
    private void NotifyStateChanged() => OnChange?.Invoke();
}

// Registro
builder.Services.AddScoped<FacturaState>();

// Uso en componente
@inject FacturaState State

@implements IDisposable

@code {
    protected override void OnInitialized()
    {
        State.OnChange += StateHasChanged;
    }
    
    public void Dispose()
    {
        State.OnChange -= StateHasChanged;
    }
}
```

---

## 8. Seguridad y Autorizacion

### 8.1 Flujo de Autenticacion

```
1. Usuario ingresa credenciales
   ?
   ?
2. Login.razor llama a AuthenticationStateProvider.LoginAsync()
   ?
   ?? En desarrollo: DummyAuthenticationStateProvider
   ?? En produccion: RevalidatingServerAuthenticationStateProvider<ApplicationUser>
   ?
   ?
3. Validar credenciales
   ?
   ?? Desarrollo: Lista en memoria
   ?? Produccion: UserManager.CheckPasswordAsync()
   ?
   ?
4. Si valido:
   ?
   ?? Crear ClaimsPrincipal
   ?? Claims: NameIdentifier, Name, Email, Roles, ClienteId
   ?? Cachear usuario (Singleton)
   ?? NotifyAuthenticationStateChanged()
   ?
   ?
5. CascadingAuthenticationState propaga cambio
   ?
   ?
6. Todos los componentes reciben nuevo AuthenticationState
```

### 8.2 Niveles de Autorizacion

**Nivel 1: Por Rol (Basico)**

```razor
<AuthorizeView Roles="Admin,GerenteFinanzas">
    <Authorized>
        <p>Contenido para Admin y Gerente</p>
    </Authorized>
    <NotAuthorized>
        <p>No tienes acceso</p>
    </NotAuthorized>
</AuthorizeView>
```

**Nivel 2: Por Politica**

```csharp
// Configuracion
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequiereGerente", policy =>
        policy.RequireRole("Admin", "GerenteFinanzas"));
});

// Uso
<AuthorizeView Policy="RequiereGerente">
    <Authorized>...</Authorized>
</AuthorizeView>
```

**Nivel 3: Por Permiso Granular (Avanzado)**

```razor
<AuthorizeAction ModuleId="Finanzas" ActionKey="Finanzas.Facturas.TimbrarSAT">
    <button>Timbrar SAT</button>
</AuthorizeAction>
```

### 8.3 Claims del Usuario

```csharp
var claims = new List<Claim>
{
    new Claim(ClaimTypes.NameIdentifier, usuario.Id),          // ID del usuario
    new Claim(ClaimTypes.Name, usuario.Username),              // Username
    new Claim(ClaimTypes.Email, usuario.Email),                // Email
    new Claim("NombreCompleto", usuario.NombreCompleto),       // Nombre completo
    new Claim("ClienteId", usuario.ClienteId)                  // ? Para multi-tenancy
};

foreach (var rol in usuario.Roles)
{
    claims.Add(new Claim(ClaimTypes.Role, rol));               // Roles
}
```

---

## 9. Persistencia de Datos

### 9.1 Entity Framework Core

**DbContext:**

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    private readonly ITenantService _tenantService;
    
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantService tenantService) : base(options)
    {
        _tenantService = tenantService;
    }
    
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<ModuloAccion> ModuloAcciones { get; set; }
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Aplicar configuraciones de entidades
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        
        // Query filters globales para multi-tenancy
        builder.Entity<Factura>().HasQueryFilter(f => 
            f.ClienteId == _tenantService.GetCurrentTenantId());
    }
}
```

### 9.2 Configuraciones de Entidades

```csharp
public class FacturaConfiguration : IEntityTypeConfiguration<Factura>
{
    public void Configure(EntityTypeBuilder<Factura> builder)
    {
        builder.HasKey(f => f.Id);
        
        builder.Property(f => f.Numero)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(f => f.Total)
            .HasColumnType("decimal(18,2)");
        
        builder.HasIndex(f => new { f.ClienteId, f.Numero })
            .IsUnique();
        
        // Relaciones
        builder.HasMany(f => f.Pagos)
            .WithOne(p => p.Factura)
            .HasForeignKey(p => p.FacturaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

### 9.3 Migraciones

```bash
# Crear migracion
dotnet ef migrations add NombreMigracion

# Ver SQL que se ejecutara
dotnet ef migrations script

# Aplicar migracion
dotnet ef database update

# Rollback
dotnet ef database update NombreMigracionAnterior

# Remover ultima migracion (si no se aplico)
dotnet ef migrations remove
```

---

## 10. Multi-Tenancy

### 10.1 Estrategia: Shared Database, Shared Schema

**Todas las tablas tienen columna `ClienteId`:**

```sql
CREATE TABLE Facturas (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ClienteId NVARCHAR(50) NOT NULL,  -- ? Discriminador
    Numero NVARCHAR(50) NOT NULL,
    Total DECIMAL(18,2) NOT NULL,
    -- Otros campos...
    
    INDEX IX_Facturas_ClienteId (ClienteId)
);
```

### 10.2 Tenant Resolution

```csharp
public interface ITenantService
{
    Task<string?> GetCurrentTenantIdAsync();
    Task<Cliente?> GetCurrentTenantAsync();
}

public class TenantService : ITenantService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    
    public async Task<string?> GetCurrentTenantIdAsync()
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        return authState.User.FindFirst("ClienteId")?.Value;
    }
}
```

### 10.3 Query Filters Globales

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    // Filtro automatico por tenant
    builder.Entity<Factura>().HasQueryFilter(f => 
        f.ClienteId == _tenantService.GetCurrentTenantId());
    
    builder.Entity<Pago>().HasQueryFilter(p => 
        p.ClienteId == _tenantService.GetCurrentTenantId());
}
```

**?? IMPORTANTE:** Todos los queries automaticamente se filtran por `ClienteId`.

### 10.4 Repository Tenant-Aware

```csharp
public abstract class TenantAwareRepository<T> where T : class, ITenantEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly ITenantService _tenantService;
    
    public virtual async Task AddAsync(T entity)
    {
        // Asignar ClienteId automaticamente
        entity.ClienteId = await _tenantService.GetCurrentTenantIdAsync() 
            ?? throw new InvalidOperationException("No tenant context");
        
        await _context.Set<T>().AddAsync(entity);
        await _context.SaveChangesAsync();
    }
}
```

---

## 11. Decisiones Tecnicas

### 11.1 ¿Por que Blazor Server?

**Ventajas:**
- ? Codigo C# en backend (seguro)
- ? No JavaScript en cliente
- ? Actualizaciones en tiempo real (SignalR)
- ? SEO amigable
- ? Menos codigo duplicado

**Desventajas:**
- ? Requiere conexion constante
- ? Escalabilidad limitada (sticky sessions)
- ? Latencia por red

**Decision:** Blazor Server es ideal para aplicaciones empresariales internas con buena conectividad.

### 11.2 ¿Por que Singleton para AuthenticationStateProvider?

**Problema:**
- Blazor Server usa SignalR
- Cada navegacion = nuevo circuito SignalR
- Con Scoped = nueva instancia = cache vacio

**Solucion:**
- Singleton = una instancia para toda la app
- Cache `_cachedUser` persiste entre navegaciones
- Todos los circuitos usan la misma instancia

### 11.3 ¿Por que Query Filters en lugar de filtrar manualmente?

**Sin Query Filters:**
```csharp
// Hay que recordar filtrar en CADA query
var facturas = await _context.Facturas
    .Where(f => f.ClienteId == clienteId)  // Facil olvidarlo!
    .ToListAsync();
```

**Con Query Filters:**
```csharp
// Filtrado automatico
var facturas = await _context.Facturas.ToListAsync();  // Ya filtrado!
```

**Decision:** Query Filters previenen leaks de datos entre tenants.

### 11.4 ¿Por que Repository Pattern?

**Ventajas:**
- ? Abstraccion de BD
- ? Testeable (mock facil)
- ? Logica centralizada
- ? Cambiar BD sin tocar servicios

**Decision:** Vale la pena en aplicaciones medianas/grandes.

---

## 12. Diagramas

### 12.1 Diagrama de Clases (Core)

```
?????????????????????????????????????????
?           <<interface>>               ?
?             IModule                   ?
?????????????????????????????????????????
? + ModuleId: string                    ?
? + GetComponents(): List<ComponentInfo>?
? + ConfigureServices(...)              ?
? + GetActionPermissions(): Dictionary  ?
?????????????????????????????????????????
                ?
                ? implements
                ?
    ?????????????????????????
    ?                       ?
????????????????????  ????????????????????
? FinanzasModule   ?  ? ProspectosModule ?
????????????????????  ????????????????????
? + ModuleId       ?  ? + ModuleId       ?
? + GetComponents()?  ? + GetComponents()?
????????????????????  ????????????????????
```

### 12.2 Diagrama de Secuencia (Login)

```
Usuario    Login.razor    AuthProvider    DbContext    Database
  ?           ?               ?              ?            ?
  ?  Ingresar ?               ?              ?            ?
  ???????????>?               ?              ?            ?
  ?           ? LoginAsync()  ?              ?            ?
  ?           ???????????????>?              ?            ?
  ?           ?               ? FindByName() ?            ?
  ?           ?               ???????????????>?  SELECT  ?
  ?           ?               ?              ???????????>?
  ?           ?               ?              ?<???????????
  ?           ?               ?<???????????????  User    ?
  ?           ?               ? CheckPassword?            ?
  ?           ?               ???????????????>? VALIDATE ?
  ?           ?               ?<??????????????? HASH     ?
  ?           ?               ? CreateClaims ?            ?
  ?           ?               ? NotifyChange ?            ?
  ?           ?<???????????????              ?            ?
  ?<??????????? Navigate("/") ?              ?            ?
  ?           ?               ?              ?            ?
```

### 12.3 Diagrama de Deployment

```
???????????????????????????????????????????????????????
?              AZURE SUBSCRIPTION                      ?
?  ?????????????????????????????????????????????????  ?
?  ?         Resource Group: vrm-prod              ?  ?
?  ?  ??????????????????????????????????????????   ?  ?
?  ?  ?   Azure App Service (Linux)            ?   ?  ?
?  ?  ?   - Blazor Server App                  ?   ?  ?
?  ?  ?   - Auto-scaling enabled               ?   ?  ?
?  ?  ?   - /Modules folder mounted            ?   ?  ?
?  ?  ??????????????????????????????????????????   ?  ?
?  ?                   ?                            ?  ?
?  ?  ??????????????????????????????????????????   ?  ?
?  ?  ?   Azure SQL Database                   ?   ?  ?
?  ?  ?   - Standard S1                        ?   ?  ?
?  ?  ?   - Auto-backup enabled                ?   ?  ?
?  ?  ?   - Geo-replication                    ?   ?  ?
?  ?  ??????????????????????????????????????????   ?  ?
?  ?                                                ?  ?
?  ?  ??????????????????????????????????????????   ?  ?
?  ?  ?   Azure Blob Storage                   ?   ?  ?
?  ?  ?   - Files, XMLs                        ?   ?  ?
?  ?  ??????????????????????????????????????????   ?  ?
?  ?                                                ?  ?
?  ?  ??????????????????????????????????????????   ?  ?
?  ?  ?   Application Insights                 ?   ?  ?
?  ?  ?   - Logging, Monitoring                ?   ?  ?
?  ?  ??????????????????????????????????????????   ?  ?
?  ?????????????????????????????????????????????????  ?
???????????????????????????????????????????????????????
```

---

## Conclusion

Esta guia proporciona los fundamentos arquitectonicos del proyecto VRM Plugin Demo. Para detalles de implementacion, consultar:

- [ROADMAP_EMPRESARIAL.md](ROADMAP_EMPRESARIAL.md) - Plan de desarrollo
- [COMANDOS_SCRIPTS.md](COMANDOS_SCRIPTS.md) - Scripts y comandos
- [GUIA_AUTENTICACION_SIMULADA.md](GUIA_AUTENTICACION_SIMULADA.md) - Sistema de auth
- [GUIA_SISTEMA_PERMISOS_GRANULARES.md](GUIA_SISTEMA_PERMISOS_GRANULARES.md) - Permisos

**Preguntas o dudas:** Contactar al equipo de arquitectura.

---

**Ultima actualizacion:** Enero 2025  
**Version:** 1.0  
**Revisar:** Trimestralmente
