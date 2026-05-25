using Taller_Mecanico_Services.Domain.Common;

namespace Taller_Mecanico_Services.Domain.Entities
{
    public class Servicio
    {
        public int Id { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public string? Descripcion { get; private set; }
        public decimal PrecioBase { get; private set; }
        public int DuracionEstimadaMinutos { get; private set; }
        public int CategoriaServicioId { get; private set; }
        public CategoriaServicio? Categoria { get; private set; }
        public bool Estado { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime? FechaModificacion { get; private set; }

        private Servicio() { }

        public static Result<Servicio> Crear(string nombre, string? descripcion, decimal precioBase, int duracionEstimadaMinutos, int categoriaServicioId)
        {
            var valNombre = ValidarNombre(nombre);
            if (valNombre.IsFailure) return Result<Servicio>.Failure(valNombre.ErrorCode!, valNombre.ErrorMessage!);

            var valPrecio = ValidarPrecio(precioBase);
            if (valPrecio.IsFailure) return Result<Servicio>.Failure(valPrecio.ErrorCode!, valPrecio.ErrorMessage!);

            var valDuracion = ValidarDuracion(duracionEstimadaMinutos);
            if (valDuracion.IsFailure) return Result<Servicio>.Failure(valDuracion.ErrorCode!, valDuracion.ErrorMessage!);

            if (categoriaServicioId <= 0)
            {
                return Result<Servicio>.Failure(ErrorCodes.ValidationInvalidValue, "La categoría de servicio seleccionada no es válida.");
            }

            return Result<Servicio>.Success(new Servicio
            {
                Id = 0,
                Nombre = valNombre.Value!,
                Descripcion = descripcion?.Trim(),
                PrecioBase = precioBase,
                DuracionEstimadaMinutos = duracionEstimadaMinutos,
                CategoriaServicioId = categoriaServicioId,
                Estado = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        public static Result<Servicio> Reconstituir(int id, string nombre, string? descripcion, decimal precioBase, int duracionEstimadaMinutos, int categoriaServicioId, bool estado, DateTime fechaCreacion, DateTime? fechaModificacion)
        {
            if (id <= 0)
            {
                return Result<Servicio>.Failure(ErrorCodes.ValidationInvalidValue, "El identificador del servicio no es válido.");
            }

            var valNombre = ValidarNombre(nombre);
            if (valNombre.IsFailure) return Result<Servicio>.Failure(valNombre.ErrorCode!, valNombre.ErrorMessage!);

            var valPrecio = ValidarPrecio(precioBase);
            if (valPrecio.IsFailure) return Result<Servicio>.Failure(valPrecio.ErrorCode!, valPrecio.ErrorMessage!);

            var valDuracion = ValidarDuracion(duracionEstimadaMinutos);
            if (valDuracion.IsFailure) return Result<Servicio>.Failure(valDuracion.ErrorCode!, valDuracion.ErrorMessage!);

            if (categoriaServicioId <= 0)
            {
                return Result<Servicio>.Failure(ErrorCodes.ValidationInvalidValue, "La categoría de servicio seleccionada no es válida.");
            }

            return Result<Servicio>.Success(new Servicio
            {
                Id = id,
                Nombre = valNombre.Value!,
                Descripcion = descripcion?.Trim(),
                PrecioBase = precioBase,
                DuracionEstimadaMinutos = duracionEstimadaMinutos,
                CategoriaServicioId = categoriaServicioId,
                Estado = estado,
                FechaCreacion = fechaCreacion,
                FechaModificacion = fechaModificacion
            });
        }

        public Result Modificar(string nombre, string? descripcion, decimal precioBase, int duracionEstimadaMinutos, int categoriaServicioId)
        {
            var valNombre = ValidarNombre(nombre);
            if (valNombre.IsFailure) return Result.Failure(valNombre.ErrorCode!, valNombre.ErrorMessage!);

            var valPrecio = ValidarPrecio(precioBase);
            if (valPrecio.IsFailure) return Result.Failure(valPrecio.ErrorCode!, valPrecio.ErrorMessage!);

            var valDuracion = ValidarDuracion(duracionEstimadaMinutos);
            if (valDuracion.IsFailure) return Result.Failure(valDuracion.ErrorCode!, valDuracion.ErrorMessage!);

            if (categoriaServicioId <= 0)
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "La categoría de servicio seleccionada no es válida.");
            }

            Nombre = valNombre.Value!;
            Descripcion = descripcion?.Trim();
            PrecioBase = precioBase;
            DuracionEstimadaMinutos = duracionEstimadaMinutos;
            CategoriaServicioId = categoriaServicioId;
            FechaModificacion = DateTime.UtcNow;

            return Result.Success();
        }

        public void Desactivar()
        {
            Estado = false;
            FechaModificacion = DateTime.UtcNow;
        }

        public void Activar()
        {
            Estado = true;
            FechaModificacion = DateTime.UtcNow;
        }

        public Result AsignarIdentificador(int id)
        {
            if (id <= 0)
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "El identificador del servicio no es válido.");
            }

            if (Id > 0 && Id != id)
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "El identificador del servicio ya fue asignado.");
            }

            Id = id;
            return Result.Success();
        }

        public void AsignarCategoria(CategoriaServicio categoria)
        {
            CategoriaServicioId = categoria.Id;
            Categoria = categoria;
        }

        private static Result<string> ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return Result<string>.Failure(ErrorCodes.ValidationRequired, "El nombre del servicio es obligatorio.");
            }

            var trimmed = nombre.Trim();
            if (trimmed.Length > 120)
            {
                return Result<string>.Failure(ErrorCodes.ValidationRequired, "El nombre no puede tener más de 120 caracteres.");
            }

            return Result<string>.Success(trimmed);
        }

        private static Result ValidarPrecio(decimal precio)
        {
            if (precio <= 0)
            {
                return Result.Failure(ErrorCodes.ServicioPrecioInvalido, "El precio base debe ser mayor a cero.");
            }

            return Result.Success();
        }

        private static Result ValidarDuracion(int duracion)
        {
            if (duracion <= 0)
            {
                return Result.Failure(ErrorCodes.ServicioDuracionInvalida, "La duración estimada debe ser mayor a cero minutos.");
            }

            return Result.Success();
        }
    }
}
