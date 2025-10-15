using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace VRM_PluginDemo.Blazor.Server.Services;

/// <summary>
/// Proveedor de autenticación simulado con usuarios dummy.
/// ?? SOLO PARA DESARROLLO - Reemplazar con autenticación real en producción.
/// 
/// NOTA: Usa cache en memoria únicamente (no persistente entre recargas).
/// Para producción, usar Cookies o ASP.NET Core Identity.
/// </summary>
public class DummyAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ILogger<DummyAuthenticationStateProvider> _logger;
    private ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
    
    // Cache en memoria del usuario actual
    private DummyUser? _cachedUser;

    public DummyAuthenticationStateProvider(
        ILogger<DummyAuthenticationStateProvider> logger)
    {
        _logger = logger;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            // Usar solo el cache en memoria (evita problemas con JavaScript interop)
            if (_cachedUser != null)
            {
                _logger.LogDebug("Usuario autenticado: {Username}", _cachedUser.Username);
                return Task.FromResult(new AuthenticationState(CreateClaimsPrincipal(_cachedUser)));
            }

            return Task.FromResult(new AuthenticationState(_currentUser));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estado de autenticación");
            return Task.FromResult(new AuthenticationState(_currentUser));
        }
    }

    /// <summary>
    /// Simula un login con usuario y contraseña.
    /// ?? En producción, esto validaría contra una base de datos con contraseñas hasheadas.
    /// </summary>
    public Task<bool> LoginAsync(string username, string password)
    {
        try
        {
            _logger.LogInformation("?? Intentando login para usuario: {Username}", username);

            // Buscar usuario por username
            var usuario = GetDummyUserByUsername(username);

            if (usuario == null)
            {
                _logger.LogWarning("? Intento de login fallido: usuario {Username} no existe", username);
                return Task.FromResult(false);
            }

            // ?? VALIDACIÓN DUMMY - En producción, verificar hash de contraseña
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("? Intento de login fallido: contraseña vacía para {Username}", username);
                return Task.FromResult(false);
            }

            // Cachear el usuario en memoria
            _cachedUser = usuario;
            _currentUser = CreateClaimsPrincipal(usuario);

            _logger.LogInformation("? Login exitoso para {Username} con roles: [{Roles}]", 
                username, 
                string.Join(", ", usuario.Roles));
            
            // Notificar cambio de autenticación
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "?? Error durante login de {Username}", username);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Cierra sesión del usuario actual.
    /// </summary>
    public Task LogoutAsync()
    {
        try
        {
            var username = _cachedUser?.Username ?? "desconocido";
            
            // Limpiar cache
            _cachedUser = null;
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());

            _logger.LogInformation("Logout exitoso para usuario: {Username}", username);

            // Notificar cambio
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante logout");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Crea un ClaimsPrincipal a partir de un DummyUser.
    /// </summary>
    private ClaimsPrincipal CreateClaimsPrincipal(DummyUser usuario)
    {
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

        var identity = new ClaimsIdentity(claims, "DummyAuth");
        return new ClaimsPrincipal(identity);
    }

    // ==================== DATOS DUMMY ====================

    /// <summary>
    /// ? USUARIOS DE PRUEBA - Reemplazar con consulta a base de datos en producción
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

    private DummyUser? GetDummyUser(string userId)
    {
        return DummyUsers.FirstOrDefault(u => u.Id == userId);
    }

    private DummyUser? GetDummyUserByUsername(string username)
    {
        return DummyUsers.FirstOrDefault(u =>
            u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
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
