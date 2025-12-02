using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Core.Abstractions.Services;
using VRM_Plugin.Module.PanelAdmin.Data.DTOs;
using VRM_Plugin.Module.PanelAdmin.Data.Repositories;
using VRM_Plugin.Module.PanelAdmin.Domain;

namespace VRM_Plugin.Module.PanelAdmin.Services;

/// <summary>
/// Implementación base del servicio de Usuario
/// Cambia la implementación según las necesidades del componente.
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUserRepository? _repo;

    public UsuarioService(IUserRepository? repo = null)
    {
        _repo = repo;
    }
    /// <summary>
    /// Obtiene todos los usuarios
    /// </summary>
    /// <returns></returns>
    public async Task<List<UserDto>> GetAllAsync()
    {
        if (_repo != null) return await _repo.GetAllUserAsync();
        return await Task.FromResult(new List<UserDto>());
    }
    /// <summary>
    /// Obtiene un usuario por ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var list = await GetAllAsync();
        return list.FirstOrDefault(x => x.UserId == id.ToString());
    }
    /// <summary>
    /// Crear un nuevo usuario
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<UserDto> CreateAsync(UserDto dto)
    {       
        return await Task.FromResult(dto);
    }

    /// <summary>    
    /// Actualiza un usuario existente
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<UserDto> UpdateAsync(UserDto dto)
    {
        
        return await Task.FromResult(dto);
    }
    /// <summary>
    /// Inactiva o elimina un usuario por ID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(int id)
    {
        
        return await Task.FromResult(false);
    }
}
