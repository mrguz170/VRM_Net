using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using VRM_Plugin.Module.PanelAdmin.Components;
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
    /// <summary>
    /// Método para crear y actualizar roles
    /// </summary>
    Task<bool> CreateNewRole(RolDto rolDto,int opc);
    /// <summary>
    /// Método obtiene todos los roles
    /// </summary>
    Task<List<RolDto>> GetallRoles();
    /// <summary>
    /// Método  obtiene todos los módulos solo id y nombre
    /// </summary>
    Task<List<ModulesDto>> GetallModules();
    /// <summary>
    /// Método obtiene todos los componentes por módulo , solo id y nombre
    /// </summary>
    Task<List<ComponentDto>> GetallComponentsByModule(int moduleid);
    /// <summary>
    /// Método obtiene todas las actions por componente y modulo
    /// </summary>
    Task<List<ActionDto>> GetallActionsByComponent(int moduleid);
    /// <summary>
    /// Método que asigna permisos de actions a roles
    /// </summary>

    Task<bool> SetActionRoles(ActionRoleDto actionroles);

    /// <summary>
    /// Obtiene los IDs de roles que tienen acceso a una acción
    /// </summary>
    Task<List<RolDto>> GetRolesByActionAsync(ulong actionKeyId);

    Task<List<RolDto>> Getall();
}
 