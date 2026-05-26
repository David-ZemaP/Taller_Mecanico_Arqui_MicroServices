using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Taller_Mecanico_Clientes.Application.UseCases.Clientes;
using Taller_Mecanico_Clientes.Domain.Entities;

namespace Taller_Mecanico_Clientes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly GetClientesUseCase _getClientesUseCase;
        private readonly GetClienteByIdUseCase _getClienteByIdUseCase;
        private readonly CreateClienteUseCase _createClienteUseCase;
        private readonly UpdateClienteUseCase _updateClienteUseCase;
        private readonly DeleteClienteUseCase _deleteClienteUseCase;

        public ClientesController(
            GetClientesUseCase getClientesUseCase,
            GetClienteByIdUseCase getClienteByIdUseCase,
            CreateClienteUseCase createClienteUseCase,
            UpdateClienteUseCase updateClienteUseCase,
            DeleteClienteUseCase deleteClienteUseCase)
        {
            _getClientesUseCase = getClientesUseCase;
            _getClienteByIdUseCase = getClienteByIdUseCase;
            _createClienteUseCase = createClienteUseCase;
            _updateClienteUseCase = updateClienteUseCase;
            _deleteClienteUseCase = deleteClienteUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _getClientesUseCase.ExecuteAsync();
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }
            return Ok(result.Value);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _getClienteByIdUseCase.ExecuteAsync(id);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }
            if (result.Value == null)
            {
                return NotFound(new { message = "Cliente no encontrado." });
            }
            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClienteRequest req)
        {
            var result = await _createClienteUseCase.ExecuteAsync(
                req.Nombre, req.PrimerApellido, req.SegundoApellido,
                req.Ci, req.CiComplemento, req.Telefono, req.Email,
                req.TipoCliente);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateClienteRequest req)
        {
            var result = await _updateClienteUseCase.ExecuteAsync(
                id, req.Nombre, req.PrimerApellido, req.SegundoApellido,
                req.Telefono, req.Email, req.UsuarioLoginId, req.TipoCliente);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }
            return Ok(result.Value);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var currentUser = GetCurrentUser();
            var result = await _deleteClienteUseCase.ExecuteAsync(id, currentUser);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }
            return NoContent();
        }

        private string? GetCurrentUser()
            => User.FindFirst(ClaimTypes.Email)?.Value
               ?? User.FindFirst("email")?.Value
               ?? User.Identity?.Name;
    }

    public record CreateClienteRequest(
        [Required, MaxLength(100)] string Nombre,
        [Required, MaxLength(100)] string PrimerApellido,
        [MaxLength(100)] string? SegundoApellido,
        [Range(1, int.MaxValue)] int Ci,
        string? CiComplemento,
        [Range(1, int.MaxValue)] int Telefono,
        [Required, MaxLength(200)] string Email,
        string? TipoCliente);

    public record UpdateClienteRequest(
        [Required, MaxLength(100)] string Nombre,
        [Required, MaxLength(100)] string PrimerApellido,
        [MaxLength(100)] string? SegundoApellido,
        [Range(1, int.MaxValue)] int Telefono,
        [Required, MaxLength(200)] string Email,
        int? UsuarioLoginId,
        string? TipoCliente);
}
