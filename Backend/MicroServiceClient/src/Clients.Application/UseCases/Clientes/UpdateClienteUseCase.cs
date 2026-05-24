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

        public async Task<Result<Cliente>> ExecuteAsync(string id, Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Result<Cliente>.Failure(ErrorCodes.ValidationRequired, "El ID del cliente es requerido.");
            }
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                return Result<Cliente>.Failure(ErrorCodes.ValidationRequired, "El nombre del cliente es requerido.");
            }
            if (string.IsNullOrWhiteSpace(cliente.Email))
            {
                return Result<Cliente>.Failure(ErrorCodes.ValidationRequired, "El email del cliente es requerido.");
            }

            return await _repository.UpdateAsync(id, cliente);
        }
    }
}
