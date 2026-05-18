using Taller_Mecanico_Users.Domain.Interfaces;
namespace Taller_Mecanico_Users.Infrastructure.Security
{
    /// <summary>
    /// Implementación de IPasswordHasher usando BCrypt.
    /// Encapsula los detalles de la librería BCrypt.Net.
    /// Si en el futuro se requiere cambiar a Argon2, solo se modifica esta clase.
    /// </summary>
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}

