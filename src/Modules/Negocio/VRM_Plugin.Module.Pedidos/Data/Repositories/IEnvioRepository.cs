using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.Pedidos.Data.DTOs;

namespace VRM_Plugin.Module.Pedidos.Data.Repositories;

/// <summary>
/// Acceso a datos de Envio (contrato)
/// </summary>
public interface IEnvioRepository
{
    /// <summary>
    /// Método de ejemplo que devuelve una lista de DTOs
    /// </summary>
    Task<List<EnvioDto>> GetSampleAsync();
}
