using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.Viaticos.Data.DTOs;

namespace VRM_Plugin.Module.Viaticos.Data.Repositories;

/// <summary>
/// Acceso a datos de Anticipo (contrato)
/// </summary>
public interface IAnticipoRepository
{
    /// <summary>
    /// Método de ejemplo que devuelve una lista de DTOs
    /// </summary>
    Task<List<AnticipoDto>> GetSampleAsync();
}
