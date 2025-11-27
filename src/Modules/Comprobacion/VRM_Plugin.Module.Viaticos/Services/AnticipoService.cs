using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VRM_Plugin.Module.Viaticos.Data.DTOs;
using VRM_Plugin.Module.Viaticos.Data.Repositories;
using VRM_Plugin.Module.Viaticos.Domain;

namespace VRM_Plugin.Module.Viaticos.Services;

/// <summary>
/// Implementación base del servicio de Anticipo
/// Cambia la implementación según las necesidades del componente.
/// </summary>
public class AnticipoService : IAnticipoService
{
    private readonly IAnticipoRepository? _repo;

    public AnticipoService(IAnticipoRepository? repo = null)
    {
        _repo = repo;
    }

    public async Task<List<AnticipoDto>> GetAllAsync()
    {
        if (_repo != null) return await _repo.GetSampleAsync();
        return await Task.FromResult(new List<AnticipoDto>());
    }

    public async Task<AnticipoDto?> GetByIdAsync(int id)
    {
        var list = await GetAllAsync();
        return list.FirstOrDefault(x => x.Id == id);
    }

    public async Task<AnticipoDto> CreateAsync(AnticipoDto dto)
    {
        
        return await Task.FromResult(dto);
    }

    public async Task<AnticipoDto> UpdateAsync(AnticipoDto dto)
    {
        
        return await Task.FromResult(dto);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        
        return await Task.FromResult(false);
    }
}
