using MicroServiceProduct.Domain.Entities;
using MicroServiceProduct.Domain.Repositories;
using MicroServiceProduct.Application.DTOs;

namespace MicroServiceProduct.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken ct = default)
    {
        var items = await _repo.GetAllAsync(ct);
        return items.Select(ToDto);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _repo.GetByIdAsync(id, ct);
        return p is null ? null : ToDto(p);
    }

    public async Task<ProductDto> CreateAsync(string name, string? description, decimal price, int stock, string? createdBy, CancellationToken ct = default)
    {
        var result = Product.Crear(name, description, price, stock);
        if (result.IsFailure)
            throw new InvalidOperationException(result.ErrorMessage);

        var p = result.Value!;
        p.AsignarCreadoPor(createdBy);
        await _repo.AddAsync(p, ct);
        return ToDto(p);
    }

    public async Task<bool> UpdateAsync(Guid id, string name, string? description, decimal price, int stock, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;

        var modifyResult = existing.Modificar(name, description, price, stock);
        if (modifyResult.IsFailure) return false;

        await _repo.UpdateAsync(existing, ct);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, string? deletedBy, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(id, ct);
        if (existing is null) return false;

        existing.Eliminar(deletedBy);
        await _repo.UpdateAsync(existing, ct);
        return true;
    }

    private static ProductDto ToDto(Product p)
        => new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.CreatedAt, p.CreatedBy, p.DeletedBy);
}
