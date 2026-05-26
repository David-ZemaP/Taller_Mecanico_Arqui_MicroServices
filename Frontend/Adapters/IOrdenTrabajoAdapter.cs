using WebService.DTOs;

namespace WebService.Adapters;

public interface IOrdenTrabajoAdapter : IAdapter
{
    // Órdenes de trabajo
    Task<List<OrdenTrabajoListDto>> GetAllOrdenesAsync();
    Task<OrdenTrabajoDetalleDto?> GetOrdenDetalleAsync(int id);
    Task<List<OrdenTrabajoListDto>> GetOrdenesByMecanicoAsync(int mecanicoId);
    Task<(bool ok, string? error, int id)> RegistrarOrdenAsync(OrdenTrabajoFormDto form);
    Task<(bool ok, string? error)> ActualizarOrdenAsync(OrdenTrabajoFormDto form);
    Task<(bool ok, string? error)> AnularOrdenAsync(int id);
    Task<(bool ok, string? error)> RestaurarOrdenAsync(int id);
    // Vehículos
    Task<List<VehiculoListDto>> GetAllVehiculosAsync();
    Task<List<VehiculoLookupDto>> BuscarVehiculosAsync(string term, int? clienteId = null);
    Task<List<VehiculoLookupDto>> BuscarVehiculosPorPlacaAsync(string term, int? clienteId = null);
    Task<(bool ok, string? error, int vehiculoId)> SaveVehiculoAsync(VehiculoFormDto form);
    Task DeleteVehiculoAsync(int id);
    Task<List<VehiculoListDto>> GetVehiculosByClienteAsync(int clienteId);
    // Clientes (delegados a IClientesAdapter)
    Task<List<ClienteLookupDto>> GetAllClientesAsync();
    Task<List<ClienteLookupDto>> BuscarClientesAsync(string term);
    Task<ClienteLookupDto?> GetClienteAsync(int id);
    Task<(bool ok, string? error)> DeleteClienteAsync(int id);
    Task<(bool ok, string? error, ClienteLookupDto? cliente)> SaveClienteAsync(ClienteFormDto form);
    // Productos
    Task<List<ProductoDto>> GetAllProductosAsync();
    Task<ProductoDto?> GetProductoAsync(int id);
    Task<(bool ok, string? error)> SaveProductoAsync(ProductoFormDto form);
    Task DeleteProductoAsync(int id);
    // Servicios
    Task<List<ServicioDto>> GetAllServiciosAsync();
    Task<ServicioDto?> GetServicioAsync(int id);
    Task<(bool ok, string? error)> SaveServicioAsync(ServicioFormDto form);
    Task DeleteServicioAsync(int id);
    // Catálogos
    Task<List<MarcaDto>> GetAllMarcasAsync();
    Task<List<ModeloDto>> GetAllModelosAsync();
    Task<List<ColorVehiculoDto>> GetAllColoresAsync();
}
