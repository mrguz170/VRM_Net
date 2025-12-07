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

    public Task CreateNewRole(RolDto rolDto)
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
            });
            return Task.FromResult(roles);
        }
        catch (Exception ex)
        {
            return Task.FromResult(roles);
        }

    }
}
