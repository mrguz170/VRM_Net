using System.Data;
using MySqlConnector;

namespace VRM_Plugin.Data;

public class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper(string connectionString)
    {
        _connectionString = connectionString;
    }

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
}
