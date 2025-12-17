namespace VRM_Plugin.Module.PanelAdmin.Domain;

/// <summary>
/// Entidad ActionRole del módulo PanelAdmin
/// </summary>
public class ActionRoles
{
    public ulong key_id { get; set; }
    public int role_id { get; set; }
    public bool Activo { get; set; } = true;
    public string modificated_user { get; set; } = string.Empty;
}
