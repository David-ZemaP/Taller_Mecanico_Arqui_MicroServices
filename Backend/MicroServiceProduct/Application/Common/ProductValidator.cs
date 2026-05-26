namespace MicroServiceProduct.Application.Common;

/// <summary>
/// Clase auxiliar para validaciones del dominio de Productos.
/// Retorna Result para mantener consistencia con el patrón.
/// </summary>
public static class ProductValidator
{
    private const int MinNameLength = 3;
    private const int MaxNameLength = 200;
    private const int MaxDescriptionLength = 1000;
    private const decimal MinPrice = 0;
    private const decimal MaxPrice = 9999999.99m;

    public static Result ValidateCreateProduct(string? name, string? description, decimal price)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(ErrorCodes.ProductNameRequired, ErrorMessages.GetMessage(ErrorCodes.ProductNameRequired));

        if (name.Length < MinNameLength)
            return Result.Failure(ErrorCodes.ProductNameTooShort, ErrorMessages.GetMessage(ErrorCodes.ProductNameTooShort));

        if (name.Length > MaxNameLength)
            return Result.Failure(ErrorCodes.ProductNameTooLong, ErrorMessages.GetMessage(ErrorCodes.ProductNameTooLong));

        if (!string.IsNullOrEmpty(description) && description.Length > MaxDescriptionLength)
            return Result.Failure(ErrorCodes.ProductDescriptionTooLong, ErrorMessages.GetMessage(ErrorCodes.ProductDescriptionTooLong));

        if (price < MinPrice)
            return Result.Failure(ErrorCodes.ProductPriceNegative, ErrorMessages.GetMessage(ErrorCodes.ProductPriceNegative));

        if (price > MaxPrice)
            return Result.Failure(ErrorCodes.ProductPriceTooHigh, ErrorMessages.GetMessage(ErrorCodes.ProductPriceTooHigh));

        return Result.Success();
    }

    public static Result ValidateUpdateProduct(string? name, string? description, decimal price)
    {
        return ValidateCreateProduct(name, description, price);
    }

    public static Result ValidateProductId(Guid id)
    {
        if (id == Guid.Empty)
            return Result.Failure(ErrorCodes.ProductInvalidId, ErrorMessages.GetMessage(ErrorCodes.ProductInvalidId));

        return Result.Success();
    }
}
