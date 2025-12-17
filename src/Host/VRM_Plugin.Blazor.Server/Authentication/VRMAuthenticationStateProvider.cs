using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Security.Claims;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Core.Abstractions.Services;

namespace VRM_Plugin.Blazor.Server.Authentication;

/// <summary>
/// Proveedor de AUTENTICACIÓN para VRM_Plugin.
/// Gestión de identidad del usuario
///	Login/Logout
///	Persistencia de estado de autenticación
///	Creación y validación de Claims
///	Manejo de cookies de sesión
/// /// </summary>
public class VRMAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly ILogger<VRMAuthenticationStateProvider> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly PersistentComponentState _persistentState;
    private readonly IUserRepository _userRepository;
    
    private PersistingComponentStateSubscription _subscription;
    private AuthenticationState? _authenticationState;

    public VRMAuthenticationStateProvider(
        ILogger<VRMAuthenticationStateProvider> logger,
        IHttpContextAccessor httpContextAccessor,
        PersistentComponentState persistentState,
        IUserRepository userRepository)
    {
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _persistentState = persistentState;
        _userRepository = userRepository;
        
        _subscription = persistentState.RegisterOnPersisting(OnPersistingAsync);
        
        _logger.LogInformation("? [Auth] VRMAuthenticationStateProvider inicializado");
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
                    //_logger.LogInformation("? [SSR] Usuario autenticado desde HttpContext: {Username}", 
                    //    httpContext.User.Identity.Name);
              
                    _authenticationState = new AuthenticationState(httpContext.User);
                    return _authenticationState;
                }
                else
                {
                    _logger.LogDebug("?? [SSR] Usuario NO autenticado en HttpContext");
                }
            }
            
            // FASE 2: Interactive Server (Circuit) - Lee del estado persistido
            // ? Usar UserDto unificado
            if (_persistentState.TryTakeFromJson<UserDto>("UserInfo", out var userDto))
            {
                _logger.LogInformation("? [Circuit] Usuario restaurado desde estado persistido: {Email} con rol: {Role}", 
                    userDto.Email,
                    userDto.Role);
         
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, userDto.UserId),
                    new Claim(ClaimTypes.Name, userDto.Username),
                    new Claim(ClaimTypes.Email, userDto.Email),
                    new Claim("NombreCompleto", userDto.NombreCompleto),
                    new Claim("RoleId", userDto.RoleId)
                };
          
                // ? Agregar el rol único del usuario
                if (!string.IsNullOrWhiteSpace(userDto.Role))
                {
                    claims.Add(new Claim(ClaimTypes.Role, userDto.Role));
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
            _logger.LogError(ex, "? [Auth] Error al obtener estado de autenticación");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    private Task OnPersistingAsync()
    {
        if (_authenticationState?.User.Identity?.IsAuthenticated ?? false)
        {
            var user = _authenticationState.User;
            
            var userDto = new UserDto
            {
                UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "",
                Username = user.Identity.Name ?? "",
                Email = user.FindFirst(ClaimTypes.Email)?.Value ?? "",
                NombreCompleto = user.FindFirst("NombreCompleto")?.Value ?? "",
                RoleId = user.FindFirst("RoleId")?.Value ?? "",
                Role = user.FindFirst(ClaimTypes.Role)?.Value ?? ""
            };
         
            _persistentState.PersistAsJson("UserInfo", userDto);
        
            //_logger.LogInformation("? [Persist] Estado de autenticación persistido para: {Email}", 
            //    userDto.Email);
        }
        else
        {
            _logger.LogDebug("?? [Persist] No hay usuario autenticado para persistir");
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Autentica un usuario usando UserDto
    /// </summary>
    public async Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            //_logger.LogInformation("?? [Auth] Intentando login para usuario: {Username}", username);

            // ? Usar repositorio que devuelve UserDto
            var usuario = _userRepository.GetUserByLogin(username, password);

            if (usuario == null)
            {
                _logger.LogWarning("? [Auth] Usuario {Username} no encontrado o contraseña incorrecta", username);
                return false;
            }

            //_logger.LogInformation("? [Auth] Usuario encontrado: {Username} con rol: {Role}", 
            //    usuario.Username, 
            //    usuario.Role);

            // Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UserId),
                new Claim(ClaimTypes.Name, usuario.Username),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("NombreCompleto", usuario.NombreCompleto),
                new Claim("RoleId", usuario.RoleId)
            };

            // Agregar el rol único del usuario
            if (!string.IsNullOrWhiteSpace(usuario.Role))
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.Role));
                _logger.LogDebug("? [Auth] Rol agregado: {Role}", usuario.Role);
            }

            // Crear identity y principal
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            // Crear cookie de autenticación
            var httpContext = _httpContextAccessor.HttpContext;
            
            if (httpContext == null)
            {
                _logger.LogError("? [Auth] HttpContext es null, no se puede crear cookie");
                return false;
            }

            if (httpContext.Response.HasStarted)
            {
                _logger.LogError("? [Auth] Response ya comenzó. No se puede escribir cookie.");
                return false;
            }

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1),
                    AllowRefresh = true
                });

            _logger.LogInformation("? [Auth] Cookie de autenticación creada exitosamente para {Username}", username);
 
            _authenticationState = new AuthenticationState(principal);
      
            NotifyAuthenticationStateChanged(Task.FromResult(_authenticationState));
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "? [Auth] Error durante login de {Username}", username);
            return false;
        }
    }

    /// <summary>
    /// Cierra sesión del usuario actual
    /// </summary>
    public async Task LogoutAsync()
    {
        try
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var username = httpContext.User?.Identity?.Name ?? "desconocido";
     
                if (!httpContext.Response.HasStarted)
                {
                    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                    _logger.LogInformation("? [Auth] Logout exitoso para usuario: {Username}", username);
                }
                else
                {
                    _logger.LogWarning("?? [Auth] No se puede eliminar cookie: Response ya comenzó");
                }

                _authenticationState = null;
   
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
            _logger.LogError(ex, "? [Auth] Error durante logout");
        }
    }
    
    public void Dispose()
    {
        _subscription.Dispose();
    }

}
