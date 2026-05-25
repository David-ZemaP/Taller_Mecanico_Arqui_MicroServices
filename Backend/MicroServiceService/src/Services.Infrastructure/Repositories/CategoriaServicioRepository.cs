using System.Data;
using System.Data.Common;
using Npgsql;
using Taller_Mecanico_Services.Domain.Common;
using Taller_Mecanico_Services.Domain.Entities;
using Taller_Mecanico_Services.Domain.Interfaces;
using Taller_Mecanico_Services.Infrastructure.Persistence;

namespace Taller_Mecanico_Services.Infrastructure.Repositories
{
    public class CategoriaServicioRepository : ICategoriaServicioRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public CategoriaServicioRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Result> AddAsync(CategoriaServicio categoria)
        {
            const string query = @"
                INSERT INTO categorias_servicio (nombre, descripcion, estado, fecha_creacion)
                VALUES (@Nombre, @Descripcion, @Estado, @FechaCreacion)
                RETURNING id;";

            try
            {
                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = query;

                AddParameter(command, "@Nombre", categoria.Nombre);
                AddParameter(command, "@Descripcion", categoria.Descripcion ?? (object)DBNull.Value);
                AddParameter(command, "@Estado", categoria.Estado);
                AddParameter(command, "@FechaCreacion", categoria.FechaCreacion);

                var result = await command.ExecuteScalarAsync();
                if (result != null)
                {
                    var assignIdResult = categoria.AsignarIdentificador(Convert.ToInt32(result));
                    if (assignIdResult.IsFailure)
                    {
                        return assignIdResult;
                    }
                }

                return Result.Success();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Result.Failure(ErrorCodes.CategoriaNombreDuplicado, "Ya existe una categoría con ese nombre.");
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError, $"Error de base de datos: {ex.Message}");
            }
        }

        public async Task<Result> UpdateAsync(CategoriaServicio categoria)
        {
            const string query = @"
                UPDATE categorias_servicio
                SET nombre = @Nombre,
                    descripcion = @Descripcion,
                    estado = @Estado,
                    fecha_modificacion = @FechaModificacion
                WHERE id = @Id;";

            try
            {
                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = query;

                AddParameter(command, "@Id", categoria.Id);
                AddParameter(command, "@Nombre", categoria.Nombre);
                AddParameter(command, "@Descripcion", categoria.Descripcion ?? (object)DBNull.Value);
                AddParameter(command, "@Estado", categoria.Estado);
                AddParameter(command, "@FechaModificacion", categoria.FechaModificacion ?? (object)DBNull.Value);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    return Result.Failure(ErrorCodes.CategoriaNotFound, "La categoría a actualizar no fue encontrada.");
                }

                return Result.Success();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Result.Failure(ErrorCodes.CategoriaNombreDuplicado, "Ya existe una categoría con ese nombre.");
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError, $"Error de base de datos: {ex.Message}");
            }
        }

        public async Task<CategoriaServicio?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT id, nombre, descripcion, estado, fecha_creacion, fecha_modificacion
                FROM categorias_servicio
                WHERE id = @Id LIMIT 1;";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = query;
            AddParameter(command, "@Id", id);

            await using var reader = await (command as DbCommand)!.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToEntity(reader);
            }

            return null;
        }

        public async Task<CategoriaServicio?> GetByNombreAsync(string nombre)
        {
            const string query = @"
                SELECT id, nombre, descripcion, estado, fecha_creacion, fecha_modificacion
                FROM categorias_servicio
                WHERE LOWER(nombre) = LOWER(@Nombre) LIMIT 1;";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = query;
            AddParameter(command, "@Nombre", nombre.Trim());

            await using var reader = await (command as DbCommand)!.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapReaderToEntity(reader);
            }

            return null;
        }

        public async Task<IEnumerable<CategoriaServicio>> GetAllAsync(bool? estado = null, string? ordenarPor = null)
        {
            var list = new List<CategoriaServicio>();
            var query = @"
                SELECT id, nombre, descripcion, estado, fecha_creacion, fecha_modificacion
                FROM categorias_servicio
                WHERE 1 = 1";

            if (estado.HasValue)
            {
                query += " AND estado = @Estado";
            }

            // Aplicar ordenamiento
            var orderColumn = "nombre ASC";
            if (!string.IsNullOrWhiteSpace(ordenarPor))
            {
                var cleanOrder = ordenarPor.Trim().ToLower();
                orderColumn = cleanOrder switch
                {
                    "nombre" => "nombre ASC",
                    "nombre_desc" => "nombre DESC",
                    "id" => "id ASC",
                    "id_desc" => "id DESC",
                    "fecha" => "fecha_creacion ASC",
                    "fecha_desc" => "fecha_creacion DESC",
                    _ => "nombre ASC"
                };
            }
            query += $" ORDER BY {orderColumn};";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = query;

            if (estado.HasValue)
            {
                AddParameter(command, "@Estado", estado.Value);
            }

            await using var reader = await (command as DbCommand)!.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapReaderToEntity(reader));
            }

            return list;
        }

        public async Task<bool> HasActiveServicesAsync(int categoriaId)
        {
            const string query = "SELECT EXISTS(SELECT 1 FROM servicios WHERE categoria_servicio_id = @CategoriaId AND estado = TRUE);";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = query;
            AddParameter(command, "@CategoriaId", categoriaId);

            var result = await command.ExecuteScalarAsync();
            return Convert.ToBoolean(result);
        }

        public async Task<Result> DeleteAsync(int id)
        {
            const string query = @"
                UPDATE categorias_servicio
                SET estado = FALSE,
                    fecha_modificacion = @FechaModificacion
                WHERE id = @Id;";

            try
            {
                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = query;

                AddParameter(command, "@Id", id);
                AddParameter(command, "@FechaModificacion", DateTime.UtcNow);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    return Result.Failure(ErrorCodes.CategoriaNotFound, "La categoría a eliminar no existe.");
                }

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError, $"Error de base de datos: {ex.Message}");
            }
        }

        private void AddParameter(IDbCommand command, string name, object value)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = name;
            parameter.Value = value;
            command.Parameters.Add(parameter);
        }

        private CategoriaServicio MapReaderToEntity(DbDataReader reader)
        {
            var id = reader.GetInt32(reader.GetOrdinal("id"));
            var nombre = reader.GetString(reader.GetOrdinal("nombre"));
            var descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? null : reader.GetString(reader.GetOrdinal("descripcion"));
            var estado = reader.GetBoolean(reader.GetOrdinal("estado"));
            var fechaCreacion = reader.GetDateTime(reader.GetOrdinal("fecha_creacion"));
            var fechaModificacion = reader.IsDBNull(reader.GetOrdinal("fecha_modificacion")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("fecha_modificacion"));

            var reconstituted = CategoriaServicio.Reconstituir(id, nombre, descripcion, estado, fechaCreacion, fechaModificacion);
            if (reconstituted.IsFailure)
            {
                throw new InvalidOperationException($"Error al reconstituir la categoría: {reconstituted.ErrorMessage}");
            }

            return reconstituted.Value!;
        }
    }
}
