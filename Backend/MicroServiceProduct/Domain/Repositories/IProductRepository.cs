using MicroServiceProduct.Domain.Entities;

namespace MicroServiceProduct.Domain.Repositories;

public interface IProductRepository : IRepository<Product, Guid>
{
    /// <summary>
    /// Eliminación lógica de un producto, registrando quién lo eliminó.
    /// </summary>
    Task DeleteAsync(Guid id, string? deletedBy, CancellationToken ct = default);
}
