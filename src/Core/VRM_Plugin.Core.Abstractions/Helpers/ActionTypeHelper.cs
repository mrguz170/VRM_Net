using VRM_Plugin.Core.Abstractions.Entities;

namespace VRM_Plugin.Core.Abstractions.Helpers;

/// <summary>
/// Constantes para IDs de tipos de acción (sincronizadas con BD)
/// </summary>
public static class ActionTypeIds
{
    /// <summary>
    /// Operaciones de lectura/consulta (SELECT)
    /// Severidad: Baja
    /// </summary>
    public const int Lectura = 1;

    /// <summary>
    /// Operaciones de escritura/modificación (INSERT, UPDATE)
    /// Severidad: Media
    /// </summary>
    public const int Escritura = 2;

    /// <summary>
    /// Operaciones críticas (DELETE, aprobaciones, cambios sensibles)
    /// Severidad: Alta
    /// </summary>
    public const int Critica = 3;
}

/// <summary>
/// Helper para trabajar con tipos de acción
/// En producción, los datos vendrían de BD
/// </summary>
public static class ActionTypeHelper
{
    /// <summary>
    /// Obtiene el catálogo de tipos de acción (simulado)
    /// En producción, esto vendría de BD
    /// </summary>
    public static List<ActionType> GetActionTypes()
    {
        return new List<ActionType>
        {
            new ActionType
            {
                IdActionType = ActionTypeIds.Lectura,
                Code = "Lectura",
                Name = "Operación de Lectura",
                Description = "Operaciones de consulta y visualización (SELECT)",
                SeverityLevel = 1,
                RequiresAudit = false,
                ColorUI = "primary",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ActionType
            {
                IdActionType = ActionTypeIds.Escritura,
                Code = "Escritura",
                Name = "Operación de Escritura",
                Description = "Operaciones de creación y modificación (INSERT, UPDATE)",
                SeverityLevel = 2,
                RequiresAudit = true,
                ColorUI = "warning",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new ActionType
            {
                IdActionType = ActionTypeIds.Critica,
                Code = "Critica",
                Name = "Operación Crítica",
                Description = "Operaciones sensibles (DELETE, aprobaciones, cambios críticos)",
                SeverityLevel = 3,
                RequiresAudit = true,
                ColorUI = "danger",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
    }

    /// <summary>
    /// Obtiene solo los tipos de acción activos
    /// </summary>
    public static List<ActionType> GetActiveActionTypes()
    {
        return GetActionTypes().Where(at => at.IsActive).ToList();
    }

    /// <summary>
    /// Obtiene el nombre del tipo de acción por ID
    /// </summary>
    public static string GetActionTypeName(int idActionType)
    {
        return idActionType switch
        {
            ActionTypeIds.Lectura => "Lectura",
            ActionTypeIds.Escritura => "Escritura",
            ActionTypeIds.Critica => "Crítica",
            _ => "Desconocido"
        };
    }

    /// <summary>
    /// Obtiene el color UI del tipo de acción por ID
    /// </summary>
    public static string GetActionTypeColor(int idActionType)
    {
        return idActionType switch
        {
            ActionTypeIds.Lectura => "primary",
            ActionTypeIds.Escritura => "warning",
            ActionTypeIds.Critica => "danger",
            _ => "secondary"
        };
    }

    /// <summary>
    /// Verifica si un tipo de acción requiere auditoría
    /// </summary>
    public static bool RequiresAudit(int idActionType)
    {
        return idActionType switch
        {
            ActionTypeIds.Lectura => false,
            ActionTypeIds.Escritura => true,
            ActionTypeIds.Critica => true,
            _ => true  // Por defecto, auditar
        };
    }

    /// <summary>
    /// Obtiene el nivel de severidad del tipo de acción
    /// </summary>
    public static int GetSeverityLevel(int idActionType)
    {
        return idActionType switch
        {
            ActionTypeIds.Lectura => 1,
            ActionTypeIds.Escritura => 2,
            ActionTypeIds.Critica => 3,
            _ => 0
        };
    }
}
