using Taller_Mecanico_Clientes.Domain.Common;
using Taller_Mecanico_Clientes.Domain.Entities;
using Taller_Mecanico_Clientes.Domain.Interfaces;

namespace Taller_Mecanico_Clientes.Application.UseCases.Clientes
{
    public class UpdateClienteUseCase
    {
        private readonly IClienteRepository _repository;

        public UpdateClienteUseCase(IClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Cliente>> ExecuteAsync(
            string id,
            string nombre,
            string primerApellido,
            string? segundoApellido,
            int telefono,
            string email,
            int? usuarioLoginId,
            string? tipoCliente = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Result<Cliente>.Failure(ErrorCodes.ValidationRequired, "El ID del cliente es requerido.");

            var existingResult = await _repository.GetByIdAsync(id);
            if (existingResult.IsFailure)
                return Result<Cliente>.Failure(existingResult.ErrorCode!, existingResult.ErrorMessage!);

            if (existingResult.Value is null)
                return Result<Cliente>.Failure(ErrorCodes.ClienteNotFound, "El cliente a actualizar no existe.");

            var existing = existingResult.Value;

            var modifyResult = existing.Modificar(nombre, primerApellido, segundoApellido, telefono, email, usuarioLoginId, tipoCliente);
            if (modifyResult.IsFailure)
                return Result<Cliente>.Failure(modifyResult.ErrorCode!, modifyResult.ErrorMessage!);

            return await _repository.UpdateAsync(id, existing);
        }
    }
}
