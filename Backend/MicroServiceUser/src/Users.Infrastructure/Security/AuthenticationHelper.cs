using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Taller_Mecanico_Users.Domain.Enums;
using Taller_Mecanico_Users.Domain.Interfaces;

namespace Taller_Mecanico_Users.Infrastructure.Security;

public class AuthenticationHelper : IAuthenticationHelper
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthenticationHelper> _logger;

    public AuthenticationHelper(IHttpContextAccessor httpContextAccessor, ILogger<AuthenticationHelper> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public NivelAcceso? GetCurrentUserAccessLevel()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var nivelAccesoClaim = httpContext.User.FindFirst("NivelAcceso");
        if (nivelAccesoClaim != null && Enum.TryParse<NivelAcceso>(nivelAccesoClaim.Value, out var level))
        {
            return level;
        }

        return null;
    }

    public string GetCurrentAuditActor()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return "sistema";
        }

        var emailClaim = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
        if (!string.IsNullOrWhiteSpace(emailClaim))
        {
            return emailClaim;
        }

        var nameClaim = httpContext.User.FindFirst(ClaimTypes.Name)?.Value;
        return string.IsNullOrWhiteSpace(nameClaim) ? "sistema" : nameClaim;
    }

    public async Task ForceLogoutAsync()
    {
        // Con JWT Bearer no es posible invalidar el token desde el servidor sin revocación.
        _logger.LogWarning("ForceLogoutAsync called but JWT bearer is used; no server-side logout performed.");
        await Task.CompletedTask;
    }

    public async Task UpdateAccessLevelClaimAsync(NivelAcceso newLevel)
    {
        // JWTs son inmutables; para actualizar claims es necesario emitir un nuevo token.
        // Esta implementación registra la petición y no intenta manipular cookies.
        _logger.LogWarning("UpdateAccessLevelClaimAsync called but JWT tokens are immutable. Request a new token to update claims.");
        await Task.CompletedTask;
    }

    public bool CanModifyAdmin(NivelAcceso targetAdminLevel)
    {
        var currentLevel = GetCurrentUserAccessLevel();
        if (currentLevel == null)
        {
            return false;
        }

        return currentLevel.Value switch
        {
            NivelAcceso.Gerente => true,
            NivelAcceso.Completo => targetAdminLevel == NivelAcceso.Parcial,
            NivelAcceso.Parcial => false,
            _ => false
        };
    }

    public bool CanCreateAdmin(NivelAcceso newAdminLevel)
    {
        var currentLevel = GetCurrentUserAccessLevel();
        if (currentLevel == null)
        {
            return false;
        }

        return currentLevel.Value switch
        {
            NivelAcceso.Gerente => newAdminLevel != NivelAcceso.Gerente,
            NivelAcceso.Completo => newAdminLevel == NivelAcceso.Parcial,
            NivelAcceso.Parcial => false,
            _ => false
        };
    }
}
