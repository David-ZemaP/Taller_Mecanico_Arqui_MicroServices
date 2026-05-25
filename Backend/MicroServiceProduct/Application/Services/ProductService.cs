using MicroServiceProduct.Domain.Entities;
using MicroServiceProduct.Domain.Repositories;
using MicroServiceProduct.Application.DTOs;

namespace MicroServiceProduct.Application.Services;

public class ProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repo.GetAllAsync(ct);
        return items.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.CreatedAt));
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        return p is null ? null : new ProductDto(p.Id, p.Name, p.Description, p.Price, p.CreatedAt);
    }

    public async Task<ProductDto> CreateAsync(string name, string? description, decimal price, CancellationToken ct = default)
    {
        var p = new Product { Id = Guid.NewGuid(), Name = name, Description = description, Price = price };
        await _repo.AddAsync(p, ct);
        return new ProductDto(p.Id, p.Name, p.Description, p.Price, p.CreatedAt);
    }

    public async Task<bool> UpdateAsync(Guid id, string name, string? description, decimal price, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;
        existing.Name = name;
        existing.Description = description;
        existing.Price = price;
        await _repo.UpdateAsync(existing, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;
        await _repo.DeleteAsync(id, ct);
        return true;
    }
}
