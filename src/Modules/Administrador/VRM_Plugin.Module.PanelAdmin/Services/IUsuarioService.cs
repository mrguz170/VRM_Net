using System.Collections.Generic;
using System.Threading.Tasks;
using VRM_Plugin.Core.Abstractions.Data.DTOs;
using VRM_Plugin.Module.PanelAdmin.Domain;

namespace VRM_Plugin.Module.PanelAdmin.Services;

/// <summary>
/// Servicio base para gestionar Usuario
/// Cambia la implementación según las necesidades del componente.
/// </summary>
public interface IUsuarioService
{
    Task<List<Usuario>> GetAllUserAsync();
    Task<Usuario?> GetUserByIdAsync(string id);
    Task<Usuario> CreateUserAsync(Usuario usuario); 
    Task<Usuario> UpdateUserAsync(Usuario usuario);
    Task<bool> DeleteUserAsync(string id);

}
