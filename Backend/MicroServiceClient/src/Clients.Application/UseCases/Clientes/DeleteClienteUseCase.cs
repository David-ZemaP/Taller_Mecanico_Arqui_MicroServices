using Taller_Mecanico_Clientes.Domain.Common;
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

        public async Task<Result> ExecuteAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return Result.Failure(ErrorCodes.ValidationRequired, "El ID del cliente es requerido.");
            }
            return await _repository.DeleteAsync(id);
        }
    }
}
