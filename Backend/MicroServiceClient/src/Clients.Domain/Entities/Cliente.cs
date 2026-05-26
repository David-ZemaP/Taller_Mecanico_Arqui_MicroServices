using Taller_Mecanico_Clientes.Domain.Common;

namespace Taller_Mecanico_Clientes.Domain.Entities
{
    public class Cliente
    {
        public string Id { get; private set; } = string.Empty;
        public string Nombre { get; private set; } = string.Empty;
        public string PrimerApellido { get; private set; } = string.Empty;
        public string? SegundoApellido { get; private set; }
        public int Ci { get; private set; }
        public string? CiComplemento { get; private set; }
        public int Telefono { get; private set; }
        public string Email { get; private set; } = string.Empty;
        public int? UsuarioLoginId { get; private set; }
        public string? CreadoPor { get; private set; }
        public string? ActualizadoPor { get; private set; }
        public string? EliminadoPor { get; private set; }
        public DateTime? FechaActualizacion { get; private set; }
        public bool IsDeleted { get; private set; }
        public DateTime FechaRegistro { get; private set; }
        public string TipoCliente { get; private set; } = "Regular";

        // Parameterless constructor for Firestore deserialization via ClienteDocument
        private Cliente() { }

        /// <summary>
        /// Factory method: crea un nuevo cliente con validación.
        /// </summary>
        public static Result<Cliente> Crear(
            string nombre,
            string primerApellido,
            string? segundoApellido,
            int ci,
            string? ciComplemento,
            int telefono,
            string email,
            string? tipoCliente = null)
        {
            var valNombre = ValidarNombre(nombre);
            if (valNombre.IsFailure)
                return Result<Cliente>.Failure(valNombre.ErrorCode!, valNombre.ErrorMessage!);

            var valApellido = ValidarPrimerApellido(primerApellido);
            if (valApellido.IsFailure)
                return Result<Cliente>.Failure(valApellido.ErrorCode!, valApellido.ErrorMessage!);

            var valTelefono = ValidarTelefono(telefono);
            if (valTelefono.IsFailure)
                return Result<Cliente>.Failure(valTelefono.ErrorCode!, valTelefono.ErrorMessage!);

            var valEmail = ValidarEmail(email);
            if (valEmail.IsFailure)
                return Result<Cliente>.Failure(valEmail.ErrorCode!, valEmail.ErrorMessage!);

            if (ci <= 0)
                return Result<Cliente>.Failure(ErrorCodes.ValidationRequired, "El número de CI (cédula de identidad) debe ser un valor válido.");

            var tipo = string.IsNullOrWhiteSpace(tipoCliente) ? "Regular" : tipoCliente.Trim();

            return Result<Cliente>.Success(new Cliente
            {
                Nombre = valNombre.Value!,
                PrimerApellido = valApellido.Value!,
                SegundoApellido = segundoApellido?.Trim(),
                Ci = ci,
                CiComplemento = ciComplemento?.Trim(),
                Telefono = telefono,
                Email = valEmail.Value!,
                TipoCliente = tipo,
                FechaRegistro = DateTime.UtcNow,
                IsDeleted = false
            });
        }

        /// <summary>
        /// Factory method: reconstituye un cliente desde Firestore.
        /// </summary>
        public static Result<Cliente> Reconstituir(
            string id,
            string nombre,
            string primerApellido,
            string? segundoApellido,
            int ci,
            string? ciComplemento,
            int telefono,
            string email,
            int? usuarioLoginId,
            string? creadoPor,
            string? actualizadoPor,
            string? eliminadoPor,
            DateTime? fechaActualizacion,
            bool isDeleted,
            DateTime fechaRegistro,
            string tipoCliente)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Result<Cliente>.Failure(ErrorCodes.ValidationRequired, "El ID del cliente no es válido.");

            var valNombre = ValidarNombre(nombre);
            if (valNombre.IsFailure)
                return Result<Cliente>.Failure(valNombre.ErrorCode!, valNombre.ErrorMessage!);

            var valApellido = ValidarPrimerApellido(primerApellido);
            if (valApellido.IsFailure)
                return Result<Cliente>.Failure(valApellido.ErrorCode!, valApellido.ErrorMessage!);

            var valTelefono = ValidarTelefono(telefono);
            if (valTelefono.IsFailure)
                return Result<Cliente>.Failure(valTelefono.ErrorCode!, valTelefono.ErrorMessage!);

            var valEmail = ValidarEmail(email);
            if (valEmail.IsFailure)
                return Result<Cliente>.Failure(valEmail.ErrorCode!, valEmail.ErrorMessage!);

            if (ci <= 0)
                return Result<Cliente>.Failure(ErrorCodes.ValidationRequired, "El número de CI (cédula de identidad) debe ser un valor válido.");

