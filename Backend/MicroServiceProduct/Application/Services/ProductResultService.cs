using MicroServiceProduct.Domain.Entities;
using MicroServiceProduct.Domain.Repositories;
using MicroServiceProduct.Application.DTOs;
using MicroServiceProduct.Application.Common;

namespace MicroServiceProduct.Application.Services;

/// <summary>
/// Implementación del servicio de productos con patrón Result y validaciones.
/// Usa los factory methods de la entidad Product (Crear, Modificar, Eliminar).
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
            var dtos = items.Select(ToDto).ToList();
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
            if (id == Guid.Empty)
                return Result<ProductDto>.Failure(
                    ErrorCodes.ProductInvalidId,
                    "El ID del producto no es válido.");

            var p = await _repo.GetByIdAsync(id, ct);
            if (p is null)
                return Result<ProductDto>.Failure(
                    ErrorCodes.ProductNotFound,
                    ErrorMessages.GetMessage(ErrorCodes.ProductNotFound));

            return Result<ProductDto>.Success(ToDto(p));
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
            var result = Product.Crear(name, description, price, stock: 0);
            if (result.IsFailure)
                return Result<ProductDto>.Failure(result.ErrorCode!, result.ErrorMessage!);

            var p = result.Value!;
            await _repo.AddAsync(p, ct);
            return Result<ProductDto>.Success(ToDto(p));
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
            if (id == Guid.Empty)
                return Result.Failure(
                    ErrorCodes.ProductInvalidId,
                    "El ID del producto no es válido.");

            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing is null)
                return Result.Failure(
                    ErrorCodes.ProductNotFound,
                    ErrorMessages.GetMessage(ErrorCodes.ProductNotFound));

            var modifyResult = existing.Modificar(name, description, price, existing.Stock);
            if (modifyResult.IsFailure)
                return modifyResult;

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
            if (id == Guid.Empty)
                return Result.Failure(
                    ErrorCodes.ProductInvalidId,
                    "El ID del producto no es válido.");

            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing is null)
                return Result.Failure(
                    ErrorCodes.ProductNotFound,
                    ErrorMessages.GetMessage(ErrorCodes.ProductNotFound));

            existing.Eliminar(deletedBy: null);
            await _repo.UpdateAsync(existing, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure(
                ErrorCodes.DbError,
                $"Error al eliminar producto: {ex.Message}");
        }
    }

    private static ProductDto ToDto(Product p)
        => new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.CreatedAt, p.CreatedBy, p.DeletedBy);
}
