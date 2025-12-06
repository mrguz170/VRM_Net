using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using VRM_Plugin.Blazor.Server.Authentication;
using VRM_Plugin.Blazor.Server.Security;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Core.Abstractions.Data.Repositories;
using VRM_Plugin.Core.Abstractions.Services;
using VRM_Plugin.Module.PanelAdmin.Data.DTOs;
using VRM_Plugin.Module.PanelAdmin.Data.Repositories;
using VRM_Plugin.Module.PanelAdmin.Domain;
using static MudBlazor.CategoryTypes;

namespace VRM_Plugin.Module.PanelAdmin.Services;

/// <summary>
/// Implementación base del servicio de Usuario
/// Cambia la implementación según las necesidades del componente.
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUserRepository? _repo;
    private readonly VRMAuthenticationStateProvider _authProvider; 

    public UsuarioService(
       IUserRepository? repo = null,
       VRMAuthenticationStateProvider authProvider = null) 
    {
        _repo = repo;
        _authProvider = authProvider;
    }
    /// <summary>
    /// Obtiene todos los usuarios
    /// </summary>
    /// <returns></returns>
    public async Task<List<Usuario>> GetAllUserAsync()
    {
        if (_repo != null) 
        {        
            var dtos = await _repo.GetAllUserAsync();
            return dtos.Select(MapDtoToDomain).ToList();
        }
        else       
            return await Task.FromResult(new List<Usuario>());     
    }
    /// <summary>
    /// Obtiene un usuario por ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Usuario?> GetUserByIdAsync(int id)
    {
        var list = await GetAllUserAsync();
        return list.FirstOrDefault(x => x.Id == id.ToString());
    } 
    /// <summary>
    /// Crear un nuevo usuario
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Usuario> CreateUserAsync(Usuario usuario)
    {
        var dto = MapDomainToDto(usuario);

        dto.created_user_id = await _authProvider.GetUserIdAsync(); 

        var userId = await _repo.CreateUserAsync(dto);
        usuario.Id = userId;
        return usuario;
    }

    /// <summary>    
    /// Actualiza un usuario existente
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Usuario> UpdateUserAsync(Usuario dto)
    {
        // Mapear directamente el DTO a Usuario
        return new Usuario();
    }
    /// <summary>
    /// Inactiva o elimina un usuario por ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> DeleteUserAsync(int id)
    {
        
        return await Task.FromResult(false);
    }

    // ==================== MÉTODOS PRIVADOS ====================

    /// <summary>
    /// Convierte DTO (BD) → Domain (negocio)
    /// </summary>
    private Usuario MapDtoToDomain(UserDto u)
    {
        return new Usuario
        {
            Id = u.UserId ?? "0",
            Username = u.Username ?? string.Empty,
            Nombre = u.Nombre ?? string.Empty,                    
            ApellidoPaterno = u.ApellidoPaterno ?? string.Empty,        
            ApellidoMaterno = u.ApellidoMaterno ?? string.Empty,        
            NombreCompleto = u.NombreCompleto ?? string.Empty,
            Email = u.Email ?? string.Empty,
            Role = u.Role ?? string.Empty,
            Status = u.IsActive ? "Activo" : "Inactivo",
            FechaModificacion = u.UpdatedDate
        };
    }

    /// <summary>
    /// Convierte Domain (negocio) → DTO (BD)
    /// </summary>
    private UserDto MapDomainToDto(Usuario u)
    {
        return new UserDto
        {
            UserId = u.Id,
            Username = u.Username,
            Nombre = u.Nombre,
            ApellidoPaterno = u.ApellidoPaterno,
            ApellidoMaterno = u.ApellidoMaterno,
            NombreCompleto = u.NombreCompleto,                    
            Email = u.Email,
            Role = u.Role,                                         
            RoleId = u.Role,                                       
            IsActive = u.Status == "Activo",
            UpdatedDate = u.FechaModificacion                      
        };
    }
}
