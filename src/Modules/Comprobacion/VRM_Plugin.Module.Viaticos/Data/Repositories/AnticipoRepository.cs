using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.Viaticos.Data.DTOs;

namespace VRM_Plugin.Module.Viaticos.Data.Repositories;

/// <summary>
/// Implementación base del repository de Anticipo (solo ejemplo)
/// </summary>
public class AnticipoRepository : IAnticipoRepository
{
    private readonly List<AnticipoDto> _items = new();

    public Task<List<AnticipoDto>> GetSampleAsync()
    {
        return Task.FromResult(_items);
    }
}
