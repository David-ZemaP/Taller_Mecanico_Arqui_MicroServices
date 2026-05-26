namespace MicroServiceProduct.Application.Common;

/// <summary>
/// Mapea códigos de error a mensajes descriptivos en español.
/// </summary>
public static class ErrorMessages
{
    private static readonly Dictionary<string, string> Messages = new()
    {
        // Errores Generales
        { ErrorCodes.ValidationRequired, "Los datos requeridos no fueron proporcionados." },
        { ErrorCodes.ValidationInvalidValue, "El valor proporcionado no es válido." },
        { ErrorCodes.NotFound, "El recurso no fue encontrado." },
        { ErrorCodes.DbError, "Error en la base de datos. Por favor, intente más tarde." },
        
        // Errores de Producto
        { ErrorCodes.ProductNotFound, "El producto no fue encontrado." },
        { ErrorCodes.ProductNameRequired, "El nombre del producto es requerido." },
        { ErrorCodes.ProductNameTooShort, "El nombre debe tener al menos 3 caracteres." },
        { ErrorCodes.ProductNameTooLong, "El nombre no puede exceder 200 caracteres." },
        { ErrorCodes.ProductDescriptionTooLong, "La descripción no puede exceder 1000 caracteres." },
        { ErrorCodes.ProductPriceNegative, "El precio no puede ser negativo." },
        { ErrorCodes.ProductPriceTooHigh, "El precio no puede exceder 9,999,999.99." },
        { ErrorCodes.ProductInvalidId, "El ID del producto no es válido." },
    };

    public static string GetMessage(string errorCode)
    {
        return Messages.TryGetValue(errorCode, out var message) 
            ? message 
            : "Ocurrió un error en la operación.";
    }

    public static int GetStatusCode(string errorCode)
    {
        return errorCode switch
        {
            ErrorCodes.ProductNotFound or ErrorCodes.NotFound => 404,
            ErrorCodes.ValidationRequired or ErrorCodes.ValidationInvalidValue or
            ErrorCodes.ProductNameTooShort or ErrorCodes.ProductNameTooLong or
            ErrorCodes.ProductNameRequired or ErrorCodes.ProductDescriptionTooLong or
            ErrorCodes.ProductPriceNegative or ErrorCodes.ProductPriceTooHigh or
            ErrorCodes.ProductInvalidId => 400,
            ErrorCodes.DbError => 500,
            _ => 400
        };
    }
}
