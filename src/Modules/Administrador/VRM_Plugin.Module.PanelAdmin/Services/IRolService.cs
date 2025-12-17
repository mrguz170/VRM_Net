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
    Task<Rol> GetByIdAsync(int id);
    Task<bool> CreateAsync(RolDto dto, int opc);
    Task<List<Modules>> GetModules();

    Task<List<Component>> GetComponentById(int idMod);

    Task<List<Actions>> GetAllActions(int modid);

    Task<bool> SetActionRoles(ActionRoles action);

    Task<List<Rol>> GetActionByRole(ulong actionid);

    Task<bool> UpdateComponentPermissionsAsync(Dictionary<int, List<int>> componentRoles, string userId);
}
