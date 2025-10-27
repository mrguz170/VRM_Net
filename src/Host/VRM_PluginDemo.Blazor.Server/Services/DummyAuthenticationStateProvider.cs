using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace VRM_PluginDemo.Blazor.Server.Services;

/// <summary>
/// Proveedor de autenticación simulado con usuarios dummy.
/// ?? SOLO PARA DESARROLLO - Reemplazar con autenticación real en producción.
/// 
/// ? ACTUALIZADO: Usa cookies HTTP para persistir autenticación entre requests.
/// </summary>
public class DummyAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILogger<DummyAuthenticationStateProvider> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DummyAuthenticationStateProvider(
        ILogger<DummyAuthenticationStateProvider> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
 // ? Leer autenticación desde cookies (HttpContext)
   var httpContext = _httpContextAccessor.HttpContext;
         
   if (httpContext?.User?.Identity?.IsAuthenticated ?? false)
            {
              _logger.LogDebug("? Usuario autenticado desde cookie: {Username}", 
         httpContext.User.Identity.Name);
        return new AuthenticationState(httpContext.User);
            }

            // Usuario no autenticado
            _logger.LogDebug("? Usuario NO autenticado");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? Error al obtener estado de autenticación");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    /// <summary>
  /// Simula un login con usuario y contraseña.
    /// ? Crea cookie de autenticación persistente.
    /// ?? En producción, esto validaría contra una base de datos con contraseñas hasheadas.
    /// </summary>
 public async Task<bool> LoginAsync(string username, string password)
    {
      try
        {
 _logger.LogInformation("?? Intentando login para usuario: {Username}", username);

    // Buscar usuario por email
            var usuario = GetDummyUserByEmail(username);

            if (usuario == null)
            {
           _logger.LogWarning("? Intento de login fallido: usuario {Username} no existe", username);
          return false;
            }

            // ?? VALIDACIÓN DUMMY - En producción, verificar hash de contraseña
            if (string.IsNullOrWhiteSpace(password))
          {
       _logger.LogWarning("? Intento de login fallido: contraseña vacía para {Username}", username);
                return false;
  }

    // ? Crear claims
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

 // ? Crear identity y principal
  var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // ? CLAVE: Crear cookie de autenticación
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
    {
   // ? Verificar que la respuesta NO haya comenzado
       if (httpContext.Response.HasStarted)
     {
           _logger.LogError("? PROBLEMA: Response ya comenzó. No se puede escribir cookie.");
            return false;
        }

  await httpContext.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
  new AuthenticationProperties
       {
  IsPersistent = true, // Persistir entre sesiones del navegador
     ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
   AllowRefresh = true
          });

      _logger.LogInformation("? Login exitoso con cookie para {Username} con roles: [{Roles}]", 
    username, 
      string.Join(", ", usuario.Roles));
      
       // Notificar cambio de autenticación
      NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(principal)));
      
                return true;
  }

       _logger.LogError("? HttpContext es null, no se puede crear cookie");
      return false;
    }
 catch (Exception ex)
        {
      _logger.LogError(ex, "? Error durante login de {Username}", username);
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
      
     // ? Verificar que la respuesta NO haya comenzado
      if (!httpContext.Response.HasStarted)
         {
          // ? Eliminar cookie de autenticación
       await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

               _logger.LogInformation("? Logout exitoso para usuario: {Username}", username);
       }
              else
    {
             _logger.LogWarning("?? No se puede eliminar cookie: Response ya comenzó");
          }

       // Notificar cambio
     NotifyAuthenticationStateChanged(Task.FromResult(
        new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
     }
      else
 {
          _logger.LogWarning("?? HttpContext es null en LogoutAsync");
}
        }
        catch (Exception ex)
        {
        _logger.LogError(ex, "? Error durante logout");
        }
    }

    // ==================== DATOS DUMMY ====================

    /// <summary>
    /// ?? USUARIOS DE PRUEBA - Reemplazar con consulta a base de datos en producción
    /// </summary>
    private static readonly List<DummyUser> DummyUsers = new()
    {
        // Usuario 1: Administrador (acceso completo a todo)
        new DummyUser
     {
      Id = "user-001",
            Username = "admin",
            Email = "admin@vrm.com",
         NombreCompleto = "Administrador del Sistema",
            ClienteId = "cliente-001",
         Roles = new List<string> { "Admin" }
    },

        // Usuario 1b: Usuario Regular (acceso limitado)
        new DummyUser
        {
            Id = "user-001b",
          Username = "user",
            Email = "user@vrm.com",
     NombreCompleto = "Usuario Regular",
            ClienteId = "cliente-001",
 Roles = new List<string> { "User" }
        },

 // Usuario 2: Gerente de Finanzas (puede timbrar facturas y ver reportes sensibles)
  new DummyUser
        {
       Id = "user-002",
            Username = "gerente.finanzas",
      Email = "gerente.finanzas@vrm.com",
            NombreCompleto = "Juan Gerente de Finanzas",
        ClienteId = "cliente-001",
          Roles = new List<string> { "GerenteFinanzas" }
        },

        // Usuario 3: Coordinador de Finanzas (puede crear/editar pero NO timbrar)
        new DummyUser
 {
            Id = "user-003",
            Username = "coordinador.finanzas",
    Email = "coordinador.finanzas@vrm.com",
            NombreCompleto = "María Coordinadora de Finanzas",
            ClienteId = "cliente-001",
  Roles = new List<string> { "CoordinadorFinanzas" }
        },

        // Usuario 4: Contador (solo lectura de finanzas)
        new DummyUser
        {
         Id = "user-004",
            Username = "contador",
            Email = "contador@vrm.com",
            NombreCompleto = "Pedro Contador",
            ClienteId = "cliente-001",
            Roles = new List<string> { "Contador" }
        },

        // Usuario 5: Gestor de Prospectos
 new DummyUser
        {
          Id = "user-005",
            Username = "gestor.prospectos",
       Email = "gestor.prospectos@vrm.com",
            NombreCompleto = "Ana Gestora de Prospectos",
            ClienteId = "cliente-001",
            Roles = new List<string> { "GestorProspectos" }
    },

        // Usuario 6: Revisor Legal
     new DummyUser
        {
            Id = "user-006",
            Username = "revisor.legal",
    Email = "revisor.legal@vrm.com",
 NombreCompleto = "Carlos Revisor Legal",
       ClienteId = "cliente-001",
 Roles = new List<string> { "RevisorLegal" }
        },

        // Usuario 7: Revisor Financiero (solo revisa área financiera de prospectos)
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

    /// <summary>
    /// Obtiene la lista completa de usuarios dummy (útil para la UI de login)
    /// </summary>
    public static List<DummyUser> GetAllDummyUsers()
    {
        return DummyUsers;
}
}

/// <summary>
/// Modelo de usuario dummy para desarrollo.
/// ?? En producción, usar entidad de base de datos.
/// </summary>
public class DummyUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string ClienteId { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}
