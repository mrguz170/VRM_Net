using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Core.Abstractions.Common;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Core.Abstractions.Data.Repositories;
using VRM_Plugin.Module.PanelAdmin.Data.DTOs;

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

    public Task<bool> CreateNewRole(RolDto rolDto)
    {
        try
        {
            var users = _db.ExecuteNonQuery("sp_set_roles", new Dictionary<string, object>
            {
                { "role_name", rolDto.role_name },
                { "description", rolDto.description },
                { "crater_user", rolDto.create_user_id }
            });
        }catch (Exception ex)
        {
            Console.WriteLine($"Error creating new role: {ex.Message}");
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

    public Task<List<RolDto>> Getall()
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

    public Task UpdateRole(RolDto rolDto)
    {
        try
        {
            var users = _db.ExecuteNonQuery("sp_update_role", new Dictionary<string, object>
            {
                { "role_name", rolDto.role_name },
                { "description", rolDto.description },
                { "is_active", rolDto.is_active },
                { "update_user", rolDto.create_user_id }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating new role: {ex.Message}");
            return Task.FromResult(false);
        }
        return Task.FromResult(true);
    }

    public Task<List<RolDto>> DeleteRol(RolDto rolDto)
    {
        List<RolDto> roles = new List<RolDto>();
        try
        {
            roles = _db.ExecuteStoredProcedure<RolDto>("sp_delete_role", new Dictionary<string, object>
            {
                { "id_role", rolDto.role_id }
            });
            return Task.FromResult(roles);
        }
        catch (Exception ex)
        {
            return Task.FromResult(roles);
        }
    }

    /// <summary>
    /// Actualiza los roles asignados a un componente específico
    /// </summary>
    public async Task<bool> UpdateComponentRoles(int componentId, List<int> roleIds, string userId)
    {
        try
        {
            var roleIdsCsv = string.Join(",", roleIds);

            var parameters = new Dictionary<string, object>
        {
            { "p_component_id", componentId },
            { "p_role_ids", roleIdsCsv },  
            { "p_user_id", userId }
        };

            _db.ExecuteStoredProcedure("sp_set_component_roles", parameters);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    /// <summary>
    /// Actualiza los roles asignados a un componente específico (versión asincrónica)
    /// </summary>
    public async Task<bool> UpdateComponentRolesAsync(int componentId, List<int> roleIds, string userId)
    {
        try
        {
            // Convertir lista a CSV: [1,2,3] → "1,2,3"
            var roleIdsCsv = string.Join(",", roleIds);
            
            var parameters = new Dictionary<string, object>
            {
                { "p_component_id", componentId },
                { "p_role_ids", roleIdsCsv },
                { "p_user_id", userId }
            };
            
            _db.ExecuteNonQuery("sp_set_component_roles", parameters);
            return await Task.FromResult(true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar roles del componente {componentId}: {ex.Message}");
            return await Task.FromResult(false);
        }
    }
}
