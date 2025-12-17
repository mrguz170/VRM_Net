using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.PanelAdmin.Data.DTOs;

namespace VRM_Plugin.Module.PanelAdmin.Data.Repositories;

/// <summary>
/// Acceso a datos de Rol (contrato)
/// </summary>
public interface IRolRepository
{   

    Task<bool> CreateNewRole(RolDto rolDto);

    Task<List<RolDto>> Getall();

    /// <summary>
    /// Actualiza los roles asignados a un componente
    /// </summary>
    Task<bool> UpdateComponentRolesAsync(int componentId, List<int> roleIds, string userId);
}
