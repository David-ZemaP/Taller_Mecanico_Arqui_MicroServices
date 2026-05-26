using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Taller_Mecanico_Users.Application.UseCases.Users;
using Taller_Mecanico_Users.Application.UseCases.Auth;
using Taller_Mecanico_Users.Domain.Interfaces;

namespace Taller_Mecanico_Users.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly CreateUserUseCase _createUserUseCase;
        private readonly GetUserByIdUseCase _getUserByIdUseCase;
        private readonly GetUsersUseCase _getUsersUseCase;
        
        private readonly UpdateUserUseCase _updateUserUseCase;
        private readonly ChangePasswordUseCase _changePasswordUseCase;
        private readonly ResetPasswordUseCase _resetPasswordUseCase;
        private readonly DeleteUserUseCase _deleteUserUseCase;
        private readonly IRolRepository _rolRepository;
        private readonly IUsuarioLoginRepository _usuarioLoginRepository;
        private readonly IPasswordHasher _passwordHasher;

        public UsersController(
            CreateUserUseCase createUserUseCase,
            GetUserByIdUseCase getUserByIdUseCase,
            GetUsersUseCase getUsersUseCase,
            
            UpdateUserUseCase updateUserUseCase,
            ChangePasswordUseCase changePasswordUseCase,
            ResetPasswordUseCase resetPasswordUseCase,
            DeleteUserUseCase deleteUserUseCase,
            IRolRepository rolRepository,
            IUsuarioLoginRepository usuarioLoginRepository,
            IPasswordHasher passwordHasher)
        {
            _createUserUseCase = createUserUseCase;
            _getUserByIdUseCase = getUserByIdUseCase;
            _getUsersUseCase = getUsersUseCase;
            _updateUserUseCase = updateUserUseCase;
            _changePasswordUseCase = changePasswordUseCase;
            _resetPasswordUseCase = resetPasswordUseCase;
            _deleteUserUseCase = deleteUserUseCase;
            _rolRepository = rolRepository;
            _usuarioLoginRepository = usuarioLoginRepository;
            _passwordHasher = passwordHasher;
        }

        [HttpPost]
        [Authorize(Roles = "Gerente,Administrador,Mecanico")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var usuarioResult = await _createUserUseCase.ExecuteAsync(request.Email, request.Password);
            if (usuarioResult.IsFailure || usuarioResult.Value == null)
            {
                return ApiResultMapper.MapError(this, usuarioResult);
            }

            var creation = usuarioResult.Value;
            return CreatedAtAction(nameof(GetUserById), new { id = creation.User.UsuarioLoginId },
                new
                {
                    creation.User.UsuarioLoginId,
                    creation.User.Email,
                    plainPassword = creation.PlainPassword,
                    notificationRecipients = creation.NotificationRecipients
                });
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Gerente,Administrador,Mecanico")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var result = await _getUserByIdUseCase.ExecuteAsync(id);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            var usuario = result.Value;
            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            return Ok(ToDto(usuario));
        }

        [HttpGet]
        [Authorize(Roles = "Gerente,Administrador,Mecanico")]
        public async Task<IActionResult> GetUsers()
        {
            var usuarios = await _getUsersUseCase.ExecuteAsync();
            return Ok(usuarios.Select(ToDto));
        }

        

        [HttpPut("{id}")]
        [Authorize(Roles = "Gerente,Administrador,Mecanico")] 
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
        {
            var result = await _updateUserUseCase.ExecuteAsync(id, request.Email, request.Activo);

            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return NoContent(); 
        }

        [HttpPost("{id}/change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
        {
            if (!CanChangeOwnPassword(id))
            {
                return Forbid();
            }

            var result = await _changePasswordUseCase.ExecuteAsync(id, request.CurrentPassword, request.NewPassword, request.ConfirmPassword);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return NoContent();
        }

        [HttpPost("{id}/verify-current-password")]
        [Authorize]
        public async Task<IActionResult> VerifyCurrentPassword(int id, [FromBody] VerifyCurrentPasswordRequest request)
        {
            if (!CanChangeOwnPassword(id))
            {
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                return BadRequest(new { message = "La contraseña actual es obligatoria." });
            }

            var userResult = await _usuarioLoginRepository.GetByIdAsync(id);
            if (userResult.IsFailure)
            {
                return ApiResultMapper.MapError(this, userResult);
            }

            var user = userResult.Value;
            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest(new { message = "La contraseña actual es incorrecta." });
            }

            return Ok(new { valid = true });
        }

        [HttpPost("{id}/reset-password")]
        [Authorize(Roles = "Gerente,Administrador,Mecanico")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var result = await _resetPasswordUseCase.ExecuteAsync(id);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return Ok(new { plainPassword = result.Value });
        }

        [HttpPut("{id}/rol")]
        [Authorize(Roles = "Gerente,Administrador,Mecanico")]
        public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateRoleRequest request)
        {
            // Verificar que el usuario actual tenga permisos (Gerente o Completo)
            // Temporal: verificar también Parcial hasta debuguear el claim
            var currentNivelAcceso = User.FindFirst("NivelAcceso")?.Value;
            if (currentNivelAcceso != "Gerente" && currentNivelAcceso != "Completo" && currentNivelAcceso != "Parcial")
            {
                return Forbid();
            }

            // Obtener el rol por nombre
            var rol = await _rolRepository.GetByNombreAsync(request.RolNombre);
            if (rol == null)
            {
                return BadRequest(new { message = "Rol no válido. Roles válidos: Gerente, Administrador, Mecanico, Cliente" });
            }

            // Obtener el usuario
            var usuarioResult = await _usuarioLoginRepository.GetByIdAsync(id);
            if (usuarioResult.IsFailure || usuarioResult.Value == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            var usuario = usuarioResult.Value;
            
            // Nota: validación de clientes/empleados eliminada (microservicio independiente)

            // Asignar el rol y actualizar
            usuario.AsignarRol(rol);
            usuario.RegistrarActualizacion(User.FindFirst(ClaimTypes.Email)?.Value);
            var result = await _usuarioLoginRepository.UpdateAsync(usuario);
            
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return Ok(new { message = "Rol actualizado correctamente", rolId = rol.RolId, rolNombre = rol.Nombre });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Gerente,Administrador,Mecanico")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _deleteUserUseCase.ExecuteAsync(id);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return NoContent();
        }

        private bool CanChangeOwnPassword(int usuarioLoginId)
        {
            if (User.IsInRole("Empleado"))
            {
                return true;
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(currentUserId, out var currentId) && currentId == usuarioLoginId;
        }

        private static UserDto ToDto(Taller_Mecanico_Users.Domain.Entities.UsuarioLogin usuario)
        {
            // Mapear Rol a NivelAcceso para el frontend
            string nivelAcceso;
            if (usuario.Rol != null)
            {
                nivelAcceso = usuario.Rol.Nombre switch
                {
                    "Gerente" => "Gerente",
                    "Administrador" => "Completo",
                    "Mecanico" => "Parcial",
                    "Cliente" => "Cliente",
                    _ => "Parcial"
                };
            }
            else
            {
                nivelAcceso = usuario.NivelAcceso ?? "Parcial";
            }

            return new UserDto
            {
                UsuarioLoginId = usuario.UsuarioLoginId,
                
                Email = usuario.Email,
                UltimoAcceso = usuario.UltimoAcceso,
                Activo = usuario.Activo,
                RequiereCambioPassword = usuario.RequiereCambioPassword,
                
                NivelAcceso = nivelAcceso
            };
        }
    }

    public class CreateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
    }

    public class UpdateUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }

        public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

        public class VerifyCurrentPasswordRequest
        {
            public string CurrentPassword { get; set; } = string.Empty;
        }

    public class UpdateRoleRequest
    {
        public string RolNombre { get; set; } = string.Empty;
    }

    public class UserDto
    {
        public int UsuarioLoginId { get; set; }
        public string Email { get; set; } = string.Empty;
        public DateTime? UltimoAcceso { get; set; }
        public bool Activo { get; set; }
        public bool RequiereCambioPassword { get; set; }
        public string? NivelAcceso { get; set; }
    }
}
