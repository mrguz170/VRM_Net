namespace VRM_Plugin.Blazor.Server.Services;

/// <summary>
/// Servicio para obtener nombres de visualización de roles.
/// Centraliza el mapeo de IDs de roles a nombres amigables.
/// </summary>
public interface IRoleDisplayNameService
{
    /// <summary>
    /// Obtiene el nombre de visualización de un rol.
    /// </summary>
    /// <param name="roleId">ID del rol (ej: "Admin", "GerenteFinanzas")</param>
    /// <returns>Nombre amigable para mostrar en UI</returns>
    string GetDisplayName(string roleId);
    
    /// <summary>
    /// Obtiene todos los roles con sus nombres de visualización.
    /// </summary>
    IReadOnlyDictionary<string, string> GetAllRoles();
}

/// <summary>
/// Implementación del servicio de nombres de roles.
/// TODO: En producción, obtener de base de datos en lugar de diccionario hardcoded.
/// </summary>
public class RoleDisplayNameService : IRoleDisplayNameService
{
    // TODO: Reemplazar con consulta a BD (tabla Roles)
    private readonly IReadOnlyDictionary<string, string> _roleDisplayNames = new Dictionary<string, string>
    {
        // Roles de sistema
        ["Admin"] = "Administrator",
        ["User"] = "User",
        ["Guest"] = "Guest",
        
        // Roles de Finanzas (ID Permisos: 1-4)
        ["GerenteFinanzas"] = "Finance Manager",
        ["Gerente.Finanzas"] = "Finance Manager",
        ["CoordinadorFinanzas"] = "Finance Coordinator",
        ["Coordinador.Finanzas"] = "Finance Coordinator",
        ["Contador"] = "Accountant",
        
        // Roles de Prospectos (ID Permisos: 5-9)
        ["GestorProspectos"] = "Prospects Manager",
        ["Gestor.Prospectos"] = "Prospects Manager",
        ["CoordinadorProspectos"] = "Prospects Coordinator",
        ["Coordinador.Prospectos"] = "Prospects Coordinator",
        ["RevisorLegal"] = "Legal Reviewer",
        ["Revisor.Legal"] = "Legal Reviewer",
        ["RevisorFinanzas"] = "Financial Reviewer",
        ["Revisor.Finanzas"] = "Financial Reviewer",
        ["RevisorTecnico"] = "Technical Reviewer",
        ["Revisor.Tecnico"] = "Technical Reviewer",
    };

    public string GetDisplayName(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId))
            return "Guest";
        
        // Intentar obtener nombre de visualización
        if (_roleDisplayNames.TryGetValue(roleId, out var displayName))
            return displayName;
        
        // Fallback: Formatear el ID del rol
        // "GerenteFinanzas" -> "Gerente Finanzas"
        return FormatRoleId(roleId);
    }

    public IReadOnlyDictionary<string, string> GetAllRoles()
    {
        return _roleDisplayNames;
    }

    /// <summary>
    /// Formatea un ID de rol para hacerlo más legible.
    /// Ej: "GerenteFinanzas" -> "Gerente Finanzas"
    /// </summary>
    private string FormatRoleId(string roleId)
    {
        // Reemplazar puntos con espacios
        roleId = roleId.Replace(".", " ");
        
        // Insertar espacios antes de mayúsculas
        var result = System.Text.RegularExpressions.Regex.Replace(
            roleId, 
            "([a-z])([A-Z])", 
            "$1 $2"
        );
        
        return result;
    }
}
