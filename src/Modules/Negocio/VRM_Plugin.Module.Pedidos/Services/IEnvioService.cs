using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Module.Pedidos.Domain;
using VRM_Plugin.Module.Pedidos.Data.DTOs;

namespace VRM_Plugin.Module.Pedidos.Services;

/// <summary>
/// Servicio base para gestionar Envio
/// Cambia la implementación según las necesidades del componente.
/// </summary>
public interface IEnvioService
{
    Task<List<EnvioDto>> GetAllAsync();
    Task<EnvioDto?> GetByIdAsync(int id);
    Task<EnvioDto> CreateAsync(EnvioDto dto);
    Task<EnvioDto> UpdateAsync(EnvioDto dto);
    Task<bool> DeleteAsync(int id);
}
