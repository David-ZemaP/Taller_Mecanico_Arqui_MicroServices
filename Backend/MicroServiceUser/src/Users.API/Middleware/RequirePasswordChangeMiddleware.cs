using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Taller_Mecanico_Users.API.Middleware
{
    public sealed class RequirePasswordChangeMiddleware
    {
        private readonly RequestDelegate _next;

        public RequirePasswordChangeMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            ILogger<RequirePasswordChangeMiddleware> logger)
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
                var isLoginPath = path.StartsWith("/api/auth/login");
                var isChangePasswordPath = path.StartsWith("/api/users/") && path.Contains("/change-password");
                var isPasswordVerificationPath = path.StartsWith("/api/users/") && path.Contains("/verify-current-password");

                if (!isLoginPath && !isChangePasswordPath && !isPasswordVerificationPath)
                {
                    var claimValue = context.User.FindFirst("RequiereCambio")?.Value;
                    if (bool.TryParse(claimValue, out var requiresPasswordChange) && requiresPasswordChange)
                    {
                        logger.LogInformation("Bloqueando acceso hasta que el usuario cambie su contraseña.");

                        context.Response.StatusCode = StatusCodes.Status428PreconditionRequired;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            code = "PASSWORD_CHANGE_REQUIRED",
                            message = "Debe cambiar su contraseña antes de continuar."
                        });
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
