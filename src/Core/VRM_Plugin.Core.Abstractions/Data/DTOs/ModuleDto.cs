namespace VRM_Plugin.Core.Abstractions.Data.DTOs;

/// <summary>
/// Entidad que representa un módulo (DLL) del sistema.
/// Mapea directamente a tabla Modulos en BD.
/// ? Ubicación: Data/DTOs/ 
/// </summary>
public class ModuleDto
{
    // ===== IDs =====
    
    /// <summary>
    /// ID del módulo en BD 
    /// </summary>
    public int ModuleId { get; set; }
    
    // ===== IDENTIFICACIÓN =====
    
    /// <summary>
    /// Nombre técnico/código del módulo 
    /// Ej: "Finanzas", "Prospectos", "Inventario"
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;
    
    /// <summary>
    /// Nombre para mostrar en UI
    /// Ej: "Gestión de Finanzas"
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Descripción del módulo
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Versión del módulo (ej: "1.0.0")
    /// </summary>
    public string Version { get; set; } = string.Empty;
    
    // ===== ASSEMBLY INFO =====
    
    /// <summary>
    /// Nombre del assembly (DLL)
    /// Ej: "VRM_Plugin.Modules.Finanzas.dll"
    /// </summary>
    public string AssemblyName { get; set; } = string.Empty;
    
    /// <summary>
    /// Ruta física del DLL en el servidor
    /// </summary>
    public string AssemblyPath { get; set; } = string.Empty;
    
    // ===== ESTADO =====
    
    /// <summary>
    /// Indica si el módulo está activo (soft delete)
    /// </summary>
    public bool IsActive { get; set; } = true;
       
    // ===== NAVEGACIÓN (Entity Framework) =====
    
    /// <summary>
    /// Componentes que pertenecen a este módulo
    /// </summary>
    public List<ModuleComponentDto> Components { get; set; } = new();
}
