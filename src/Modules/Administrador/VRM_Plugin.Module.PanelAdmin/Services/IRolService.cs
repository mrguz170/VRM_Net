using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.PanelAdmin.Domain;
using VRM_Plugin.Module.PanelAdmin.Data.DTOs;

namespace VRM_Plugin.Module.PanelAdmin.Services;

/// <summary>
/// Servicio base para gestionar Rol
/// Cambia la implementación según las necesidades del componente.
/// </summary>
public interface IRolService
{
    Task<List<Rol>> GetAllAsync();
    Task<Rol?> GetByIdAsync(int id);
    Task<bool> CreateAsync(RolDto dto);
    Task<RolDto> UpdateAsync(RolDto dto);
}
