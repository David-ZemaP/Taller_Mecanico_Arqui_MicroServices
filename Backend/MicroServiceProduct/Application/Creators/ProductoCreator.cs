using MicroServiceProduct.Domain.Entities;
using MicroServiceProduct.Domain.Repositories;

namespace MicroServiceProduct.Application.Creators;

/// <summary>
/// ConcreteCreator del patrón Factory Method para Product.
/// Delega la implementación concreta al repositorio en Infrastructure.
/// </summary>
public class ProductoCreator : CrudService<Product, Guid>
{
    public ProductoCreator(IProductRepository repository) : base(repository) { }
}
