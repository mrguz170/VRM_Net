using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.PanelAdmin.Data.DTOs;

namespace VRM_Plugin.Module.PanelAdmin.Data.Repositories;

/// <summary>
/// Acceso a datos de Rol (contrato)
/// </summary>
public interface IRolRepository
{
    /// <summary>
    /// Método de ejemplo que devuelve una lista de DTOs
    /// </summary>
    Task<List<RolDto>> GetSampleAsync();

    Task<bool> CreateNewRole(RolDto rolDto);

    Task<List<RolDto>> Getall();
}
