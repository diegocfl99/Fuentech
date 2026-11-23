
using MySqlConnector;

namespace Fuentech.Services
{
    public class SolicitudContacto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string? Correo { get; set; }
        public string? Telefono { get; set; }
        public string Mensaje { get; set; } = "";
        public DateTime Fecha { get; set; }
    }

    public class SolicitudService
    {
        private readonly string _connectionString;

        public SolicitudService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("FuentechDB")
                ?? throw new InvalidOperationException(
                    "No se encontró la conexión FuentechDB.");
        }

        // GUARDAR SOLICITUD
        public async Task<int> GuardarSolicitudAsync(
            string nombre,
            string? correo,
            string? telefono,
            string mensaje)
        {
            const string query = @"
                INSERT INTO solicitudes_contacto
                (nombre, correo, telefono, mensaje)
                VALUES
                (@nombre, @correo, @telefono, @mensaje);
            ";

            await using var connection =
                new MySqlConnection(_connectionString);

            await connection.OpenAsync();

            await using var command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@nombre", nombre);
            command.Parameters.AddWithValue(
                "@correo", (object?)correo ?? DBNull.Value);
            command.Parameters.AddWithValue(
                "@telefono", (object?)telefono ?? DBNull.Value);
            command.Parameters.AddWithValue("@mensaje", mensaje);

            return await command.ExecuteNonQueryAsync();
        }

        // CONSULTAR SOLICITUDES
        public async Task<List<SolicitudContacto>> ObtenerSolicitudesAsync()
        {
            const string query = @"
                SELECT id, nombre, correo, telefono, mensaje, fecha
                FROM solicitudes_contacto
                ORDER BY fecha DESC;
            ";

            var solicitudes = new List<SolicitudContacto>();

            await using var connection =
                new MySqlConnection(_connectionString);

            await connection.OpenAsync();

            await using var command =
                new MySqlCommand(query, connection);

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                solicitudes.Add(new SolicitudContacto
                {
                    Id = reader.GetInt32("id"),
                    Nombre = reader.GetString("nombre"),
                    Correo = reader.IsDBNull(reader.GetOrdinal("correo"))
                        ? null : reader.GetString("correo"),
                    Telefono = reader.IsDBNull(reader.GetOrdinal("telefono"))
                        ? null : reader.GetString("telefono"),
                    Mensaje = reader.GetString("mensaje"),
                    Fecha = reader.GetDateTime("fecha")
                });
            }

            return solicitudes;
        }

        // ELIMINAR SOLICITUD
        public async Task<bool> EliminarSolicitudAsync(int id)
        {
            const string query = @"
                DELETE FROM solicitudes_contacto
                WHERE id = @id;
            ";

            await using var connection =
                new MySqlConnection(_connectionString);

            await connection.OpenAsync();

            await using var command =
                new MySqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            return await command.ExecuteNonQueryAsync() > 0;
        }
    }
}
