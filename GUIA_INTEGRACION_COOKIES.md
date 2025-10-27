# ?? GUÍA DE INTEGRACIÓN: COOKIES EN DUMMYAUTHENTICATIONSTATEPROVIDER

> **Objetivo:** Integrar autenticación con cookies persistentes en `DummyAuthenticationStateProvider` manteniendo la lista de usuarios dummy para desarrollo, preparando migración futura a ASP.NET Core Identity.

---

## ?? **ÍNDICE**

1. [Resumen Ejecutivo](#resumen-ejecutivo)
2. [Arquitectura Actual vs Propuesta](#arquitectura-actual-vs-propuesta)
3. [Cambios Necesarios](#cambios-necesarios)
4. [Paso a Paso de Implementación](#paso-a-paso-de-implementación)
5. [Verificación y Testing](#verificación-y-testing)
6. [Roadmap de Migración](#roadmap-de-migración)

---

## ?? **RESUMEN EJECUTIVO**

### **Problema Actual:**
- `DummyAuthenticationStateProvider` guarda estado solo en memoria (`_cachedUser`)
- No crea cookies HTTP, por lo que la sesión **se pierde al recargar** la página
- Hay configuración de cookies en `Program.cs` pero **no se usan**

### **Solución Propuesta:**
- Integrar `IHttpContextAccessor` en `DummyAuthenticationStateProvider`
- Usar `SignInAsync()` para crear cookies persistentes en `LoginAsync()`
- Leer `HttpContext.User` en `GetAuthenticationStateAsync()` (en lugar de cache memoria)
- Mantener lista de usuarios dummy para desarrollo

### **Beneficios:**
- ? Sesión persiste entre recargas de página
- ? Compatible con `[Authorize]`
- ? Fácil migración a Identity después
- ? Sin cambios en UI (Login.razor, etc.)
- ? Mantiene datos dummy para desarrollo

---

## ??? **ARQUITECTURA ACTUAL VS PROPUESTA**

### **ACTUAL (? Problema):**

```
Usuario ? Login.razor ? DummyAuthenticationStateProvider.LoginAsync()
                ?
             Guarda en _cachedUser (memoria)
          ?
             NotifyAuthenticationStateChanged()
                ?
     ? NO crea cookie
  ?
Usuario recarga F5 ? ? Sesión perdida
```

---

### **PROPUESTA (? Solución):**

```
Usuario ? Login.razor ? DummyAuthenticationStateProvider.LoginAsync()
     ?
        Valida contra DummyUsers (lista hardcodeada)
  ?
  HttpContext.SignInAsync("Cookies", claims)
    ?
            ? Crea cookie encriptada
          ?
  NotifyAuthenticationStateChanged()
?
Usuario recarga F5 ? Cookie enviada ? ? Sesión recuperada
```

---

## ?? **CAMBIOS NECESARIOS**

### **Archivos a Modificar:**

| Archivo | Cambios | Complejidad |
|---------|---------|-------------|
| `DummyAuthenticationStateProvider.cs` | Agregar IHttpContextAccessor, SignInAsync, SignOutAsync | Media |
| `Program.cs` | Agregar `AddHttpContextAccessor()` | Baja |
| `Login.razor` | Agregar `@formname` attribute | Baja |

**Total:** 3 archivos, ~50 líneas de código

---

## ?? **PASO A PASO DE IMPLEMENTACIÓN**

### **PASO 1: Modificar `Program.cs`**

**Ubicación:** `src/Host/VRM_Plugin.Blazor.Server/Program.cs`

**Cambio:**

```csharp
// ==================== AUTENTICACIÓN Y AUTORIZACIÓN ====================

// ? AGREGAR ESTA LÍNEA (necesaria para acceder a HttpContext)
builder.Services.AddHttpContextAccessor();

// Cambiar la configuración de autenticación
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
    options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "VRM.Auth";
        options.Cookie.HttpOnly = true;
   options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// Mantener como Singleton (funciona bien para Blazor Server)
builder.Services.AddSingleton<DummyAuthenticationStateProvider>();
builder.Services.AddSingleton<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<DummyAuthenticationStateProvider>());
```

**Imports necesarios:**
```csharp
using Microsoft.AspNetCore.Authentication.Cookies;
```

---

### **PASO 2: Actualizar `DummyAuthenticationStateProvider.cs`**

**Ubicación:** `src/Host/VRM_Plugin.Blazor.Server/Services/DummyAuthenticationStateProvider.cs`

#### **2.1: Agregar imports**

```csharp
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
```

#### **2.2: Modificar constructor**

```csharp
public class DummyAuthenticationStateProvider : AuthenticationStateProvider
{
 private readonly ILogger<DummyAuthenticationStateProvider> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;  // ? NUEVO
    
// ? ELIMINAR: private DummyUser? _cachedUser;
    // ? ELIMINAR: private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

    public DummyAuthenticationStateProvider(
        ILogger<DummyAuthenticationStateProvider> logger,
        IHttpContextAccessor httpContextAccessor)  // ? NUEVO
    {
   _logger = logger;
    _httpContextAccessor = httpContextAccessor;  // ? NUEVO
    }
    
    // ... resto del código
}
```

#### **2.3: Modificar `GetAuthenticationStateAsync()`**

```csharp
public override Task<AuthenticationState> GetAuthenticationStateAsync()
{
    try
    {
        // ? Leer desde HttpContext.User (ya validado por ASP.NET Core)
      var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User ?? new ClaimsPrincipal(new ClaimsIdentity());

        _logger.LogDebug("Usuario autenticado: {IsAuthenticated}, Name: {Name}", 
            user.Identity?.IsAuthenticated ?? false,
      user.Identity?.Name ?? "Anonymous");

        return Task.FromResult(new AuthenticationState(user));
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al obtener estado de autenticación");
        return Task.FromResult(new AuthenticationState(
            new ClaimsPrincipal(new ClaimsIdentity())));
    }
}
```

#### **2.4: Modificar `LoginAsync()`**

```csharp
public async Task<bool> LoginAsync(string username, string password)
{
    try
    {
        _logger.LogInformation("?? Intentando login para usuario: {Username}", username);

   // ? Buscar en lista hardcodeada (sin cambios)
        var usuario = GetDummyUserByUsername(username);

        if (usuario == null)
 {
            _logger.LogWarning("? Usuario {Username} no existe", username);
      return false;
        }

        // ? Validación de contraseña (dummy)
     if (string.IsNullOrWhiteSpace(password))
        {
        _logger.LogWarning("? Contraseña vacía para {Username}", username);
 return false;
        }

      // ? Crear claims (sin cambios)
        var claims = new List<Claim>
   {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id),
    new Claim(ClaimTypes.Name, usuario.Username),
        new Claim(ClaimTypes.Email, usuario.Email),
    new Claim("NombreCompleto", usuario.NombreCompleto),
    new Claim("ClienteId", usuario.ClienteId)
   };

        foreach (var rol in usuario.Roles)
        {
     claims.Add(new Claim(ClaimTypes.Role, rol));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
   var principal = new ClaimsPrincipal(identity);

     // ? CREAR COOKIE (NUEVO)
  var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
      {
      _logger.LogError("? HttpContext es null");
            return false;
        }

        await httpContext.SignInAsync(
          CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
   {
    IsPersistent = true,  // Cookie persiste al cerrar navegador
   ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
     AllowRefresh = true
    });

        _logger.LogInformation("? Login exitoso para {Username} con roles: [{Roles}]", 
            username, 
            string.Join(", ", usuario.Roles));
        
      // ? Notificar cambio (sin cambios)
     NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

 return true;
 }
    catch (Exception ex)
    {
        _logger.LogError(ex, "? Error durante login de {Username}", username);
  return false;
    }
}
```

#### **2.5: Modificar `LogoutAsync()`**

```csharp
public async Task LogoutAsync()
{
    try
    {
      var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
   {
 _logger.LogWarning("HttpContext es null durante logout");
  return;
        }

  var username = httpContext.User?.Identity?.Name ?? "desconocido";

// ? Eliminar cookie (NUEVO)
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation("?? Logout exitoso para usuario: {Username}", username);

        // ? Notificar cambio
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
  }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error durante logout");
    }
}
```

---

### **PASO 3: Agregar `@formname` a `Login.razor`**

**Ubicación:** `src/Host/VRM_Plugin.Blazor.Server/Components/Pages/Login.razor`

**Problema:** Blazor requiere `@formname` en formularios para identificarlos.

**Cambio:**

```razor
<!-- Formulario de Login -->
<EditForm Model="@loginModel" OnValidSubmit="@HandleLogin" FormName="LoginForm">
    <!-- ? AGREGAR FormName="LoginForm" -->
    <DataAnnotationsValidator />

    <div class="space-y-4">
        <!-- ... resto del formulario ... -->
    </div>
</EditForm>
```

---

## ? **VERIFICACIÓN Y TESTING**

### **Checklist de Verificación:**

```
Compilación:
? dotnet build (sin errores)

Funcionalidad:
? Abrir http://localhost:5062
? Redirige automáticamente a /login
? Login con: admin / admin123
? Muestra "Welcome, admin!"
? Recargar página (F5)
? ? Sesión persiste (no pide login de nuevo)
? Logout
? ? Redirige a /login

Cookies:
? Abrir DevTools ? Application ? Cookies
? Verificar cookie "VRM.Auth" existe
? Verificar HttpOnly = true
? Verificar Secure (si HTTPS)

Console Logs:
? Ver logs de login exitoso
? Ver logs de cookie creada
? Ver logs de logout
```

---

### **Testing con Diferentes Usuarios:**

| Usuario | Password | Rol | Permisos |
|---------|----------|-----|----------|
| admin | admin123 | Admin | Todos |
| user | user123 | User | Limitado |
| gerente.finanzas | cualquiera | GerenteFinanzas | Finanzas completo |
| coordinador.finanzas | cualquiera | CoordinadorFinanzas | Finanzas sin timbrar |

---

## ??? **ROADMAP DE MIGRACIÓN**

### **FASE 1: Desarrollo (Actual - con cookies)**
```
? DummyAuthenticationStateProvider
? Lista hardcodeada (DummyUsers)
? Cookies persistentes
? Sin base de datos
```

**Duración:** Ya implementado

---

### **FASE 2: Pre-Producción (Opcional)**
```
?? DummyAuthenticationStateProvider (sin cambios)
?? Base de datos con Dapper/EF Core
? Cookies (sin cambios)
? Sin Identity (aún)
```

**Cambios:**
```csharp
// Cambiar GetDummyUserByUsername() por:
private async Task<DummyUser?> GetUserByUsernameAsync(string username)
{
    using var connection = new SqlConnection(_connectionString);
    return await connection.QueryFirstOrDefaultAsync<DummyUser>(
        "SELECT * FROM Users WHERE Username = @Username",
  new { Username = username });
}
```

**Duración:** 1-2 semanas

---

### **FASE 3: Producción (ASP.NET Core Identity)**
```
?? Renombrar a CustomAuthenticationStateProvider
?? Integrar UserManager<ApplicationUser>
? Cookies (sin cambios)
? Identity completo (password hash, 2FA, etc.)
```

**Cambios:**
```csharp
public class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    
    public async Task<bool> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return false;
   
        var result = await _signInManager.PasswordSignInAsync(
            user, password, isPersistent: true, lockoutOnFailure: true);
        
        if (result.Succeeded)
        {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
       return true;
  }
        
    return false;
  }
}
```

**Duración:** 2-4 semanas

---

## ?? **COMPARACIÓN: ANTES vs DESPUÉS**

### **ANTES (Sin Cookies):**

| Característica | Estado |
|----------------|--------|
| Persistencia entre recargas | ? No |
| Funciona con `[Authorize]` | ?? Parcial |
| Sesión en memoria | ? Sí |
| Multi-tab sync | ? No |
| Producción ready | ? No |

### **DESPUÉS (Con Cookies):**

| Característica | Estado |
|----------------|--------|
| Persistencia entre recargas | ? Sí |
| Funciona con `[Authorize]` | ? Completo |
| Sesión en cookie | ? Sí |
| Multi-tab sync | ? Sí |
| Producción ready | ?? Con migración a Identity |

---

## ?? **CONSIDERACIONES DE SEGURIDAD**

### **Configuración de Cookies:**

```csharp
options.Cookie.Name = "VRM.Auth";      // Nombre personalizado
options.Cookie.HttpOnly = true;      // ? No accesible desde JavaScript
options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;  // ? Solo HTTPS en producción
options.Cookie.SameSite = SameSiteMode.Strict;  // ? Protección CSRF
options.ExpireTimeSpan = TimeSpan.FromHours(8);  // Expiración
options.SlidingExpiration = true;  // ? Renovación automática
```

### **Recomendaciones:**

1. **HTTPS Obligatorio en Producción:**
   ```csharp
   if (!app.Environment.IsDevelopment())
   {
       options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
   }
   ```

2. **Agregar Anti-Forgery:**
   ```csharp
   builder.Services.AddAntiforgery();
   app.UseAntiforgery();
   ```

3. **Rate Limiting en Login:**
   ```csharp
   builder.Services.AddRateLimiter(options => { ... });
   ```

---

## ?? **RECURSOS ADICIONALES**

- [ASP.NET Core Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Cookie Authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie)
- [Blazor Server Security](https://learn.microsoft.com/en-us/aspnet/core/blazor/security/)
- [Identity Integration](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)

---

## ? **FAQ**

### **¿Por qué Singleton si usamos cookies?**
**R:** El Singleton de `DummyAuthenticationStateProvider` no guarda estado de usuarios (eso lo hacen las cookies). Solo proporciona la lógica de autenticación. Singleton es válido porque la lógica es stateless.

### **¿Puedo usar Scoped en lugar de Singleton?**
**R:** Sí, puedes cambiar a `Scoped` sin problemas. La diferencia es mínima para Blazor Server.

### **¿Necesito cambiar algo en la UI?**
**R:** Solo agregar `FormName="LoginForm"` en `Login.razor`. El resto funciona igual.

### **¿Cuándo migrar a Identity?**
**R:** Cuando necesites:
- Hash de contraseñas
- Recuperación de contraseña
- Two-Factor Authentication
- Lockout de cuentas
- Múltiples tenants con BD

---

## ?? **CONCLUSIÓN**

Esta integración te da:

? **Lo mejor de ambos mundos:**
- Simplicidad de desarrollo con datos dummy
- Robustez de producción con cookies persistentes

? **Migración gradual:**
- Fase 1: Dummy + Cookies (ahora)
- Fase 2: BD + Cookies
- Fase 3: Identity + Cookies

? **Sin refactoring masivo:**
- Solo 3 archivos modificados
- ~50 líneas de código
- UI sin cambios

---

**?? Última actualización:** $(Get-Date -Format "yyyy-MM-dd")  
**?? Autor:** VRM Development Team  
**?? Soporte:** Crear issue en GitHub
