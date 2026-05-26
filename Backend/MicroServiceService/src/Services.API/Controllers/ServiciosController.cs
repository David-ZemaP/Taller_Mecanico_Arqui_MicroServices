using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Taller_Mecanico_Services.Application.DTOs;
using Taller_Mecanico_Services.Application.UseCases.Servicios;

namespace Taller_Mecanico_Services.API.Controllers
{
    [ApiController]
    [Route("api/servicios")]
    [Authorize]
    public class ServiciosController : ControllerBase
    {
        private readonly CreateServicioUseCase _createUseCase;
        private readonly UpdateServicioUseCase _updateUseCase;
        private readonly GetServiciosUseCase _getUseCase;
        private readonly GetServicioByIdUseCase _getByIdUseCase;
        private readonly DeleteServicioUseCase _deleteUseCase;

        public ServiciosController(
            CreateServicioUseCase createUseCase,
            UpdateServicioUseCase updateUseCase,
            GetServiciosUseCase getUseCase,
            GetServicioByIdUseCase getByIdUseCase,
            DeleteServicioUseCase deleteUseCase)
        {
            _createUseCase = createUseCase;
            _updateUseCase = updateUseCase;
            _getUseCase = getUseCase;
            _getByIdUseCase = getByIdUseCase;
            _deleteUseCase = deleteUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ServicioCreateDTO dto)
        {
            var result = await _createUseCase.ExecuteAsync(dto);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ServicioUpdateDTO dto)
        {
            var result = await _updateUseCase.ExecuteAsync(id, dto);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int? categoriaId = null,
            [FromQuery] bool? estado = null,
            [FromQuery] string? nombre = null,
            [FromQuery] string? ordenarPor = null)
        {
            var list = await _getUseCase.ExecuteAsync(categoriaId, estado, nombre, ordenarPor);
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _getByIdUseCase.ExecuteAsync(id);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return Ok(result.Value);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _deleteUseCase.ExecuteAsync(id);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return NoContent();
        }
    }
}
