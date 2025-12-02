using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.PanelAdmin.Data.DTOs;

namespace VRM_Plugin.Module.PanelAdmin.Data.Repositories;

/// <summary>
/// Implementación base del repository de Rol (solo ejemplo)
/// </summary>
public class RolRepository : IRolRepository
{
    private readonly List<RolDto> _items = new();

    public Task<List<RolDto>> GetSampleAsync()
    {
        return Task.FromResult(_items);
    }
}
