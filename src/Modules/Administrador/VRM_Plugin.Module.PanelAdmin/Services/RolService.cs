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

    public async Task<List<Rol?>> GetAllAsync()
    {
        var res = await _repo.Getall();
        List<Rol?> roles = new List<Rol?>();

        foreach (var item in res)
        {
            roles.Add(new Rol
            {
                Id = item.role_id,
                Nombre = item.role_name,
                Descripcion = item.description,
                Activo = item.is_active,
                Fecha = item.updated_date != default(DateTime) ? item.updated_date : item.created_date,
                UserCreated = item.user_name
            });
        }

        if (roles.Count>0) 
            return roles;

        return await Task.FromResult(roles);
    }

    public async Task<Rol?> GetByIdAsync(int id)
    {
        var list = await GetAllAsync();
        return list.FirstOrDefault(x => x.Id == id);
    }

    public Task<bool> CreateAsync(RolDto dto)
    {
        var item = _repo.CreateNewRole(dto);
        return item;
    }

    public async Task<RolDto> UpdateAsync(RolDto dto)
    {
        
        return await Task.FromResult(dto);
    }

}
