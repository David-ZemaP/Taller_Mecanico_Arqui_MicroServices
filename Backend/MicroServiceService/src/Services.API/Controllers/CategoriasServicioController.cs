using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Taller_Mecanico_Services.Application.DTOs;
using Taller_Mecanico_Services.Application.UseCases.Categorias;

namespace Taller_Mecanico_Services.API.Controllers
{
    [ApiController]
    [Route("api/categorias-servicio")]
    [Authorize]
    public class CategoriasServicioController : ControllerBase
    {
        private readonly CreateCategoriaUseCase _createUseCase;
        private readonly UpdateCategoriaUseCase _updateUseCase;
        private readonly GetCategoriasUseCase _getUseCase;
        private readonly GetCategoriaByIdUseCase _getByIdUseCase;
        private readonly DeleteCategoriaUseCase _deleteUseCase;

        public CategoriasServicioController(
            CreateCategoriaUseCase createUseCase,
            UpdateCategoriaUseCase updateUseCase,
            GetCategoriasUseCase getUseCase,
            GetCategoriaByIdUseCase getByIdUseCase,
            DeleteCategoriaUseCase deleteUseCase)
        {
            _createUseCase = createUseCase;
            _updateUseCase = updateUseCase;
            _getUseCase = getUseCase;
            _getByIdUseCase = getByIdUseCase;
            _deleteUseCase = deleteUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoriaServicioCreateDTO dto)
        {
            var result = await _createUseCase.ExecuteAsync(dto);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoriaServicioUpdateDTO dto)
        {
            var result = await _updateUseCase.ExecuteAsync(id, dto);
            if (result.IsFailure)
            {
                return ApiResultMapper.MapError(this, result);
            }

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] bool? estado = null, [FromQuery] string? ordenarPor = null)
        {
            var list = await _getUseCase.ExecuteAsync(estado, ordenarPor);
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
