using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRM_Plugin.Module.Pedidos.Data.DTOs;
using VRM_Plugin.Module.Pedidos.Data.Repositories;
using VRM_Plugin.Module.Pedidos.Domain;

namespace VRM_Plugin.Module.Pedidos.Services;

/// <summary>
/// Implementación base del servicio de Envio
/// Cambia la implementación según las necesidades del componente.
/// </summary>
public class EnvioService : IEnvioService
{
    private readonly IEnvioRepository? _repo;

    public EnvioService(IEnvioRepository? repo = null)
    {
        _repo = repo;
    }

    public async Task<List<EnvioDto>> GetAllAsync()
    {
        if (_repo != null) return await _repo.GetSampleAsync();
        return await Task.FromResult(new List<EnvioDto>());
    }

    public async Task<EnvioDto?> GetByIdAsync(int id)
    {
        var list = await GetAllAsync();
        return list.FirstOrDefault(x => x.Id == id);
    }

    public async Task<EnvioDto> CreateAsync(EnvioDto dto)
    {
        
        return await Task.FromResult(dto);
    }

    public async Task<EnvioDto> UpdateAsync(EnvioDto dto)
    {
        
        return await Task.FromResult(dto);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        
        return await Task.FromResult(false);
    }
}
