using System.Data;
using System.Reflection;
using MySqlConnector;

namespace VRM_Plugin.Data.Common;  

public class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

    // ==================== MÉTODOS ORIGINALES (Mantener compatibilidad) ====================

    /// <summary>
    /// Ejecuta un SP que devuelve datos (SELECT)
    /// </summary>
    public DataTable ExecuteStoredProcedure(string procedureName, Dictionary<string, object>? parameters = null)
    {
        DataTable result = new DataTable();

        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        using (MySqlCommand cmd = new MySqlCommand(procedureName, conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            // Agregar parámetros dinámicos
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
            {
                adapter.Fill(result);
            }
        }

        return result;
    }

    /// <summary>
    /// Ejecuta un SP que no devuelve datos (INSERT, UPDATE, DELETE)
    /// </summary>
    public int ExecuteNonQuery(string procedureName, Dictionary<string, object>? parameters = null)
    {
        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        using (MySqlCommand cmd = new MySqlCommand(procedureName, conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            conn.Open();
            return cmd.ExecuteNonQuery();
        }
    }

    /// <summary>
    /// Ejecuta un SP que devuelve un único valor (COUNT, MAX, etc.)
    /// </summary>
    public object? ExecuteScalar(string procedureName, Dictionary<string, object>? parameters = null)
    {
        using (MySqlConnection conn = new MySqlConnection(_connectionString))
        using (MySqlCommand cmd = new MySqlCommand(procedureName, conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
                }
            }

            conn.Open();
            return cmd.ExecuteScalar();
        }
    }

    // ==================== NUEVOS MÉTODOS GENÉRICOS ====================

    /// <summary>
    /// ✅ Ejecuta un SP y parsea automáticamente el resultado a una lista de objetos
    /// </summary>
    public List<T> ExecuteStoredProcedure<T>(string procedureName, Dictionary<string, object>? parameters = null) where T : new()
    {
        var dataTable = ExecuteStoredProcedure(procedureName, parameters);
        return DataTableToList<T>(dataTable);
    }

    /// <summary>
    /// ✅ Ejecuta un SP y parsea automáticamente el resultado a un solo objeto (primera fila)
    /// </summary>
    public T? ExecuteStoredProcedureSingle<T>(string procedureName, Dictionary<string, object>? parameters = null) where T : new()
    {
        var dataTable = ExecuteStoredProcedure(procedureName, parameters);
        return DataTableToList<T>(dataTable).FirstOrDefault();
    }

    // ==================== MÉTODOS DE MAPEO GENÉRICO ====================

    /// <summary>
    /// Convierte un DataTable a una lista de objetos del tipo especificado
    /// Usa reflexión para mapear columnas a propiedades automáticamente
    /// Soporta: PascalCase, snake_case, camelCase
    /// </summary>
    private List<T> DataTableToList<T>(DataTable dataTable) where T : new()
    {
        var list = new List<T>();
        
        if (dataTable.Rows.Count == 0)
            return list;

        // Obtener todas las propiedades públicas que se pueden escribir
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToList();

        foreach (DataRow row in dataTable.Rows)
        {
            var obj = new T();

            foreach (var property in properties)
            {
                // Buscar columna que coincida con el nombre de la propiedad
                var columnName = FindMatchingColumn(dataTable, property.Name);
                
                if (columnName == null || row[columnName] == DBNull.Value)
                    continue;

                try
                {
                    var value = row[columnName];
                    var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

                    // Conversión segura según el tipo de la propiedad
                    object convertedValue = propertyType.Name switch
                    {
                        nameof(String) => value.ToString() ?? string.Empty,
                        nameof(Int32) => Convert.ToInt32(value),
                        nameof(Int64) => Convert.ToInt64(value),
                        nameof(Decimal) => Convert.ToDecimal(value),
                        nameof(Double) => Convert.ToDouble(value),
                        nameof(Boolean) => Convert.ToBoolean(value),
                        nameof(DateTime) => Convert.ToDateTime(value),
                        nameof(Guid) => Guid.Parse(value.ToString() ?? string.Empty),
                        _ when propertyType.IsEnum => Enum.Parse(propertyType, value.ToString() ?? "0"),
                        _ => value
                    };

                    property.SetValue(obj, convertedValue);
                }
                catch (Exception ex)
                {
                    // Log error pero no falla todo el mapeo
                    Console.WriteLine($"⚠️ Error mapeando propiedad {property.Name}: {ex.Message}");
                }
            }

            list.Add(obj);
        }

        return list;
    }

    /// <summary>
    /// Encuentra el nombre de columna que coincida con el nombre de propiedad
    /// Soporta: PascalCase, snake_case, camelCase (insensible a mayúsculas)
    /// </summary>
    private string? FindMatchingColumn(DataTable dataTable, string propertyName)
    {
        // 1. Búsqueda exacta
        if (dataTable.Columns.Contains(propertyName))
            return propertyName;

        // 2. Búsqueda insensible a mayúsculas/minúsculas
        var column = dataTable.Columns.Cast<DataColumn>()
            .FirstOrDefault(c => c.ColumnName.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
        
        if (column != null)
            return column.ColumnName;

        // 3. Convertir PascalCase a snake_case (ej: "ComponentId" → "component_id")
        var snakeCase = ToSnakeCase(propertyName);
        if (dataTable.Columns.Contains(snakeCase))
            return snakeCase;

        // 4. Buscar snake_case insensible a mayúsculas
        column = dataTable.Columns.Cast<DataColumn>()
            .FirstOrDefault(c => c.ColumnName.Equals(snakeCase, StringComparison.OrdinalIgnoreCase));

        return column?.ColumnName;
    }

    /// <summary>
    /// Convierte PascalCase a snake_case
    /// Ejemplo: "ComponentId" → "component_id"
    /// </summary>
    private string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return string.Concat(input.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x : x.ToString()))
            .ToLower();
    }
}
