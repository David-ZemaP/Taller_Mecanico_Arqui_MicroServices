using WebService.DTOs;

namespace WebService.Adapters;

public interface IClientesAdapter : IAdapter
{
    Task<(bool ok, ClienteDto? cliente, string? error)> GetClienteByIdAsync(int id);
    Task<List<ClienteLookupDto>> GetAllClientesAsync();
    Task<ClienteLookupDto?> GetClienteAsync(int id);
    Task<List<ClienteLookupDto>> BuscarClientesAsync(string term);
    Task<List<VehiculoListDto>> GetVehiculosByClienteAsync(int clienteId);
    Task<(bool ok, string? error, ClienteLookupDto? cliente)> SaveClienteAsync(ClienteFormDto form);
    Task<(bool ok, string? error)> DeleteClienteAsync(int id);
}
