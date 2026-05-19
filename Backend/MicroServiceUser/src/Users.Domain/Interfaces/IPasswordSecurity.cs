using Taller_Mecanico_Users.Domain.Common;

namespace Taller_Mecanico_Users.Domain.Interfaces
{
    public interface IPasswordSecurity
    {
        Result ValidatePassword(string? password);
        string GenerateSecurePassword(int length = 12);
    }
}
