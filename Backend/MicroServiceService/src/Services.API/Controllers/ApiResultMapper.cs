using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Services.Domain.Common;

namespace Taller_Mecanico_Services.API.Controllers
{
    internal static class ApiResultMapper
    {
        public static IActionResult MapError(ControllerBase controller, Result result)
        {
            var message = result.ErrorMessage ?? "Ocurrió un error en la operación.";

            return result.ErrorCode switch
            {
                ErrorCodes.CategoriaNotFound or ErrorCodes.ServicioNotFound or ErrorCodes.NotFound
                    => controller.NotFound(new { code = result.ErrorCode, message }),

                ErrorCodes.CategoriaNombreDuplicado or ErrorCodes.ServicioNombreDuplicado or ErrorCodes.ValidationDuplicateValue
                    => controller.Conflict(new { code = result.ErrorCode, message }),

                ErrorCodes.ValidationRequired or ErrorCodes.ValidationInvalidValue or ErrorCodes.ServicioPrecioInvalido or ErrorCodes.ServicioDuracionInvalida or ErrorCodes.CategoriaTieneServiciosActivos
                    => controller.BadRequest(new { code = result.ErrorCode, message }),

                ErrorCodes.DbError or ErrorCodes.DbInsertFailed
                    => controller.StatusCode(StatusCodes.Status500InternalServerError, new { code = result.ErrorCode, message }),

                _ => controller.BadRequest(new { code = result.ErrorCode, message })
            };
        }
    }
}
