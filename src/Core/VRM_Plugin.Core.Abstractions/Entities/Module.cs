namespace VRM_Plugin.Core.Abstractions.Entities;

/// <summary>
/// Entidad que representa un módulo (DLL) del sistema.
/// Mapea directamente a tabla Modulos en BD.
/// Esta es la entidad persistente que los módulos (IModule) referencian.
/// </summary>
public class Module
{
    // ===== IDs =====
    
    /// <summary>
    /// ID del módulo en BD (PK, auto-generado por IDENTITY)
    /// </summary>
    public int IdModule { get; set; }
    
    // ===== IDENTIFICACIÓN =====
    
    /// <summary>
    /// Nombre técnico/código del módulo (UNIQUE en BD)
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
    
    // ===== AUDITORÍA =====
    
    /// <summary>
    /// Fecha y hora de creación del registro (UTC)
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Fecha y hora de última actualización (UTC)
    /// NULL si nunca se ha actualizado
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// ID del usuario que creó el registro
    /// NULL si fue creado por el sistema
    /// </summary>
    public int? CreatedBy { get; set; }
    
    /// <summary>
    /// ID del usuario que realizó la última actualización
    /// NULL si nunca se ha actualizado
    /// </summary>
    public int? UpdatedBy { get; set; }
    
    // ===== NAVEGACIÓN (Entity Framework) =====
    
    /// <summary>
    /// Componentes que pertenecen a este módulo
    /// </summary>
    public List<ModuleComponent> Components { get; set; } = new();
}
