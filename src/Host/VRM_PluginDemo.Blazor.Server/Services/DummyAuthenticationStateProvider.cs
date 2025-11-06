using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace VRM_Plugin.Blazor.Server.Services;

/// <summary>
/// Proveedor de autenticación simulado con usuarios dummy.
/// -- SOLO PARA DESARROLLO --
/// </summary>
public class DummyAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly ILogger<DummyAuthenticationStateProvider> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PersistentComponentState _persistentState;
    
    private PersistingComponentStateSubscription _subscription;
    private AuthenticationState? _authenticationState;

    public DummyAuthenticationStateProvider(
  ILogger<DummyAuthenticationStateProvider> logger,
    IHttpContextAccessor httpContextAccessor,
        PersistentComponentState persistentState)
{
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _persistentState = persistentState;
        
        // Suscribirse al ciclo de vida del prerendering
        _subscription = persistentState.RegisterOnPersisting(OnPersistingAsync);
        
        _logger.LogInformation("?? [Auth] DummyAuthenticationStateProvider inicializado con PersistentComponentState");
  }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // FASE 1: Prerendering (SSR) - Lee de HttpContext
         var httpContext = _httpContextAccessor.HttpContext;
  
       if (httpContext != null)
          {
     if (httpContext.User?.Identity?.IsAuthenticated ?? false)
     {
           _logger.LogInformation("? [SSR] Usuario autenticado desde HttpContext: {Username}", 
 httpContext.User.Identity.Name);
              
    _authenticationState = new AuthenticationState(httpContext.User);
     return _authenticationState;
       }
      else
        {
         _logger.LogDebug("?? [SSR] Usuario NO autenticado en HttpContext");
                }
         }
            
   // FASE 2: Interactive Server (Circuit) - Lee del estado persistido
  if (_persistentState.TryTakeFromJson<UserInfo>("UserInfo", out var userInfo))
            {
      _logger.LogInformation("? [Circuit] Usuario restaurado desde estado persistido: {Email} con roles: [{Roles}]", 
   userInfo.Email,
         string.Join(", ", userInfo.Roles));
         
       var claims = new List<Claim>
{
     new Claim(ClaimTypes.NameIdentifier, userInfo.Id),
         new Claim(ClaimTypes.Name, userInfo.Username),
         new Claim(ClaimTypes.Email, userInfo.Email),
      new Claim("NombreCompleto", userInfo.NombreCompleto),
  new Claim("ClienteId", userInfo.ClienteId)
     };
          
                foreach (var role in userInfo.Roles)
              {
 claims.Add(new Claim(ClaimTypes.Role, role));
     }
     
           var identity = new ClaimsIdentity(claims, "PersistentState");
        _authenticationState = new AuthenticationState(new ClaimsPrincipal(identity));
       return _authenticationState;
            }
        
        _logger.LogDebug("?? [Circuit] No hay estado persistido, usuario NO autenticado");
       return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
   }
        catch (Exception ex)
        {
         _logger.LogError(ex, "?? [Auth] Error al obtener estado de autenticación");
        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    private Task OnPersistingAsync()
 {
        // Guardar estado de autenticación para el Circuit
     if (_authenticationState?.User.Identity?.IsAuthenticated ?? false)
  {
            var user = _authenticationState.User;
       var userInfo = new UserInfo
            {
      Id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
         Username = user.Identity.Name ?? "",
            Email = user.FindFirst(ClaimTypes.Email)?.Value ?? "",
   NombreCompleto = user.FindFirst("NombreCompleto")?.Value ?? "",
         ClienteId = user.FindFirst("ClienteId")?.Value ?? "",
        Roles = user.Claims
     .Where(c => c.Type == ClaimTypes.Role)
    .Select(c => c.Value)
      .ToList()
            };
         
        _persistentState.PersistAsJson("UserInfo", userInfo);
        
            _logger.LogInformation("?? [Persist] Estado de autenticación persistido para: {Email}", 
        userInfo.Email);
        }
        else
        {
   _logger.LogDebug("?? [Persist] No hay usuario autenticado para persistir");
        }
        
     return Task.CompletedTask;
    }

    /// <summary>
    /// Simula un login con usuario y contraseña.
    /// Crea cookie de autenticación persistente.
    /// </summary>
    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            _logger.LogInformation("?? [Auth] Intentando login para usuario: {Username}", username);

  // Buscar usuario por email
            var usuario = GetDummyUserByEmail(username);

          if (usuario == null)
            {
         _logger.LogWarning("?? [Auth] Usuario {Username} no encontrado", username);
 return false;
      }

            // ? VALIDACIÓN DUMMY - Cualquier contraseña funciona en desarrollo
         if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("?? [Auth] Contraseña vacía para {Username}", username);
      return false;
    }

            _logger.LogInformation("? [Auth] Usuario encontrado: {Username} con roles: [{Roles}]", 
                usuario.Username, 
    string.Join(", ", usuario.Roles));

            //  Crear claims
         var claims = new List<Claim>
    {
  new Claim(ClaimTypes.NameIdentifier, usuario.Id),
         new Claim(ClaimTypes.Name, usuario.Username),
       new Claim(ClaimTypes.Email, usuario.Email),
          new Claim("NombreCompleto", usuario.NombreCompleto),
         new Claim("ClienteId", usuario.ClienteId)
        };

            // Agregar roles como claims
       foreach (var rol in usuario.Roles)
     {
   claims.Add(new Claim(ClaimTypes.Role, rol));
    }

    //  Crear identity y principal
         var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

      //  Crear cookie de autenticación
       var httpContext = _httpContextAccessor.HttpContext;
            
 if (httpContext == null)
            {
      _logger.LogError("?? [Auth] HttpContext es null, no se puede crear cookie");
 return false;
      }

            // ?? Verificar que la respuesta NO haya comenzado
       if (httpContext.Response.HasStarted)
    {
         _logger.LogError("?? [Auth] Response ya comenzó. No se puede escribir cookie.");
       _logger.LogError("?? [Auth] SOLUCIÓN: Asegúrate de que Login.razor NO tiene @rendermode InteractiveServer");
         return false;
    }

            // ? Crear cookie persistente
        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
    principal,
              new AuthenticationProperties
       {
         IsPersistent = true,
 ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
        AllowRefresh = true
         });

        _logger.LogInformation("? [Auth] Cookie de autenticación creada exitosamente para {Username}", username);
 
       // Guardar estado actual para persistencia
  _authenticationState = new AuthenticationState(principal);
      
   // Notificar cambio de autenticación
            NotifyAuthenticationStateChanged(Task.FromResult(_authenticationState));
            
        return true;
        }
        catch (Exception ex)
        {
_logger.LogError(ex, "?? [Auth] Error durante login de {Username}", username);
      return false;
        }
  }

 /// <summary>
    /// Cierra sesión del usuario actual.
    /// ? Elimina cookie de autenticación.
    /// </summary>
    public async Task LogoutAsync()
    {
   try
        {
            var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
            {
                var username = httpContext.User?.Identity?.Name ?? "desconocido";
     
     // ?? Verificar que la respuesta NO haya comenzado
    if (!httpContext.Response.HasStarted)
    {
     // ?? Eliminar cookie de autenticación
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

    _logger.LogInformation("? [Auth] Logout exitoso para usuario: {Username}", username);
        }
     else
            {
     _logger.LogWarning("?? [Auth] No se puede eliminar cookie: Response ya comenzó");
       }

     // Limpiar estado
            _authenticationState = null;
   
        // Notificar cambio
   NotifyAuthenticationStateChanged(Task.FromResult(
         new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
          }
          else
            {
         _logger.LogWarning("?? [Auth] HttpContext es null en LogoutAsync");
            }
        }
        catch (Exception ex)
        {
   _logger.LogError(ex, "?? [Auth] Error durante logout");
        }
    }

    // ==================== DATOS DUMMY ====================

  private static readonly List<DummyUser> DummyUsers = new()
    {
        new DummyUser
        {
            Id = "user-001",
            Username = "admin",
          Email = "admin@vrm.com",
      NombreCompleto = "Administrador del Sistema",
      ClienteId = "cliente-001",
     Roles = new List<string> { "Admin" }
        },
      new DummyUser
        {
   Id = "user-001b",
     Username = "user",
            Email = "user@vrm.com",
 NombreCompleto = "Usuario Regular",
ClienteId = "cliente-001",
   Roles = new List<string> { "User" }
     },
  new DummyUser
   {
     Id = "user-002",
  Username = "gerente.finanzas",
         Email = "gerente.finanzas@vrm.com",
            NombreCompleto = "Juan Gerente de Finanzas",
  ClienteId = "cliente-001",
       Roles = new List<string> { "GerenteFinanzas" }
    },
        new DummyUser
        {
       Id = "user-003",
     Username = "coordinador.finanzas",
            Email = "coordinador.finanzas@vrm.com",
      NombreCompleto = "María Coordinadora de Finanzas",
   ClienteId = "cliente-001",
            Roles = new List<string> { "CoordinadorFinanzas" }
        },
        new DummyUser
  {
            Id = "user-004",
        Username = "contador",
         Email = "contador@vrm.com",
     NombreCompleto = "Pedro Contador",
            ClienteId = "cliente-001",
         Roles = new List<string> { "Contador" }
        },
        new DummyUser
        {
 Id = "user-005",
          Username = "gestor.prospectos",
          Email = "gestor.prospectos@vrm.com",
        NombreCompleto = "Ana Gestora de Prospectos",
     ClienteId = "cliente-001",
            Roles = new List<string> { "GestorProspectos" }
    },
    new DummyUser
        {
    Id = "user-006",
    Username = "revisor.legal",
   Email = "revisor.legal@vrm.com",
            NombreCompleto = "Carlos Revisor Legal",
  ClienteId = "cliente-001",
            Roles = new List<string> { "RevisorLegal" }
        },
   new DummyUser
 {
 Id = "user-007",
            Username = "revisor.finanzas",
         Email = "revisor.finanzas@vrm.com",
     NombreCompleto = "Laura Revisora Financiera",
     ClienteId = "cliente-001",
        Roles = new List<string> { "RevisorFinanzas" }
        }
    };

    private DummyUser? GetDummyUserByEmail(string email)
    {
  return DummyUsers.FirstOrDefault(u =>
     u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }

    public static List<DummyUser> GetAllDummyUsers()
    {
        return DummyUsers;
    }
    
    public void Dispose()
    {
        _subscription.Dispose();
    }
}

public class DummyUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string ClienteId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
