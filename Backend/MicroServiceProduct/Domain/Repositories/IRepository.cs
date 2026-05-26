namespace MicroServiceProduct.Domain.Repositories;

/// <summary>
/// Repositorio genérico con operaciones CRUD básicas.
/// </summary>
/// <typeparam name="T">Entidad de dominio</typeparam>
/// <typeparam name="TId">Tipo del identificador (Guid, int, etc.)</typeparam>
public interface IRepository<T, TId> where T : class
{
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task<T?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(TId id, CancellationToken ct = default);
}
