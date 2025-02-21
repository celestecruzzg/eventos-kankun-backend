using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EventosAPI.Data
{
    public static class DatabaseHealthCheck
    {
        public static async Task<(bool IsSuccess, string Message)> VerifyDatabaseConnection(string connectionString)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Verificar si la base de datos existe
                    string databaseName = connection.Database;
                    var command = new SqlCommand(
                        $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'", 
                        connection);
                    int exists = Convert.ToInt32(await command.ExecuteScalarAsync());

                    if (exists == 0)
                    {
                        return (false, $"La base de datos '{databaseName}' no existe.");
                    }

                    // Verificar permisos del usuario
                    command = new SqlCommand(
                        "SELECT HAS_PERMS_BY_NAME(null, null, 'VIEW DATABASE STATE')", 
                        connection);
                    bool hasPermissions = Convert.ToBoolean(await command.ExecuteScalarAsync());

                    if (!hasPermissions)
                    {
                        return (false, "El usuario no tiene los permisos necesarios.");
                    }

                    return (true, "Conexión exitosa a la base de datos.");
                }
            }
            catch (SqlException ex)
            {
                return (false, $"Error de SQL: {ex.Message}\nNúmero de error: {ex.Number}");
            }
            catch (Exception ex)
            {
                return (false, $"Error general: {ex.Message}");
            }
        }
    }
}