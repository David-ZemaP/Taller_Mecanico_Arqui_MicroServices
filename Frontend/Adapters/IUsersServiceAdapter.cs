using WebService.DTOs;

namespace WebService.Adapters;

public interface IUsersServiceAdapter : IAdapter
{
    Task<(bool ok, UsersLoginResponseDto? response, string? error)> LoginAsync(string email, string password);
    Task<(bool ok, string? error)> VerifyCurrentPasswordAsync(int usuarioLoginId, string currentPassword);
    Task<(bool ok, string? error)> ChangePasswordAsync(int usuarioLoginId, string currentPassword, string newPassword, string confirmPassword);
}
