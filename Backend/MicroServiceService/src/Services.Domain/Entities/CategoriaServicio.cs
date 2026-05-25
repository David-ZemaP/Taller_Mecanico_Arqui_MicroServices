using Taller_Mecanico_Services.Domain.Common;

namespace Taller_Mecanico_Services.Domain.Entities
{
    public class CategoriaServicio
    {
        public int Id { get; private set; }
        public string Nombre { get; private set; } = string.Empty;
        public string? Descripcion { get; private set; }
        public bool Estado { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime? FechaModificacion { get; private set; }

        private CategoriaServicio() { }

        public static Result<CategoriaServicio> Crear(string nombre, string? descripcion)
        {
            var validationResult = ValidarNombre(nombre);
            if (validationResult.IsFailure)
            {
                return Result<CategoriaServicio>.Failure(validationResult.ErrorCode!, validationResult.ErrorMessage!);
            }

            return Result<CategoriaServicio>.Success(new CategoriaServicio
            {
                Id = 0,
                Nombre = validationResult.Value!,
                Descripcion = descripcion?.Trim(),
                Estado = true,
                FechaCreacion = DateTime.UtcNow
            });
        }

        public static Result<CategoriaServicio> Reconstituir(int id, string nombre, string? descripcion, bool estado, DateTime fechaCreacion, DateTime? fechaModificacion)
        {
            if (id <= 0)
            {
                return Result<CategoriaServicio>.Failure(ErrorCodes.ValidationInvalidValue, "El identificador de la categoría no es válido.");
            }

            var validationResult = ValidarNombre(nombre);
            if (validationResult.IsFailure)
            {
                return Result<CategoriaServicio>.Failure(validationResult.ErrorCode!, validationResult.ErrorMessage!);
            }

            return Result<CategoriaServicio>.Success(new CategoriaServicio
            {
                Id = id,
                Nombre = validationResult.Value!,
                Descripcion = descripcion?.Trim(),
                Estado = estado,
                FechaCreacion = fechaCreacion,
                FechaModificacion = fechaModificacion
            });
        }

        public Result Modificar(string nombre, string? descripcion)
        {
            var validationResult = ValidarNombre(nombre);
            if (validationResult.IsFailure)
            {
                return Result.Failure(validationResult.ErrorCode!, validationResult.ErrorMessage!);
            }

            Nombre = validationResult.Value!;
            Descripcion = descripcion?.Trim();
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
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "El identificador de la categoría no es válido.");
            }

            if (Id > 0 && Id != id)
            {
                return Result.Failure(ErrorCodes.ValidationInvalidValue, "El identificador de la categoría ya fue asignado.");
            }

            Id = id;
            return Result.Success();
        }

        private static Result<string> ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return Result<string>.Failure(ErrorCodes.ValidationRequired, "El nombre de la categoría es obligatorio.");
            }

            var trimmed = nombre.Trim();
            if (trimmed.Length > 100)
            {
                return Result<string>.Failure(ErrorCodes.ValidationRequired, "El nombre no puede tener más de 100 caracteres.");
            }

            return Result<string>.Success(trimmed);
        }
    }
}
