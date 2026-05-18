using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Application.Interfaces;

namespace Taller_Mecanico_Users.Infrastructure.Security
{
    /// <summary>
    /// Implementación concreta de IJwtTokenGenerator.
    /// Encapsula la lógica de creación de JWT tokens.
    /// </summary>
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IJwtSettings _jwtSettings;

        public JwtTokenGenerator(IJwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }

        public string GenerateToken(UsuarioLogin usuario)
        {
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            // Obtener nivel de acceso desde Rol
            string nivelAcceso = usuario.Rol?.Nombre switch
            {
                "Gerente" => "Gerente",
                "Administrador" => "Completo",
                "Mecanico" => "Parcial",
                _ => usuario.NivelAcceso ?? "Parcial"
            };

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioLoginId.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim("RequiereCambio", usuario.RequiereCambioPassword.ToString()),
                new Claim("NivelAcceso", nivelAcceso)
            };

            if (usuario.RolId.HasValue)
            {
                claims.Add(new Claim("RolId", usuario.RolId.Value.ToString()));
            }

            // Añadir claim de rol para que la autorización basada en Roles funcione (Authorize(Roles = "...")).
            if (!string.IsNullOrWhiteSpace(usuario.Rol?.Nombre))
            {
                claims.Add(new Claim(ClaimTypes.Role, usuario.Rol.Nombre));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationInMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

