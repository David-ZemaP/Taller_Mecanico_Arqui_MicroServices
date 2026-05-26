using MicroServiceProduct.Domain.Entities;
using MicroServiceProduct.Domain.Repositories;
using MicroServiceProduct.Application.DTOs;
using MicroServiceProduct.Application.Common;

namespace MicroServiceProduct.Application.Services;

/// <summary>
/// Implementación del servicio de productos con patrón Result y validaciones.
/// </summary>
public class ProductResultService : IProductResultService
{
    private readonly IProductRepository _repo;

    public ProductResultService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<IEnumerable<ProductDto>>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            var items = await _repo.GetAllAsync(ct);
            var dtos = items.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.CreatedAt)).ToList();
            return Result<IEnumerable<ProductDto>>.Success(dtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ProductDto>>.Failure(
                ErrorCodes.DbError,
                $"Error al obtener productos: {ex.Message}");
        }
    }

    public async Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var validationResult = ProductValidator.ValidateProductId(id);
            if (validationResult.IsFailure)
                return Result<ProductDto>.Failure(validationResult.ErrorCode!, validationResult.ErrorMessage!);

            var p = await _repo.GetByIdAsync(id, ct);
            if (p is null)
                return Result<ProductDto>.Failure(
                    ErrorCodes.ProductNotFound,
                    ErrorMessages.GetMessage(ErrorCodes.ProductNotFound));

            var dto = new ProductDto(p.Id, p.Name, p.Description, p.Price, p.CreatedAt);
            return Result<ProductDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ProductDto>.Failure(
                ErrorCodes.DbError,
                $"Error al obtener producto: {ex.Message}");
        }
    }

    public async Task<Result<ProductDto>> CreateAsync(string name, string? description, decimal price, CancellationToken ct = default)
    {
        try
        {
            var validationResult = ProductValidator.ValidateCreateProduct(name, description, price);
            if (validationResult.IsFailure)
                return Result<ProductDto>.Failure(validationResult.ErrorCode!, validationResult.ErrorMessage!);

            var p = new Product 
            { 
                Id = Guid.NewGuid(), 
                Name = name!, 
                Description = description, 
                Price = price,
                CreatedAt = DateTime.UtcNow
            };
            
            await _repo.AddAsync(p, ct);
            var dto = new ProductDto(p.Id, p.Name, p.Description, p.Price, p.CreatedAt);
            return Result<ProductDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<ProductDto>.Failure(
                ErrorCodes.DbError,
                $"Error al crear producto: {ex.Message}");
        }
    }

    public async Task<Result> UpdateAsync(Guid id, string name, string? description, decimal price, CancellationToken ct = default)
    {
        try
        {
            var validationResult = ProductValidator.ValidateProductId(id);
            if (validationResult.IsFailure)
                return Result.Failure(validationResult.ErrorCode!, validationResult.ErrorMessage!);

            var validateDataResult = ProductValidator.ValidateUpdateProduct(name, description, price);
            if (validateDataResult.IsFailure)
                return Result.Failure(validateDataResult.ErrorCode!, validateDataResult.ErrorMessage!);

            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing is null)
                return Result.Failure(
                    ErrorCodes.ProductNotFound,
                    ErrorMessages.GetMessage(ErrorCodes.ProductNotFound));

            existing.Name = name!;
            existing.Description = description;
            existing.Price = price;
            
            await _repo.UpdateAsync(existing, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                ErrorCodes.DbError,
                $"Error al actualizar producto: {ex.Message}");
        }
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            var validationResult = ProductValidator.ValidateProductId(id);
            if (validationResult.IsFailure)
                return Result.Failure(validationResult.ErrorCode!, validationResult.ErrorMessage!);

            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing is null)
                return Result.Failure(
                    ErrorCodes.ProductNotFound,
                    ErrorMessages.GetMessage(ErrorCodes.ProductNotFound));

            await _repo.DeleteAsync(id, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                ErrorCodes.DbError,
                $"Error al eliminar producto: {ex.Message}");
        }
    }
}
