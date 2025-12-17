using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRM_Plugin.Blazor.Server.Security;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Core.Abstractions.Data.Repositories;
using VRM_Plugin.Core.Abstractions.Services;
using VRM_Plugin.Module.PanelAdmin.Data.Repositories;
using VRM_Plugin.Module.PanelAdmin.Domain;

namespace VRM_Plugin.Module.PanelAdmin.Services;

/// <summary>
/// Implementación base del servicio de Usuario
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUserRepository? _repo;
    private readonly ICurrentUserService _currentUserService;

    public UsuarioService(
       IUserRepository? repo = null,
       ICurrentUserService? currentUserService = null) 
    {
        _repo = repo;
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
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
    public async Task<Usuario?> GetUserByIdAsync(string id)
    {
        var list = await GetAllUserAsync();
        return list.FirstOrDefault(x => x.Id == id);
    } 
    /// <summary>
    /// Crear un nuevo usuario
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Usuario> CreateUserAsync(Usuario usuario)
    {
        var dto = MapDomainToDto(usuario);

        dto.created_user_id = await _currentUserService.GetUserIdAsync() ?? "SYSTEM";
        dto.Password = PasswordHasher.GenerateAndHashDefaultPassword(dto.Nombre, dto.ApellidoPaterno).HashedPassword;
        dto.IsActive = false; // Nuevo usuario inactivo por defecto

        var userId = await _repo.CreateUserAsync(dto);
        usuario.Id = userId;
        return usuario;
    }

    /// <summary>    
    /// Actualiza un usuario existente
    /// </summary>
    /// <param name="usuario"></param>
    /// <returns></returns>
    public async Task<Usuario> UpdateUserAsync(Usuario usuario)
    {
        var dto = MapDomainToDto(usuario);
        
        // Obtener el usuario que está actualizando
        dto.updated_user_id = await _currentUserService.GetUserIdAsync() ?? "SYSTEM";
        
        // Verificar si se debe regenerar la contraseña
        if (usuario.RegenerarPassword)
        {
            // Generar nueva contraseña por defecto (igual que en creación)
            dto.Password = PasswordHasher.GenerateAndHashDefaultPassword(dto.Nombre, dto.ApellidoPaterno).HashedPassword;
        }
        else
        {
            // NO actualizar la contraseña (mantener la existente)
            dto.Password = null;
        }
        
        // Llamar al repositorio para actualizar
        var userId = await _repo.UpdateUserAsync(dto);
        usuario.Id = userId;
        
        return usuario;
    }
    /// <summary>
    /// Inactiva o elimina un usuario por ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> DeleteUserAsync(string id)
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
            RoleId = u.RoleId ?? string.Empty,
            Status = u.IsActive ? "Activo" : "Inactivo",
            FechaModificacion = u.UpdatedDate,
            
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
            RoleId = u.RoleId,                                       
            IsActive = u.Status == "Activo",
            UpdatedDate = u.FechaModificacion                      
        };
    }
}
