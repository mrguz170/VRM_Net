# ?? Adaptación de Plantilla Sliced a VRM System

## ?? Tabla de Contenidos
1. [Introducción](#introducción)
2. [Sobre Sliced Template](#sobre-sliced-template)
3. [Cambios Principales Realizados](#cambios-principales-realizados)
4. [Componentes Adaptados](#componentes-adaptados)
5. [Estilos y Assets](#estilos-y-assets)
6. [Estado y Servicios](#estado-y-servicios)

---

## ?? Introducción

**Sliced** es una plantilla profesional de administración construida con **Tailwind CSS** que ofrece componentes UI modernos y responsivos. VRM System adaptó esta plantilla para:

- ? **Mantener la identidad visual profesional** de Sliced
- ? **Integrar con arquitectura de plugins** de VRM
- ? **Agregar menú dinámico** basado en módulos cargados
- ? **Implementar sistema de permisos** granulares

---

## ?? Sobre Sliced Template

### **Características Originales**

- **Framework CSS**: Tailwind CSS 3.x
- **Componentes**: Sidebar, Topbar, Cards, Modals, Tables
- **Iconografía**: Remix Icons
- **Tema**: Light/Dark mode con `ModeStateService`
- **Layout**: Responsive con soporte móvil

### **Estructura Original de Sliced**

```
sliced_web_app/
??? wwwroot/
?   ??? assets/
?   ?   ??? css/
?   ?   ?   ??? tailwind.min.css      # Tailwind compilado
?   ?   ??? libs/
?   ?   ?   ??? remixicon/            # Iconos
?   ?   ?   ??? tailwindcss/          # Framework
?   ?   ?   ??? ...
?   ?   ??? images/
?   ?       ??? logo.svg
?   ??? ...
??? Components/
?   ??? Layout/
?   ?   ??? MainLayout.razor
?   ?   ??? Sidebar.razor              # Menú estático
?   ?   ??? Topbar.razor
?   ??? Pages/
?       ??? Index.razor
?       ??? ...
??? StateService/
    ??? ModeStateService.cs            # Cambio de tema
```

---

## ?? Cambios Principales Realizados

### **1. Integración de Arquitectura de Plugins**

#### **Antes (Sliced Original)**
```razor
<!-- Sidebar.razor - Menú hardcoded -->
<nav>
    <a href="/dashboard">Dashboard</a>
    <a href="/productos">Productos</a>
    <a href="/ventas">Ventas</a>
</nav>
```

#### **Después (VRM Adaptado)**
```razor
<!-- Sidebar.razor - Menú dinámico -->
@inject IModuleManager ModuleManager

<nav>
    @foreach (var module in ModuleManager.GetAllModules())
    {
        foreach (var component in module.GetComponents()
            .Where(c => c.ShowInMenu && c.IdParent == null)
            .OrderBy(c => c.MenuOrder))
        {
            <AuthorizeModule IdComponent="@component.IdComponent">
                <div class="nav-item">
                    <i class="@component.Icon"></i>
                    <span>@component.Name</span>
                    
                    @* Submenú (hijos) *@
                    @foreach (var child in GetChildComponents(component.IdComponent))
                    {
                        <a href="@child.Route">@child.Name</a>
                    }
                </div>
            </AuthorizeModule>
        }
    }
</nav>
```

**? Ventaja**: El menú se genera automáticamente según los módulos cargados y permisos del usuario.

---

### **2. Sistema de Autenticación**

#### **Antes (Sliced Original)**
- No incluía sistema de autenticación
- Solo plantilla UI

#### **Después (VRM Adaptado)**
```csharp
// DummyAuthenticationStateProvider.cs
public class DummyAuthenticationStateProvider : AuthenticationStateProvider
{
    private DummyUser? _cachedUser;
    
    public Task<bool> LoginAsync(string username, string password)
    {
        var usuario = GetDummyUserByUsername(username);
        _cachedUser = usuario;
        
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, usuario.Username),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim("FullName", usuario.FullName),
            // Claims por cada rol
            ...usuario.Roles.Select(r => new Claim(ClaimTypes.Role, r))
        };
        
        var identity = new ClaimsIdentity(claims, "DummyAuth");
        var principal = new ClaimsPrincipal(identity);
        
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(principal))
        );
        
        return Task.FromResult(true);
    }
}
```

**? Agregado**:
- Login page (`/login`)
- Logout functionality
- Cookie-based authentication
- Role-based claims

---

### **3. Componentes de Autorización Personalizados**

#### **Nuevo: `AuthorizeModule.razor`**

Protege componentes completos basándose en `IdComponent`:

```razor
@using Microsoft.AspNetCore.Components.Authorization
@inject IModuleAuthorizationService AuthService

@if (_canAccess)
{
    @ChildContent
}
else if (_isLoading)
{
    <div class="loading">Verificando permisos...</div>
}
else
{
    <div class="alert alert-warning">
        No tienes permisos para acceder a este módulo.
    </div>
}

@code {
    [Parameter] public int IdComponent { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    private bool _canAccess = false;
    private bool _isLoading = true;
    
    protected override async Task OnInitializedAsync()
    {
        _canAccess = await AuthService.CanUserAccessComponentAsync(IdComponent);
        _isLoading = false;
    }
}
```

#### **Nuevo: `AuthorizeAction.razor`**

Protege acciones específicas basándose en `ActionKey`:

```razor
@if (_canPerform)
{
    @ChildContent
}

@code {
    [Parameter] public string ActionKey { get; set; } = string.Empty;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    private bool _canPerform = false;
    
    protected override async Task OnInitializedAsync()
    {
        _canPerform = await AuthService.CanUserPerformActionAsync(ActionKey);
    }
}
```

**? Uso en páginas**:
```razor
<!-- Proteger toda la página -->
<AuthorizeModule IdComponent="2">
    <h1>Facturas</h1>
    
    <!-- Proteger acción crítica -->
    <AuthorizeAction ActionKey="Finanzas.Facturas.TimbrarSAT">
        <button>Timbrar SAT</button>
    </AuthorizeAction>
</AuthorizeModule>
```

---

### **4. Topbar Mejorado**

#### **Agregado al Topbar Original**

```razor
<!-- Topbar.razor -->
<header>
    <!-- Logo -->
    <div class="logo">
        <img src="/assets/images/logo.svg" alt="VRM" />
    </div>
    
    <!-- ? NUEVO: Información de usuario -->
    <AuthorizeView>
        <Authorized>
            <div class="user-info">
                <span class="user-name">@context.User.Identity?.Name</span>
                <span class="user-role">@GetPrimaryRole(context.User)</span>
                
                <!-- Dropdown de usuario -->
                <div class="dropdown">
                    <a href="/profile">Mi Perfil</a>
                    <a href="/settings">Configuración</a>
                    <hr />
                    <a href="/logout">Cerrar Sesión</a>
                </div>
            </div>
        </Authorized>
        <NotAuthorized>
            <a href="/login" class="btn-login">Iniciar Sesión</a>
        </NotAuthorized>
    </AuthorizeView>
    
    <!-- ? CONSERVADO: Toggle de tema -->
    <button @onclick="ToggleMode">
        @if (ModeState.IsDarkMode)
        {
            <i class="ri-sun-line"></i>
        }
        else
        {
            <i class="ri-moon-line"></i>
        }
    </button>
</header>
```

---

### **5. Estructura de Carpetas Adaptada**

```
VRM_Plugin.Blazor.Server/
??? wwwroot/
?   ??? assets/                        # ? CONSERVADO: Assets de Sliced
?   ?   ??? css/
?   ?   ?   ??? tailwind.min.css      # Tailwind original
?   ?   ??? libs/
?   ?   ?   ??? remixicon/            # Iconos Remix
?   ?   ?   ??? tailwindcss/          
?   ?   ??? images/
?   ?       ??? logo.svg
?   ??? ...
?
??? Components/
?   ??? Layout/
?   ?   ??? MainLayout.razor          # ? ADAPTADO: Layout de Sliced
?   ?   ??? Sidebar.razor             # ? MODIFICADO: Menú dinámico
?   ?   ??? Topbar.razor              # ? MODIFICADO: Info de usuario
?   ?
?   ??? Auth/                          # ? NUEVO: Componentes de autorización
?   ?   ??? AuthorizeModule.razor
?   ?   ??? AuthorizeAction.razor
?   ?   ??? RedirectToLogin.razor
?   ?
?   ??? Pages/
?       ??? Index.razor               # ? CONSERVADO: Dashboard de Sliced
?       ??? Login.razor               # ? NUEVO: Página de login
?       ??? Logout.razor              # ? NUEVO: Logout
?       ??? ModulesInfo.razor         # ? NUEVO: Info de plugins
?
??? Services/                          # ? NUEVO: Servicios de plugins
?   ??? ModuleLoader.cs
?   ??? IModuleManager.cs
?   ??? ModuleAuthorizationService.cs
?   ??? DummyAuthenticationStateProvider.cs
?
??? StateService/                      # ? CONSERVADO: De Sliced
?   ??? ModeStateService.cs           # Tema claro/oscuro
?
??? Modules/                           # ? NUEVO: Carpeta de plugins
    ??? VRM_Plugin.Modules.Finanzas.dll
    ??? VRM_Plugin.Modules.Prospectos.dll
```

---

## ?? Componentes Adaptados

### **MainLayout.razor**

#### **Cambios Realizados**

```razor
<!-- ANTES (Sliced Original) -->
<div class="main-layout">
    <Sidebar />
    <div class="content">
        <Topbar />
        @Body
    </div>
</div>

<!-- DESPUÉS (VRM Adaptado) -->
<CascadingAuthenticationState>               ?? NUEVO: Estado de autenticación
    <div class="main-layout @GetThemeClass()">
        
        <AuthorizeView>                      ?? NUEVO: Solo si está autenticado
            <Authorized>
                <Sidebar />
                <div class="content">
                    <Topbar />
                    <main>
                        @Body
                    </main>
                </div>
            </Authorized>
            <NotAuthorized>
                @Body                        ?? Páginas públicas (Login)
            </NotAuthorized>
        </AuthorizeView>
        
    </div>
</CascadingAuthenticationState>

@code {
    [CascadingParameter]
    private Task<AuthenticationState>? AuthState { get; set; }
    
    [Inject] 
    private ModeStateService ModeState { get; set; } = default!;
    
    private string GetThemeClass() => ModeState.IsDarkMode ? "dark" : "light";
}
```

---

## ?? Estilos y Assets

### **Conservados de Sliced**

? **Tailwind CSS**: Toda la configuración de Tailwind se mantuvo  
? **Remix Icons**: Sistema de iconografía completo  
? **Colores y Variables**: Paleta de colores original  
? **Componentes CSS**: Cards, buttons, forms, tables  
? **Responsive**: Breakpoints y utilidades móviles  

### **Agregados para VRM**

```css
/* custom-vrm.css */

/* Estilos para menú dinámico */
.nav-item.has-children {
    position: relative;
}

.nav-item .submenu {
    margin-left: 1.5rem;
    border-left: 2px solid var(--color-primary);
}

/* Estados de autorización */
.unauthorized-message {
    padding: 1rem;
    background: var(--color-warning-light);
    border-left: 4px solid var(--color-warning);
}

/* Badges de permisos */
.permission-badge {
    font-size: 0.75rem;
    padding: 0.25rem 0.5rem;
    border-radius: 0.25rem;
    background: var(--color-info-light);
}
```

---

## ?? Estado y Servicios

### **ModeStateService (Conservado)**

Servicio original de Sliced para cambio de tema:

```csharp
public class ModeStateService
{
    public bool IsDarkMode { get; private set; } = false;
    
    public event Action? OnChange;
    
    public void ToggleMode()
    {
        IsDarkMode = !IsDarkMode;
        OnChange?.Invoke();
    }
    
    public string GetModeClass() => IsDarkMode ? "dark" : "light";
}
```

**Uso en componentes**:
```razor
@inject ModeStateService ModeState
@implements IDisposable

<div class="@ModeState.GetModeClass()">
    <!-- Contenido -->
</div>

@code {
    protected override void OnInitialized()
    {
        ModeState.OnChange += StateHasChanged;
    }
    
    public void Dispose()
    {
        ModeState.OnChange -= StateHasChanged;
    }
}
```

---

## ?? Comparación Antes/Después

| Aspecto | Sliced Original | VRM Adaptado |
|---------|----------------|--------------|
| **Menú** | Estático (hardcoded) | **Dinámico (plugins)** |
| **Autenticación** | No incluida | **Cookie-based auth** |
| **Autorización** | Solo `<AuthorizeView>` | **AuthorizeModule + AuthorizeAction** |
| **Permisos** | Roles básicos | **Permisos granulares por ID** |
| **Módulos** | Todo en un proyecto | **Plugins independientes** |
| **Tema** | Light/Dark | **Light/Dark (conservado)** |
| **Estilos** | Tailwind puro | **Tailwind + estilos VRM** |
| **Iconos** | Remix Icons | **Remix Icons (conservado)** |

---

## ? Resumen de Adaptación

### **Lo que se Conservó**
1. ? Toda la estructura visual de Sliced
2. ? Sistema de temas (Light/Dark)
3. ? Iconografía Remix
4. ? Tailwind CSS completo
5. ? Diseño responsive
6. ? Componentes UI (cards, tables, forms)

### **Lo que se Agregó**
1. ? Arquitectura de plugins dinámicos
2. ? Sistema de autenticación
3. ? Autorización granular (componentes + acciones)
4. ? Menú dinámico basado en módulos
5. ? Servicios de gestión de plugins (ModuleLoader)
6. ? Componentes de protección (`AuthorizeModule`, `AuthorizeAction`)

### **Lo que se Modificó**
1. ?? `Sidebar.razor` - De menú estático a dinámico
2. ?? `Topbar.razor` - Agregada info de usuario
3. ?? `MainLayout.razor` - Integrado CascadingAuthenticationState
4. ?? Routing - Soporte para módulos externos

---

## ?? Resultado Final

La adaptación de Sliced a VRM resultó en:

- ? **Profesional**: Mantiene la calidad visual de Sliced
- ? **Modular**: Sistema de plugins completamente funcional
- ? **Seguro**: Permisos granulares integrados
- ? **Escalable**: Fácil agregar nuevos módulos
- ? **Mantenible**: Separación clara entre UI y lógica de negocio

**VRM = Sliced Template + Arquitectura de Plugins + Permisos Granulares**
