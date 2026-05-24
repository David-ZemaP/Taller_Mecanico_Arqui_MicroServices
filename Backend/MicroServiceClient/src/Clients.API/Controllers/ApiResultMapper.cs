using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Clientes.Domain.Common;

namespace Taller_Mecanico_Clientes.API.Controllers
{
    public static class ApiResultMapper
    {
        public static IActionResult MapError(ControllerBase controller, Result result)
        {
            if (result.IsSuccess)
            {
                throw new InvalidOperationException("No se puede mapear un resultado exitoso como un error.");
            }

            return result.ErrorCode switch
            {
                ErrorCodes.NotFound or ErrorCodes.ClienteNotFound => controller.NotFound(new { message = result.ErrorMessage }),
                ErrorCodes.ValidationRequired or ErrorCodes.ValidationInvalidValue => controller.BadRequest(new { message = result.ErrorMessage }),
                _ => controller.StatusCode(500, new { message = result.ErrorMessage ?? "Ha ocurrido un error inesperado." })
            };
        }
    }
}
