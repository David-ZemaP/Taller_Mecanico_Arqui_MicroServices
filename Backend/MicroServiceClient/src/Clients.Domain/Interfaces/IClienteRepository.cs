using Taller_Mecanico_Clientes.Domain.Common;
using Taller_Mecanico_Clientes.Domain.Entities;

namespace Taller_Mecanico_Clientes.Domain.Interfaces
{
    public interface IClienteRepository
    {
        Task<Result<List<Cliente>>> GetAllAsync();
        Task<Result<Cliente?>> GetByIdAsync(string id);
        Task<Result<Cliente>> CreateAsync(Cliente cliente);
        Task<Result<Cliente>> UpdateAsync(string id, Cliente cliente);
        Task<Result> DeleteAsync(string id);
    }
}
