using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using VRM_Plugin.Core.Abstractions.Common;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Core.Abstractions.Data.Repositories;
using VRM_Plugin.Module.PanelAdmin.Components;
using VRM_Plugin.Module.PanelAdmin.Data.DTOs;
using VRM_Plugin.Module.PanelAdmin.Domain;

namespace VRM_Plugin.Module.PanelAdmin.Data.Repositories;

/// <summary>
/// Implementación base del repository de Rol (solo ejemplo)
/// </summary>
public class RolRepository : IRolRepository
{
    private readonly DatabaseHelper _db;
    private readonly List<RolDto> _items = new();

    public RolRepository(
    IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection not found");

        _db = new DatabaseHelper(connectionString);
    }
    public Task<List<RolDto>> GetSampleAsync()
    {
        return Task.FromResult(_items);
    }

    public Task<bool> CreateNewRole(RolDto rolDto,int opc)
    {
        try
        {
            var users = _db.ExecuteNonQuery("sp_set_roles", new Dictionary<string, object>
            {
                { "Opc", opc},
                { "role_name", rolDto.role_name },
                { "description", rolDto.description },
                { "user_modify", rolDto.create_user_id },
                { "is_active", rolDto.is_active },
                { "ID_rol", rolDto.role_id },
            });
        }catch (Exception ex)
        {
            Console.WriteLine($"Error creating new role: {ex.Message}");
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

    public Task<List<RolDto>> GetallRoles()
    {
        List<RolDto> roles = new List<RolDto>();
        try
        {
            roles = _db.ExecuteStoredProcedure<RolDto>("sp_get_all_roles", new Dictionary<string, object>
            {
                    { "Opc", 1 },
                    { "action_key_id", 0 }
            });
            return Task.FromResult(roles);
        }
        catch (Exception ex)
        {
            return Task.FromResult(roles);
        }

    }

    public Task<List<ModulesDto>> GetallModules()
    {
        List<ModulesDto> modules = new List<ModulesDto>();
        try
        {
            modules = _db.ExecuteStoredProcedure<ModulesDto>("sp_get_module_info", new Dictionary<string, object>
            {
                 { "Opc", 1 },
                 { "action_key_id", 0 }
            });
            return Task.FromResult(modules);
        }
        catch (Exception ex)
        {
            return Task.FromResult(modules);
        }

    }

    public Task<List<ComponentDto>> GetallComponentsByModule(int moduleid)
    {
        List<ComponentDto> components = new List<ComponentDto>();
        try
        {
            components = _db.ExecuteStoredProcedure<ComponentDto>("sp_get_component", new Dictionary<string, object>
            {
                {"module_id",moduleid }
            });
            return Task.FromResult(components);
        }
        catch (Exception ex)
        {
            return Task.FromResult(components);
        }

    }

    public Task<List<ActionDto>> GetallActionsByComponent(int moduleid)
    {
        List<ActionDto> actions = new List<ActionDto>();
        try
        {
            actions = _db.ExecuteStoredProcedure<ActionDto>("sp_get_actions", new Dictionary<string, object>
            {
                {"module_id",moduleid },
                {"Opcion",2 }
            });
            return Task.FromResult(actions);
        }
        catch (Exception ex)
        {
            return Task.FromResult(actions);
        }

    }

    public Task<bool> SetActionRoles(ActionRoleDto actionroles)
    {
        try
        {
            var users = _db.ExecuteNonQuery("sp_set_action_role", new Dictionary<string, object>
            {
                { "p_action_key_id", actionroles.action_key_id},
                { "p_role_id", actionroles.role_id },
                { "p_is_active", actionroles.Activo },
                { "p_modificated_user_id", actionroles.created_user_id  }

            });
         }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating new role: {ex.Message}");
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

    /// <summary>
    /// Obtiene los IDs de roles que tienen acceso a una acción
    /// Llama al SP: sp_get_roles_by_action
    /// </summary>
    public async Task<List<RolDto>> GetRolesByActionAsync(ulong actionKeyId)
    {
        try
        {

            var result = await Task.Run(() =>
                _db.ExecuteStoredProcedure<RolDto>("sp_get_all_roles", new Dictionary<string, object>
                {
                    { "Opc", 2 },
                    { "action_key_id", actionKeyId }
                }));

            return result;
        }
        catch (Exception ex)
        {

            return new List<RolDto>();
        }
    }

}
