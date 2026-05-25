using System.Data;
using System.Data.Common;
using Npgsql;
using Taller_Mecanico_Services.Domain.Common;
using Taller_Mecanico_Services.Domain.Entities;
using Taller_Mecanico_Services.Domain.Interfaces;
using Taller_Mecanico_Services.Infrastructure.Persistence;

namespace Taller_Mecanico_Services.Infrastructure.Repositories
{
    public class ServicioRepository : IServicioRepository
    {
        private readonly ISqlConnectionFactory _connectionFactory;

        public ServicioRepository(ISqlConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<Result> AddAsync(Servicio servicio)
        {
            const string query = @"
                INSERT INTO servicios (nombre, descripcion, precio_base, duracion_estimada_minutos, categoria_servicio_id, estado, fecha_creacion)
                VALUES (@Nombre, @Descripcion, @PrecioBase, @DuracionEstimada, @CategoriaId, @Estado, @FechaCreacion)
                RETURNING id;";

            try
            {
                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = query;

                AddParameter(command, "@Nombre", servicio.Nombre);
                AddParameter(command, "@Descripcion", servicio.Descripcion ?? (object)DBNull.Value);
                AddParameter(command, "@PrecioBase", servicio.PrecioBase);
                AddParameter(command, "@DuracionEstimada", servicio.DuracionEstimadaMinutos);
                AddParameter(command, "@CategoriaId", servicio.CategoriaServicioId);
                AddParameter(command, "@Estado", servicio.Estado);
                AddParameter(command, "@FechaCreacion", servicio.FechaCreacion);

                var result = await command.ExecuteScalarAsync();
                if (result != null)
                {
                    var assignIdResult = servicio.AsignarIdentificador(Convert.ToInt32(result));
                    if (assignIdResult.IsFailure)
                    {
                        return assignIdResult;
                    }
                }

                return Result.Success();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Result.Failure(ErrorCodes.ServicioNombreDuplicado, "Ya existe un servicio con ese nombre en la categoría especificada.");
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError, $"Error de base de datos: {ex.Message}");
            }
        }

        public async Task<Result> UpdateAsync(Servicio servicio)
        {
            const string query = @"
                UPDATE servicios
                SET nombre = @Nombre,
                    descripcion = @Descripcion,
                    precio_base = @PrecioBase,
                    duracion_estimada_minutos = @DuracionEstimada,
                    categoria_servicio_id = @CategoriaId,
                    estado = @Estado,
                    fecha_modificacion = @FechaModificacion
                WHERE id = @Id;";

            try
            {
                await using var connection = _connectionFactory.CreateConnection();
                await connection.OpenAsync();

                await using var command = connection.CreateCommand();
                command.CommandText = query;

                AddParameter(command, "@Id", servicio.Id);
                AddParameter(command, "@Nombre", servicio.Nombre);
                AddParameter(command, "@Descripcion", servicio.Descripcion ?? (object)DBNull.Value);
                AddParameter(command, "@PrecioBase", servicio.PrecioBase);
                AddParameter(command, "@DuracionEstimada", servicio.DuracionEstimadaMinutos);
                AddParameter(command, "@CategoriaId", servicio.CategoriaServicioId);
                AddParameter(command, "@Estado", servicio.Estado);
                AddParameter(command, "@FechaModificacion", servicio.FechaModificacion ?? (object)DBNull.Value);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    return Result.Failure(ErrorCodes.ServicioNotFound, "El servicio a actualizar no existe.");
                }

                return Result.Success();
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                return Result.Failure(ErrorCodes.ServicioNombreDuplicado, "Ya existe otro servicio con ese nombre en la categoría especificada.");
            }
            catch (Exception ex)
            {
                return Result.Failure(ErrorCodes.DbError, $"Error de base de datos: {ex.Message}");
            }
        }

