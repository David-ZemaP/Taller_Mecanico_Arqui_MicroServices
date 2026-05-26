using Microsoft.AspNetCore.Mvc;
using MicroServiceProduct.Application.Common;

namespace MicroServiceProduct.Controllers;

/// <summary>
/// Mapea resultados del patrón Result a respuestas HTTP apropiadas.
/// Similar al ApiResultMapper del microservicio de Servicios.
/// </summary>
internal static class ApiResultMapper
{
    public static IActionResult MapError(ControllerBase controller, Result result)
    {
        var message = result.ErrorMessage ?? ErrorMessages.GetMessage(result.ErrorCode ?? "");
        var statusCode = ErrorMessages.GetStatusCode(result.ErrorCode ?? "");

        return statusCode switch
        {
            404 => controller.NotFound(new { code = result.ErrorCode, message }),
            400 => controller.BadRequest(new { code = result.ErrorCode, message }),
            409 => controller.Conflict(new { code = result.ErrorCode, message }),
            500 => controller.StatusCode(StatusCodes.Status500InternalServerError, new { code = result.ErrorCode, message }),
            _ => controller.BadRequest(new { code = result.ErrorCode, message })
        };
    }

    public static IActionResult MapError<T>(ControllerBase controller, Result<T> result)
    {
        return MapError(controller, (Result)result);
    }
}
