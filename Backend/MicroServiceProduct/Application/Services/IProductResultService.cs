using MicroServiceProduct.Application.DTOs;
using MicroServiceProduct.Application.Common;

namespace MicroServiceProduct.Application.Services;

/// <summary>
/// Interfaz para el servicio de productos con patrón Result.
/// Basado en la estructura simplificada del microservicio de Servicios.
/// </summary>
public interface IProductResultService
{
    /// <summary>
    /// Obtiene todos los productos.
    /// </summary>
    Task<Result<IEnumerable<ProductDto>>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Obtiene un producto por ID.
    /// Retorna 404 si no existe.
    /// </summary>
    Task<Result<ProductDto>> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Crea un nuevo producto.
    /// Retorna 400 si hay validación fallida.
    /// </summary>
    Task<Result<ProductDto>> CreateAsync(string name, string? description, decimal price, CancellationToken ct = default);

    /// <summary>
    /// Actualiza un producto existente.
    /// Retorna 404 si no existe, 400 si hay validación fallida.
    /// </summary>
    Task<Result> UpdateAsync(Guid id, string name, string? description, decimal price, CancellationToken ct = default);

    /// <summary>
    /// Elimina un producto.
    /// Retorna 404 si no existe.
    /// </summary>
    Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);
}
