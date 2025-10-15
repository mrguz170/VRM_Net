# Host - Aplicacion Blazor Server Principal

Esta carpeta contiene la aplicacion host que carga los modulos dinamicamente.

## Proyecto: VRM_PluginDemo.Blazor.Server

### Estructura

```
VRM_PluginDemo.Blazor.Server/
??? Components/
?   ??? Layout/
?   ?   ??? MainLayout.razor
?   ?   ??? NavMenu.razor         # Menu dinamico
?   ??? Pages/
?   ?   ??? Home.razor
?   ?   ??? Login.razor           # Pagina de login
?   ?   ??? Logout.razor
?   ??? Routes.razor              # Router principal
?   ??? App.razor
??? Services/
?   ??? ModuleLoader.cs           # CLAVE: Carga DLLs dinamicamente
?   ??? IModuleManager.cs
?   ??? DummyAuthenticationStateProvider.cs  # Auth simulada
?   ??? IModuleAuthorizationService.cs
?   ??? ModuleAuthorizationService.cs
??? Modules/                      # Carpeta donde se copian las DLLs
??? Program.cs                    # Configuracion principal
```

---

## Componentes Clave

### 1. ModuleLoader.cs

Descubre y carga modulos automaticamente:

```csharp
public class ModuleLoader : IModuleManager
{
    public async Task<int> DiscoverAndLoadModulesAsync(string modulesPath)
    {
        // 1. Buscar DLLs
        var dllFiles = Directory.GetFiles(
            modulesPath, 
            "VRM_PluginDemo.Modules.*.dll");
        
        // 2. Cargar cada ensamblado
        foreach (var dllPath in dllFiles)
        {
            var assembly = Assembly.LoadFrom(dllPath);
            
            // 3. Buscar tipos que implementen IModule
            var moduleTypes = assembly.GetTypes()
                .Where(t => typeof(IModule).IsAssignableFrom(t));
            
            // 4. Crear instancia
            foreach (var moduleType in moduleTypes)
            {
                var module = (IModule)Activator.CreateInstance(moduleType);
                _loadedModules.Add(module);
            }
        }
        
        return _loadedModules.Count;
    }
}
```

### 2. DummyAuthenticationStateProvider.cs

Autenticacion simulada con cache en memoria:

```csharp
public class DummyAuthenticationStateProvider : AuthenticationStateProvider
{
    // IMPORTANTE: Debe ser Singleton
    private DummyUser? _cachedUser;
    
    public Task<bool> LoginAsync(string username, string password)
    {
        var usuario = GetDummyUserByUsername(username);
        _cachedUser = usuario;
        
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(CreateClaimsPrincipal(usuario))));
        
        return Task.FromResult(true);
    }
}
```

**CRITICO:** Debe estar registrado como Singleton:

```csharp
builder.Services.AddSingleton<DummyAuthenticationStateProvider>();
```

### 3. ModuleAuthorizationService.cs

Verifica permisos granulares:

```csharp
public class ModuleAuthorizationService : IModuleAuthorizationService
{
    public async Task<bool> CanUserPerformActionAsync(
        string moduleId, 
        string actionKey)
    {
        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        
        if (user.IsInRole("Admin")) return true;
        
        var module = _moduleManager.GetModule(moduleId);
        var actionPermissions = module.GetActionPermissions();
        
        if (!actionPermissions.TryGetValue(actionKey, out var allowedRoles))
            return false;
        
        return allowedRoles.Any(role => user.IsInRole(role));
    }
}
```

### 4. Routes.razor

Router con ensamblados dinamicos:

```razor
<Router AppAssembly="typeof(Program).Assembly" 
        AdditionalAssemblies="@additionalAssemblies">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="routeData" 
                           DefaultLayout="typeof(Layout.MainLayout)">
        </AuthorizeRouteView>
    </Found>
</Router>

@code {
    private static List<Assembly> additionalAssemblies = new();
    
    protected override void OnInitialized()
    {
        if (!isInitialized)
        {
            foreach (var module in ModuleManager.GetAllModules())
            {
                additionalAssemblies.Add(module.GetType().Assembly);
            }
            isInitialized = true;
        }
    }
}
```

### 5. NavMenu.razor

Menu generado dinamicamente:

```razor
<AuthorizeView>
    <Authorized>
        @foreach (var module in ModuleManager.GetAllModules())
        {
            foreach (var component in module.GetComponents()
                .Where(c => c.ShowInMenu))
            {
                <NavLink href="@component.Route">
                    @component.Name
                </NavLink>
            }
        }
    </Authorized>
</AuthorizeView>
```

---

## Configuracion en Program.cs

### 1. Cargar Modulos

```csharp
var moduleLoader = new ModuleLoader(logger);
builder.Services.AddSingleton<IModuleManager>(moduleLoader);

var modulosEncontrados = await moduleLoader.DiscoverAndLoadModulesAsync("Modules");
```

### 2. Registrar Servicios de Modulos

```csharp
foreach (var modulo in moduleLoader.GetAllModules())
{
    modulo.ConfigureServices(builder.Services, builder.Configuration);
}
```

### 3. Configurar Autenticacion (CRITICO)

```csharp
// Singleton para mantener cache entre navegaciones
builder.Services.AddSingleton<DummyAuthenticationStateProvider>();
builder.Services.AddSingleton<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<DummyAuthenticationStateProvider>());
```

### 4. Configurar Routing Dinamico

```csharp
var moduleAssemblies = moduleLoader.GetAllModules()
    .Select(m => m.GetType().Assembly)
    .Distinct()
    .ToArray();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(moduleAssemblies);
```

---

## Como Funciona el Hot Deployment

1. Compilar modulo: `dotnet build src/Modules/Finanzas/...`
2. Copiar DLL a `src/Host/.../Modules/`
3. Reiniciar aplicacion
4. Modulo aparece automaticamente en el menu

---

Volver a [README principal](../../README.md)
