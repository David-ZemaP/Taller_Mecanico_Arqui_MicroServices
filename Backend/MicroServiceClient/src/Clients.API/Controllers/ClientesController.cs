using Microsoft.AspNetCore.Mvc;
using Taller_Mecanico_Clientes.Application.UseCases.Clientes;
using Taller_Mecanico_Clientes.Domain.Entities;

namespace Taller_Mecanico_Clientes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        public async Task<IActionResult> Create([FromBody] Cliente cliente)
        {
            var result = await _createClienteUseCase.ExecuteAsync(cliente);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }
            return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id }, result.Value);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] Cliente cliente)
        {
            var result = await _updateClienteUseCase.ExecuteAsync(id, cliente);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }
            return Ok(result.Value);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _deleteClienteUseCase.ExecuteAsync(id);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }
            return NoContent();
        }
    }
}
