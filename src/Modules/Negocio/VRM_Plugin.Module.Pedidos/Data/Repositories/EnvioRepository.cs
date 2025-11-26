using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.Pedidos.Data.DTOs;

namespace VRM_Plugin.Module.Pedidos.Data.Repositories;

/// <summary>
/// Implementación base del repository de Envio (solo ejemplo)
/// </summary>
public class EnvioRepository : IEnvioRepository
{
    private readonly List<EnvioDto> _items = new();

    public Task<List<EnvioDto>> GetSampleAsync()
    {
        return Task.FromResult(_items);
    }
}
