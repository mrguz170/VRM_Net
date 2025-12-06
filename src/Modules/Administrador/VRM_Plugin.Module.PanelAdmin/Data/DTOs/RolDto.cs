namespace VRM_Plugin.Module.PanelAdmin.Data.DTOs;

/// <summary>
/// DTO básico para Rol
/// </summary>
public class RolDto
{
    public int role_id { get; set; }
    public string role_name { get; set; } = string.Empty;
    public string description { get; set; } = string.Empty;
    public bool is_active { get; set; } = true;
    public DateTime created_date { get; set; }
    public DateTime updated_date { get; set; }

    public string user_name { get; set; } = string.Empty;
    public string create_user_id { get; set; }

}
