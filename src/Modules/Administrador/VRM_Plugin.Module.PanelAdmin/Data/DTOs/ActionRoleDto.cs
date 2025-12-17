namespace VRM_Plugin.Module.PanelAdmin.Data.DTOs;

/// <summary>
/// DTO básico para ActionRole
/// </summary>
public class ActionRoleDto
{
    public ulong action_key_id { get; set; }
    public int role_id { get; set; }
    public bool Activo { get; set; }

    public string created_user_id { get; set; }= string.Empty;

}
