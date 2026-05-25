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

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _db.Products.FindAsync(new object[] { id }, ct);
        if (p is null) return;
        _db.Products.Remove(p);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Products.AsNoTracking().ToListAsync(ct);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Products.FindAsync(new object[] { id }, ct);
    }

    public async Task UpdateAsync(Product product, CancellationToken ct = default)
    {
        _db.Products.Update(product);
        await _db.SaveChangesAsync(ct);
    }
}