            return Result<Cliente>.Success(new Cliente
            {
                Id = id,
                Nombre = valNombre.Value!,
                PrimerApellido = valApellido.Value!,
                SegundoApellido = segundoApellido?.Trim(),
                Ci = ci,
                CiComplemento = ciComplemento?.Trim(),
                Telefono = telefono,
                Email = valEmail.Value!,
                UsuarioLoginId = usuarioLoginId,
                CreadoPor = creadoPor,
                ActualizadoPor = actualizadoPor,
                EliminadoPor = eliminadoPor,
                FechaActualizacion = fechaActualizacion,
                IsDeleted = isDeleted,
                FechaRegistro = fechaRegistro,
                TipoCliente = string.IsNullOrWhiteSpace(tipoCliente) ? "Regular" : tipoCliente
            });
        }

        /// <summary>
        /// Modifica los datos del cliente con validación.
        /// No permite cambiar CI ni FechaRegistro (son inmutables después de la creación).
        /// </summary>
        public Result Modificar(
            string nombre,
            string primerApellido,
            string? segundoApellido,
            int telefono,
            string email,
            int? usuarioLoginId,
            string? tipoCliente)
        {
            var valNombre = ValidarNombre(nombre);
            if (valNombre.IsFailure)
                return Result.Failure(valNombre.ErrorCode!, valNombre.ErrorMessage!);

            var valApellido = ValidarPrimerApellido(primerApellido);
            if (valApellido.IsFailure)
                return Result.Failure(valApellido.ErrorCode!, valApellido.ErrorMessage!);

            var valTelefono = ValidarTelefono(telefono);
            if (valTelefono.IsFailure)
                return Result.Failure(valTelefono.ErrorCode!, valTelefono.ErrorMessage!);

            var valEmail = ValidarEmail(email);
            if (valEmail.IsFailure)
                return Result.Failure(valEmail.ErrorCode!, valEmail.ErrorMessage!);

            Nombre = valNombre.Value!;
            PrimerApellido = valApellido.Value!;
            SegundoApellido = segundoApellido?.Trim();
            Telefono = telefono;
            Email = valEmail.Value!;
            UsuarioLoginId = usuarioLoginId;
            TipoCliente = string.IsNullOrWhiteSpace(tipoCliente) ? "Regular" : tipoCliente.Trim();
            FechaActualizacion = DateTime.UtcNow;

            return Result.Success();
        }

        /// <summary>
        /// Soft delete: marca el cliente como eliminado.
        /// </summary>
        public void Eliminar(string? eliminadoPor)
        {
            IsDeleted = true;
            EliminadoPor = eliminadoPor;
            FechaActualizacion = DateTime.UtcNow;
        }

        /// <summary>
        /// Asigna el ID de Firestore después de la creación.
        /// </summary>
        public void AsignarIdentificador(string id)
        {
            if (!string.IsNullOrWhiteSpace(id))
                Id = id;
        }

        /// <summary>
        /// Asigna el creador después de la creación.
        /// </summary>
        public void AsignarCreadoPor(string? creadoPor)
        {
            CreadoPor = creadoPor;
        }

        // ────────────── Validaciones privadas ──────────────

        private static Result<string> ValidarNombre(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return Result<string>.Failure(ErrorCodes.ValidationRequired, "El nombre del cliente es obligatorio.");

            var trimmed = nombre.Trim();
            if (trimmed.Length > 100)
                return Result<string>.Failure(ErrorCodes.ValidationInvalidValue, "El nombre no puede exceder 100 caracteres.");

            return Result<string>.Success(trimmed);
        }

        private static Result<string> ValidarPrimerApellido(string apellido)
        {
            if (string.IsNullOrWhiteSpace(apellido))
                return Result<string>.Failure(ErrorCodes.ValidationRequired, "El primer apellido del cliente es obligatorio.");

            var trimmed = apellido.Trim();
            if (trimmed.Length > 100)
                return Result<string>.Failure(ErrorCodes.ValidationInvalidValue, "El primer apellido no puede exceder 100 caracteres.");

            return Result<string>.Success(trimmed);
        }

        private static Result ValidarTelefono(int telefono)
        {
            if (telefono <= 0)
                return Result.Failure(ErrorCodes.ValidationRequired, "El número de teléfono debe ser un valor válido.");

            return Result.Success();
        }

        private static Result<string> ValidarEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Result<string>.Failure(ErrorCodes.ValidationRequired, "El email del cliente es requerido.");

            var trimmed = email.Trim();
            if (!trimmed.Contains('@') || !trimmed.Contains('.'))
                return Result<string>.Failure(ErrorCodes.ValidationInvalidValue, "El email no es válido.");

            return Result<string>.Success(trimmed);
        }
    }
}
