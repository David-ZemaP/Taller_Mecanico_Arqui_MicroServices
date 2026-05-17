using Microsoft.AspNetCore.Builder;

namespace Taller_Mecanico_Users.API.Middleware
{
    public static class RequirePasswordChangeMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequirePasswordChange(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequirePasswordChangeMiddleware>();
        }
    }
}
