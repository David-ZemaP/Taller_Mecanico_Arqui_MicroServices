using Taller_Mecanico_Clientes.Domain.Common;
using Taller_Mecanico_Clientes.Domain.Entities;
using Taller_Mecanico_Clientes.Domain.Interfaces;

namespace Taller_Mecanico_Clientes.Application.UseCases.Clientes
{
    public class CreateClienteUseCase
    {
        private readonly IClienteRepository _repository;

        public CreateClienteUseCase(IClienteRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Cliente>> ExecuteAsync(
            string nombre,
            string primerApellido,
            string? segundoApellido,
            int ci,
            string? ciComplemento,
            int telefono,
            string email,
            string? tipoCliente = null)
        {
            var result = Cliente.Crear(nombre, primerApellido, segundoApellido, ci, ciComplemento, telefono, email, tipoCliente);
            if (result.IsFailure)
                return result;

            return await _repository.CreateAsync(result.Value!);
        }
    }
}
