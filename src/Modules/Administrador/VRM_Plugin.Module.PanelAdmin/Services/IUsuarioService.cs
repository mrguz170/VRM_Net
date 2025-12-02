using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Module.PanelAdmin.Data.DTOs;
using VRM_Plugin.Module.PanelAdmin.Domain;

namespace VRM_Plugin.Module.PanelAdmin.Services;

/// <summary>
/// Servicio base para gestionar Usuario
/// Cambia la implementación según las necesidades del componente.
/// </summary>
public interface IUsuarioService
{
    Task<List<UserDto>> GetAllAsync();
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto> CreateAsync(UserDto dto);
    Task<UserDto> UpdateAsync(UserDto dto);
    Task<bool> DeleteAsync(int id);
}
