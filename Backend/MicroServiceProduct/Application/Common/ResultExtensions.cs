namespace MicroServiceProduct.Application.Common;

/// <summary>
/// Extensiones útiles para trabajar con el patrón Result.
/// Proporciona métodos de conveniencia para encadenamiento y mapeo.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Ejecuta una acción si el resultado es exitoso.
    /// </summary>
    public static Result OnSuccess(this Result result, Action action)
    {
        if (result.IsSuccess)
            action();
        return result;
    }

    /// <summary>
    /// Ejecuta una acción si el resultado es exitoso con valor.
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess && result.Value != null)
            action(result.Value);
        return result;
    }

    /// <summary>
    /// Ejecuta una acción si el resultado es fallido.
    /// </summary>
    public static Result OnFailure(this Result result, Action<string, string> action)
    {
        if (result.IsFailure)
            action(result.ErrorCode ?? "", result.ErrorMessage ?? "");
        return result;
    }

    /// <summary>
    /// Ejecuta una acción si el resultado es fallido con valor.
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<string, string> action)
    {
        if (result.IsFailure)
            action(result.ErrorCode ?? "", result.ErrorMessage ?? "");
        return result;
    }

    /// <summary>
    /// Mapea un Result<T> a Result<TOut> usando una función transformadora.
    /// </summary>
    public static Result<TOut> Map<T, TOut>(this Result<T> result, Func<T?, TOut> mapper)
    {
        if (result.IsFailure)
            return Result<TOut>.Failure(result.ErrorCode!, result.ErrorMessage!);

        try
        {
            var mappedValue = mapper(result.Value);
            return Result<TOut>.Success(mappedValue);
        }
        catch (Exception ex)
        {
            return Result<TOut>.Failure(ErrorCodes.ValidationInvalidValue, $"Error al mapear valor: {ex.Message}");
        }
    }

    /// <summary>
    /// Encadena dos operaciones Result.
    /// </summary>
    public static async Task<Result<TOut>> Bind<T, TOut>(
        this Task<Result<T>> resultTask,
        Func<T, Task<Result<TOut>>> binder)
    {
        var result = await resultTask;
        if (result.IsFailure)
            return Result<TOut>.Failure(result.ErrorCode!, result.ErrorMessage!);

        return await binder(result.Value!);
    }

    /// <summary>
    /// Obtiene el valor o null si hay error.
    /// </summary>
    public static T? GetValueOrNull<T>(this Result<T> result)
    {
        return result.IsSuccess ? result.Value : default;
    }

    /// <summary>
    /// Obtiene el valor o lanza una excepción si hay error.
    /// </summary>
    public static T GetValueOrThrow<T>(this Result<T> result)
    {
        if (result.IsFailure)
            throw new InvalidOperationException($"Resultado fallido - Código: {result.ErrorCode}, Mensaje: {result.ErrorMessage}");

        return result.Value ?? throw new InvalidOperationException("El resultado es exitoso pero el valor es null");
    }

    /// <summary>
    /// Convierte el resultado a un diccionario para serialización JSON.
    /// </summary>
    public static Dictionary<string, object?> ToDict(this Result result)
    {
        if (result.IsSuccess)
            return new Dictionary<string, object?> { { "success", true } };

        return new Dictionary<string, object?>
        {
            { "success", false },
            { "code", result.ErrorCode },
            { "message", result.ErrorMessage }
        };
    }

    /// <summary>
    /// Convierte el resultado genérico a un diccionario para serialización JSON.
    /// </summary>
    public static Dictionary<string, object?> ToDict<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new Dictionary<string, object?>
            {
                { "success", true },
                { "data", result.Value }
            };

        return new Dictionary<string, object?>
        {
            { "success", false },
            { "code", result.ErrorCode },
            { "message", result.ErrorMessage }
        };
    }
}
