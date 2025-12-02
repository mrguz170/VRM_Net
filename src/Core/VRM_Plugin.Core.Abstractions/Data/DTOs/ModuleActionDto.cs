namespace VRM_Plugin.Core.Abstractions.Data.DTOs;

/// <summary>
/// Entidad que representa una acción granular (operación/business logic).
/// Mapea directamente a tabla AccionesGranulares en BD.
/// Cada acción tiene una relación explícita con su componente mediante IdComponent. 
/// ? Ubicación: Data/DTOs/ 
/// </summary>
public class ModuleActionDto
{
    // ===== IDs PARA BD =====

    /// <summary>
    /// ID de la acción en BD (auto-generado por IDENTITY)
    /// </summary>
    public long ActionKeyId { get; set; }

    /// <summary>
    /// ID del componente al que pertenece esta acción (FK)
    /// </summary>
    public int? ComponentId { get; set; }

    // ===== METADATA DE LA ACCIÓN =====

    /// <summary>
    /// Formato: "{ModuleName}.{ComponentName}.{ActionName}"
    /// Ej: "Finanzas.Facturas.TimbrarSAT"
    /// </summary>
    public string ActionKey { get; set; } = string.Empty;

    /// <summary>
    /// Nombre descriptivo de la acción (para UI)
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Descripción detallada de la acción
    /// </summary>
    public string Description { get; set; } = string.Empty;
        
    // ===== PERMISOS DE EJECUCIÓN =====

    /// <summary>
    /// ? Propiedad temporal para recibir "roles" del SP como string "1,2,3"
    /// El parseador genérico mapeará "roles" (BD) ? "RolesString" (propiedad)
    /// Luego se parsea a RequiredPermissionIds en post-procesamiento
    /// </summary>
    public string? RolesString { get; set; }
    
    /// <summary>
    /// ? Alias para compatibilidad con parseador genérico
    /// Si el SP devuelve "roles", se mapeará aquí automáticamente
    /// </summary>
    public string? Roles 
    { 
        get => RolesString;
        set => RolesString = value;
    }

    /// <summary>
    /// IDs de permisos requeridos para EJECUTAR esta acción
    /// Se llena después del parseo desde RolesString
    /// </summary>
    public List<int> RequiredPermissionIds { get; set; } = new();

    // ===== ESTADO (SOFT DELETE) =====

    /// <summary>
    /// Indica si la acción está activa
    /// </summary>
    public bool IsActive { get; set; } = true;
}
