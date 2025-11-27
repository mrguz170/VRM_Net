using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.Viaticos.Domain;
using VRM_Plugin.Module.Viaticos.Data.DTOs;

namespace VRM_Plugin.Module.Viaticos.Services;

/// <summary>
/// Servicio base para gestionar Anticipo
/// Cambia la implementación según las necesidades del componente.
/// </summary>
public interface IAnticipoService
{
    Task<List<AnticipoDto>> GetAllAsync();
    Task<AnticipoDto?> GetByIdAsync(int id);
    Task<AnticipoDto> CreateAsync(AnticipoDto dto);
    Task<AnticipoDto> UpdateAsync(AnticipoDto dto);
    Task<bool> DeleteAsync(int id);
}
