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

        public async Task<Result<Cliente>> ExecuteAsync(Cliente cliente)
        {
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                return Result<Cliente>.Failure(ErrorCodes.ValidationRequired, "El nombre del cliente es requerido.");
            }
            if (string.IsNullOrWhiteSpace(cliente.PrimerApellido))
            {
                return Result<Cliente>.Failure(ErrorCodes.ValidationRequired, "El primer apellido del cliente es requerido.");
            }
            if (string.IsNullOrWhiteSpace(cliente.Email))
            {
                return Result<Cliente>.Failure(ErrorCodes.ValidationRequired, "El email del cliente es requerido.");
            }
            if (cliente.Telefono <= 0)
            {
                return Result<Cliente>.Failure(ErrorCodes.ValidationRequired, "El número de teléfono debe ser un valor válido.");
            }
            if (cliente.Ci <= 0)
            {
                return Result<Cliente>.Failure(ErrorCodes.ValidationRequired, "El número de CI (cédula de identidad) debe ser un valor válido.");
            }

            cliente.FechaRegistro = DateTime.UtcNow;
            cliente.IsDeleted = false;
            
            if (string.IsNullOrWhiteSpace(cliente.TipoCliente))
            {
                cliente.TipoCliente = "Regular";
            }

            return await _repository.CreateAsync(cliente);
        }
    }
}
