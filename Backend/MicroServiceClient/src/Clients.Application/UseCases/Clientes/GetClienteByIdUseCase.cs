using Taller_Mecanico_Clientes.Domain.Common;
using Taller_Mecanico_Clientes.Domain.Entities;
using Taller_Mecanico_Clientes.Domain.Interfaces;

namespace Taller_Mecanico_Clientes.Application.UseCases.Clientes
{
    public class GetClienteByIdUseCase
    {
        private readonly IClienteRepository _repository;

        public GetClienteByIdUseCase(IClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Cliente?>> ExecuteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Result<Cliente?>.Failure(ErrorCodes.ValidationRequired, "El ID del cliente es requerido.");
            }
            return await _repository.GetByIdAsync(id);
        }
    }
}
