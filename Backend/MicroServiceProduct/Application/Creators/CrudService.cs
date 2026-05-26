using MicroServiceProduct.Domain.Repositories;
using MicroServiceProduct.Application.Common;

namespace MicroServiceProduct.Application.Creators;

/// <summary>
/// Creator abstracto del patrón Factory Method.
/// Define el esqueleto CRUD y un hook de fábrica que los ConcreteCreators implementan.
/// </summary>
public abstract class CrudService<T, TId> where T : class
{
    protected readonly IRepository<T, TId> Repository;

    protected CrudService(IRepository<T, TId> repository)
    {
        Repository = repository;
    }

    // ─── Query ───
    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default)
        => await Repository.GetAllAsync(ct);

    public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken ct = default)
        => await Repository.GetByIdAsync(id, ct);

    // ─── Command ───
    public virtual async Task DeleteAsync(TId id, CancellationToken ct = default)
        => await Repository.DeleteAsync(id, ct);
}