        public async Task<Servicio?> GetByIdAsync(int id)
        {
            const string query = @"
                SELECT s.id, s.nombre, s.descripcion, s.precio_base, s.duracion_estimada_minutos, s.categoria_servicio_id, s.estado, s.fecha_creacion, s.fecha_modificacion,
                       c.nombre AS categoria_nombre, c.descripcion AS categoria_descripcion, c.estado AS categoria_estado, c.fecha_creacion AS categoria_fecha_creacion, c.fecha_modificacion AS categoria_fecha_modificacion
                FROM servicios s
                INNER JOIN categorias_servicio c ON c.id = s.categoria_servicio_id
                WHERE s.id = @Id LIMIT 1;";

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

        public async Task<bool> ExistsNombreInCategoriaAsync(string nombre, int categoriaId, int? excludeId = null)
        {
            var query = @"
                SELECT EXISTS(
                    SELECT 1 
                    FROM servicios 
                    WHERE LOWER(nombre) = LOWER(@Nombre) 
                      AND categoria_servicio_id = @CategoriaId 
                      AND estado = TRUE";

            if (excludeId.HasValue)
            {
                query += " AND id <> @ExcludeId";
            }
            query += ");";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = query;

            AddParameter(command, "@Nombre", nombre.Trim());
            AddParameter(command, "@CategoriaId", categoriaId);
            if (excludeId.HasValue)
            {
                AddParameter(command, "@ExcludeId", excludeId.Value);
            }

            var result = await command.ExecuteScalarAsync();
            return Convert.ToBoolean(result);
        }

        public async Task<IEnumerable<Servicio>> GetAllAsync(int? categoriaId = null, bool? estado = null, string? nombre = null, string? ordenarPor = null)
        {
            var list = new List<Servicio>();
            var query = @"
                SELECT s.id, s.nombre, s.descripcion, s.precio_base, s.duracion_estimada_minutos, s.categoria_servicio_id, s.estado, s.fecha_creacion, s.fecha_modificacion,
                       c.nombre AS categoria_nombre, c.descripcion AS categoria_descripcion, c.estado AS categoria_estado, c.fecha_creacion AS categoria_fecha_creacion, c.fecha_modificacion AS categoria_fecha_modificacion
                FROM servicios s
                INNER JOIN categorias_servicio c ON c.id = s.categoria_servicio_id
                WHERE 1 = 1";

            if (categoriaId.HasValue)
            {
                query += " AND s.categoria_servicio_id = @CategoriaId";
            }

            if (estado.HasValue)
            {
                query += " AND s.estado = @Estado";
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query += " AND s.nombre ILIKE @Nombre";
            }

            // Ordenamiento dinámico seguro en SQL
            var orderColumn = "s.nombre ASC";
            if (!string.IsNullOrWhiteSpace(ordenarPor))
            {
                var cleanOrder = ordenarPor.Trim().ToLower();
                orderColumn = cleanOrder switch
                {
                    "nombre" => "s.nombre ASC",
                    "nombre_desc" => "s.nombre DESC",
                    "precio" => "s.precio_base ASC",
                    "precio_desc" => "s.precio_base DESC",
                    "duracion" => "s.duracion_estimada_minutos ASC",
                    "duracion_desc" => "s.duracion_estimada_minutos DESC",
                    "id" => "s.id ASC",
                    "id_desc" => "s.id DESC",
                    _ => "s.nombre ASC"
                };
            }
            query += $" ORDER BY {orderColumn};";

            await using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = query;

            if (categoriaId.HasValue)
            {
                AddParameter(command, "@CategoriaId", categoriaId.Value);
            }

            if (estado.HasValue)
            {
                AddParameter(command, "@Estado", estado.Value);
            }

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                AddParameter(command, "@Nombre", $"%{nombre.Trim()}%");
            }

            await using var reader = await (command as DbCommand)!.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapReaderToEntity(reader));
            }

            return list;
        }

        public async Task<Result> DeleteAsync(int id)
        {
            const string query = @"
                UPDATE servicios
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
                    return Result.Failure(ErrorCodes.ServicioNotFound, "El servicio a eliminar no existe.");
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

        private Servicio MapReaderToEntity(DbDataReader reader)
        {
            // Reconstituir Servicio
            var id = reader.GetInt32(reader.GetOrdinal("id"));
            var nombre = reader.GetString(reader.GetOrdinal("nombre"));
            var descripcion = reader.IsDBNull(reader.GetOrdinal("descripcion")) ? null : reader.GetString(reader.GetOrdinal("descripcion"));
            var precioBase = reader.GetDecimal(reader.GetOrdinal("precio_base"));
            var duracion = reader.GetInt32(reader.GetOrdinal("duracion_estimada_minutos"));
            var categoriaId = reader.GetInt32(reader.GetOrdinal("categoria_servicio_id"));
            var estado = reader.GetBoolean(reader.GetOrdinal("estado"));
            var fechaCreacion = reader.GetDateTime(reader.GetOrdinal("fecha_creacion"));
            var fechaModificacion = reader.IsDBNull(reader.GetOrdinal("fecha_modificacion")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("fecha_modificacion"));

            var reconstituted = Servicio.Reconstituir(id, nombre, descripcion, precioBase, duracion, categoriaId, estado, fechaCreacion, fechaModificacion);
            if (reconstituted.IsFailure)
            {
                throw new InvalidOperationException($"Error al reconstituir el servicio: {reconstituted.ErrorMessage}");
            }

            var servicio = reconstituted.Value!;

            // Reconstituir Categoría si está en el reader
            var catId = reader.GetInt32(reader.GetOrdinal("categoria_servicio_id"));
            var catNombre = reader.GetString(reader.GetOrdinal("categoria_nombre"));
            var catDesc = reader.IsDBNull(reader.GetOrdinal("categoria_descripcion")) ? null : reader.GetString(reader.GetOrdinal("categoria_descripcion"));
            var catEstado = reader.GetBoolean(reader.GetOrdinal("categoria_estado"));
            var catCreacion = reader.GetDateTime(reader.GetOrdinal("categoria_fecha_creacion"));
            var catModif = reader.IsDBNull(reader.GetOrdinal("categoria_fecha_modificacion")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("categoria_fecha_modificacion"));

            var reconstitutedCat = CategoriaServicio.Reconstituir(catId, catNombre, catDesc, catEstado, catCreacion, catModif);
            if (reconstitutedCat.IsSuccess)
            {
                servicio.AsignarCategoria(reconstitutedCat.Value!);
            }

            return servicio;
        }
    }
}
