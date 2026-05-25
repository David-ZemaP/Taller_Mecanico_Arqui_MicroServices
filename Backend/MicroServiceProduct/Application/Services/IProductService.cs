using MicroServiceProduct.Application.DTOs;

namespace MicroServiceProduct.Application.Services;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync(CancellationToken ct = default);
    Task<ProductDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(string name, string? description, decimal price, CancellationToken ct = default);
    Task<bool> UpdateAsync(Guid id, string name, string? description, decimal price, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}
