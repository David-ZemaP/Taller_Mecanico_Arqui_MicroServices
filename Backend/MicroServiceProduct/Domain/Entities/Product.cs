using MicroServiceProduct.Application.Common;

namespace MicroServiceProduct.Domain.Entities;

public class Product
{
    // EF Core needs these to be gettable; setters are private for domain integrity
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public string? DeletedBy { get; private set; }

    // Parameterless constructor for EF Core
    private Product() { }

    /// <summary>
    /// Factory method: crea un nuevo producto con validación.
    /// </summary>
    public static Result<Product> Crear(string name, string? description, decimal price, int stock = 0)
    {
        var validation = Validate(name, description, price);
        if (validation.IsFailure)
            return Result<Product>.Failure(validation.ErrorCode!, validation.ErrorMessage!);

        return Result<Product>.Success(new Product
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Price = price,
            Stock = stock,
            CreatedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Factory method: reconstituye un producto desde la base de datos.
    /// </summary>
    public static Result<Product> Reconstituir(Guid id, string name, string? description, decimal price, int stock, DateTime createdAt, bool isDeleted, DateTime? deletedAt, string? createdBy, string? deletedBy)
    {
        if (id == Guid.Empty)
            return Result<Product>.Failure(ErrorCodes.ProductInvalidId, "El ID del producto no es válido.");

        var validation = Validate(name, description, price);
        if (validation.IsFailure)
            return Result<Product>.Failure(validation.ErrorCode!, validation.ErrorMessage!);

        return Result<Product>.Success(new Product
        {
            Id = id,
            Name = name.Trim(),
            Description = description?.Trim(),
            Price = price,
            Stock = stock,
            CreatedAt = createdAt,
            IsDeleted = isDeleted,
            DeletedAt = deletedAt,
            CreatedBy = createdBy,
            DeletedBy = deletedBy
        });
    }

    /// <summary>
    /// Modifica los datos del producto con validación.
    /// </summary>
    public Result Modificar(string name, string? description, decimal price, int stock)
    {
        var validation = Validate(name, description, price);
        if (validation.IsFailure)
            return validation;

        Name = name.Trim();
        Description = description?.Trim();
        Price = price;
        Stock = stock;
        return Result.Success();
    }

    /// <summary>
    /// Soft delete: marca el producto como eliminado.
    /// </summary>
    public void Eliminar(string? deletedBy)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    /// <summary>
    /// Asigna el CreatedBy después de la creación (llamado por el repositorio/usecase).
    /// </summary>
    public void AsignarCreadoPor(string? createdBy)
    {
        CreatedBy = createdBy;
    }

    private static Result Validate(string name, string? description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(ErrorCodes.ProductNameRequired, "El nombre del producto es obligatorio.");
        if (name.Trim().Length < 3)
            return Result.Failure(ErrorCodes.ProductNameTooShort, "El nombre debe tener al menos 3 caracteres.");
        if (name.Trim().Length > 200)
            return Result.Failure(ErrorCodes.ProductNameTooLong, "El nombre no puede exceder 200 caracteres.");
        if (!string.IsNullOrEmpty(description) && description.Trim().Length > 1000)
            return Result.Failure(ErrorCodes.ProductDescriptionTooLong, "La descripción no puede exceder 1000 caracteres.");
        if (price < 0)
            return Result.Failure(ErrorCodes.ProductPriceNegative, "El precio no puede ser negativo.");
        if (price > 9999999.99m)
            return Result.Failure(ErrorCodes.ProductPriceTooHigh, "El precio no puede exceder 9,999,999.99.");
        return Result.Success();
    }
}
