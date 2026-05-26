using Microsoft.EntityFrameworkCore;
using MicroServiceProduct.Domain.Entities;
using MicroServiceProduct.Domain.Repositories;
using MicroServiceProduct.Infrastructure.Persistence;

namespace MicroServiceProduct.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductsDbContext _db;

    public ProductRepository(ProductsDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Product product, CancellationToken ct = default)
    {
        await _db.Products.AddAsync(product, ct);
        await _db.SaveChangesAsync(ct);
    }

    // ────────────── IRepository<Product, Guid> ──────────────

    async Task IRepository<Product, Guid>.DeleteAsync(Guid id, CancellationToken ct)
        => await DeleteAsync(id, deletedBy: null, ct);

    // ────────────── IProductRepository ──────────────

    public async Task DeleteAsync(Guid id, string? deletedBy, CancellationToken ct = default)
    {
        var p = await _db.Products.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return;
        p.Eliminar(deletedBy);
        _db.Products.Update(p);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Products.AsNoTracking().ToListAsync(ct);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
    }
}
