using Taller_Mecanico_Clientes.Domain.Common;
using Taller_Mecanico_Clientes.Domain.Entities;
using Taller_Mecanico_Clientes.Domain.Interfaces;

namespace Taller_Mecanico_Clientes.Application.UseCases.Clientes
{
    public class DeleteClienteUseCase
    {
        private readonly IClienteRepository _repository;

        public DeleteClienteUseCase(IClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result> ExecuteAsync(string id, string? eliminadoPor = null)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Result.Failure(ErrorCodes.ValidationRequired, "El ID del cliente es requerido.");

            var existingResult = await _repository.GetByIdAsync(id);
            if (existingResult.IsFailure)
                return Result.Failure(existingResult.ErrorCode!, existingResult.ErrorMessage!);

            if (existingResult.Value is null)
                return Result.Failure(ErrorCodes.ClienteNotFound, "El cliente a eliminar no existe.");

            var existing = existingResult.Value;
            existing.Eliminar(eliminadoPor);

            var updateResult = await _repository.UpdateAsync(id, existing);
            if (updateResult.IsFailure)
                return Result.Failure(updateResult.ErrorCode!, updateResult.ErrorMessage!);

            return Result.Success();
        }
    }
}
