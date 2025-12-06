using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRM_Plugin.Module.PanelAdmin.Data.DTOs;
using VRM_Plugin.Module.PanelAdmin.Data.Repositories;
using VRM_Plugin.Module.PanelAdmin.Domain;

namespace VRM_Plugin.Module.PanelAdmin.Services;

/// <summary>
/// Implementación base del servicio de Rol
/// Cambia la implementación según las necesidades del componente.
/// </summary>
public class RolService : IRolService
{
    private readonly IRolRepository? _repo;

    public RolService(IRolRepository? repo = null)
    {
        _repo = repo;
    }

    public async Task<List<RolDto>> GetAllAsync()
    {
        if (_repo != null) return await _repo.Getall();
        return await Task.FromResult(new List<RolDto>());
    }

    public async Task<RolDto?> GetByIdAsync(int id)
    {
        var list = await GetAllAsync();
        return list.FirstOrDefault(x => x.role_id == id);
    }

    public Task CreateAsync(RolDto dto)
    {
        var res = _repo.CreateNewRole(dto);
        return res ;
    }

    public async Task<RolDto> UpdateAsync(RolDto dto)
    {
        
        return await Task.FromResult(dto);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        
        return await Task.FromResult(false);
    }
}
