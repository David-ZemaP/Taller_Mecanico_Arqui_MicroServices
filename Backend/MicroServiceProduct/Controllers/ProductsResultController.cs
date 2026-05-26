using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MicroServiceProduct.Application.Services;
using MicroServiceProduct.Application.Common;
using System.ComponentModel.DataAnnotations;
using MicroServiceProduct.Application.DTOs;

namespace MicroServiceProduct.Controllers;

/// <summary>
/// Controlador de Productos con patrón Result.
/// Este controller demuestra cómo usar el patrón Result para manejo de errores y validaciones.
/// </summary>
[ApiController]
[Route("api/v2/[controller]")]
[Authorize]
public class ProductsResultController : ControllerBase
{
    private readonly IProductResultService _svc;

    public ProductsResultController(IProductResultService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _svc.GetAllAsync();
        if (result.IsFailure)
        {
            return ApiResultMapper.MapError(this, result);
        }

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var result = await _svc.GetByIdAsync(id);
        if (result.IsFailure)
        {
            return ApiResultMapper.MapError(this, result);
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductResultRequest req)
    {
        var result = await _svc.CreateAsync(req.Name, req.Description, req.Price);
        if (result.IsFailure)
        {
            return ApiResultMapper.MapError(this, result);
        }

        return CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductResultRequest req)
    {
        var result = await _svc.UpdateAsync(id, req.Name, req.Description, req.Price);
        if (result.IsFailure)
        {
            return ApiResultMapper.MapError(this, result);
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _svc.DeleteAsync(id);
        if (result.IsFailure)
        {
            return ApiResultMapper.MapError(this, result);
        }

        return NoContent();
    }
}

public record CreateProductResultRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(0, 9999999.99)] decimal Price);

public record UpdateProductResultRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(0, 9999999.99)] decimal Price);
