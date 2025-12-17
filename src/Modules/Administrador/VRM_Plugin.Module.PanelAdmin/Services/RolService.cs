using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
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
        var res = await _repo.GetallRoles();
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

    public async Task<Rol> GetByIdAsync(int id)
    { 
        var list = await GetAllAsync();
        return list.FirstOrDefault(x => x.Id == id);
    }

    public Task<bool> CreateAsync(RolDto dto, int opc)
    {
        var item = _repo.CreateNewRole(dto,opc);
        return item;
    }

   public async Task<List<Modules>> GetModules()
    {

        var res = await _repo.GetallModules();

        List<Modules> modules = new List<Modules>();

        foreach (var item in res)
        {
            modules.Add(new Modules
            {
                Moduleid = item.module_id,
                ModuleName = item.module_name
            });
        }

        if(modules.Count > 0)
            return modules;

        return await Task.FromResult(modules);

    }

   public async Task<List<Component>> GetComponentById(int idMod) { 
    var components = await _repo.GetallComponentsByModule(idMod); 
        return components.Select(MapDtoToDomainComponent).ToList();

    }

    public async Task<List<Actions>> GetAllActions(int moduleid)
    {
        var Actions = await _repo.GetallActionsByComponent(moduleid);
        return Actions.Select(MapDtoToDomainAction).ToList();

    }

    public async Task<bool> SetActionRoles(ActionRoles action)
    {
        var res = await _repo.SetActionRoles(MapDomainToDtoAction(action));
        return res;
    }

    public async Task<List<Rol>> GetActionByRole(ulong actionid)
    { 
      var roles = await _repo.GetRolesByActionAsync(actionid);
        return roles.Select(MapDtoToDomainRole).ToList();

    }



    private Actions MapDtoToDomainAction(ActionDto u)
    {
        return new Actions
        {
            Actionid = u.action_key_id,
            Actionname = u.action_name ?? string.Empty,
            Componentid = u.component_id         
        };
    }

    private Component MapDtoToDomainComponent(ComponentDto u)
    {
        return new Component
        {
            Componentid = u.component_id ,
            ComponentName = u.name ?? string.Empty,
            Parentid = u.parent_id
        };
    }
    private Modules MapDtoToDomainModule(ModulesDto u)
    {
        return new Modules
        {
            Moduleid = u.module_id,
            ModuleName = u.module_name ?? string.Empty
        };
    }

    private Rol MapDtoToDomainRole(RolDto u)
    {
        return new Rol
        {
            Id = u.role_id,
            Nombre = u.role_name ?? string.Empty,
            Activo = u.is_active
        };
    }

    private ActionRoleDto MapDomainToDtoAction(ActionRoles u)
    {
        return new ActionRoleDto
        {
            action_key_id = u.key_id,
            role_id = u.role_id,
            created_user_id = u.modificated_user,
            Activo = u.Activo
        };


    /// <summary>
    /// Actualiza permisos de componentes (múltiples a la vez)
    /// </summary>
    public async Task<bool> UpdateComponentPermissionsAsync(Dictionary<int, List<int>> componentRoles, string userId)
    {
        try
        {
            foreach (var kvp in componentRoles)
            {
                var componentId = kvp.Key;
                var roleIds = kvp.Value;
                
                var resultado = await _repo.UpdateComponentRolesAsync(componentId, roleIds, userId);
                
                if (!resultado)
                {
                    return false;
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar permisos: {ex.Message}");
            return false;
        }
    }
}
