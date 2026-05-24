using Taller_Mecanico_Clientes.Domain.Common;
using Taller_Mecanico_Clientes.Domain.Entities;
using Taller_Mecanico_Clientes.Domain.Interfaces;

namespace Taller_Mecanico_Clientes.Application.UseCases.Clientes
{
    public class GetClientesUseCase
    {
        private readonly IClienteRepository _repository;

        public GetClientesUseCase(IClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<List<Cliente>>> ExecuteAsync()
        {
            return await _repository.GetAllAsync();
        }
    }
}
