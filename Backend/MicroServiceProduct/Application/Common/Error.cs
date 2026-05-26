namespace MicroServiceProduct.Application.Common;

/// <summary>
/// Códigos de error estandarizados para el microservicio de Productos.
/// Cada código mapea a un mensaje descriptivo en español.
/// </summary>
public static class ErrorCodes
{
    // Errores Generales
    public const string ValidationRequired = "VALIDATION_REQUIRED";
    public const string ValidationInvalidValue = "VALIDATION_INVALID_VALUE";
    public const string NotFound = "NOT_FOUND";
    public const string DbError = "DB_ERROR";
    
    // Errores de Producto
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";
    public const string ProductNameRequired = "PRODUCT_NAME_REQUIRED";
    public const string ProductNameTooShort = "PRODUCT_NAME_TOO_SHORT";
    public const string ProductNameTooLong = "PRODUCT_NAME_TOO_LONG";
    public const string ProductDescriptionTooLong = "PRODUCT_DESCRIPTION_TOO_LONG";
    public const string ProductPriceNegative = "PRODUCT_PRICE_NEGATIVE";
    public const string ProductPriceTooHigh = "PRODUCT_PRICE_TOO_HIGH";
    public const string ProductInvalidId = "PRODUCT_INVALID_ID";
}
