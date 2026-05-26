using WebService.DTOs;

namespace WebService.Adapters;

public interface IEmpleadosAdapter : IAdapter
{
    Task<(bool ok, IEnumerable<EmpleadoDto>? empleados, string? error)> GetAllEmpleadosAsync();
    Task<(bool ok, EmpleadoDto? empleado, string? error)> GetEmpleadoByIdAsync(int id);
    Task<(bool ok, int? empleadoId, string? error)> CrearEmpleadoAsync(EmpleadoFormDto form);
    Task<(bool ok, string? error)> ActualizarEmpleadoAsync(int id, EmpleadoFormDto form);
    Task<(bool ok, string? error)> EliminarEmpleadoAsync(int id);
    Task<(bool ok, IEnumerable<UsuarioDto>? usuarios, string? error)> GetAllUsuariosAsync();
    Task<(bool ok, UsuarioDto? usuario, string? error)> GetUsuarioByIdAsync(int id);
    Task<(bool ok, UsuarioDto? usuario, string? error)> GetUsuarioByEmpleadoIdAsync(int empleadoId);
    Task<(bool ok, string? plainPassword, IReadOnlyList<string>? notificationRecipients, string? error)> CreateUsuarioAsync(int empleadoId, string email, string? password);
    Task<(bool ok, string? error)> UpdateUsuarioAsync(int id, string email, bool activo);
    Task<(bool ok, string? plainPassword, string? error)> ResetPasswordAsync(int id);
    Task<(bool ok, string? error)> UpdateUsuarioRolAsync(int usuarioId, string rolNombre);
}
