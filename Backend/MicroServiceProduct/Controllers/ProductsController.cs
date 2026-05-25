using Microsoft.AspNetCore.Mvc;
using MicroServiceProduct.Application.Services;
using System.ComponentModel.DataAnnotations;
using MicroServiceProduct.Application.DTOs;

namespace MicroServiceProduct.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _svc;

    public ProductsController(IProductService svc)
    {
        _svc = svc;
    }

    [HttpGet]
    public async Task<IEnumerable<ProductDto>> GetAll() => await _svc.GetAllAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> Get(Guid id)
    {
        var p = await _svc.GetByIdAsync(id);
        if (p is null) return NotFound();
        return Ok(p);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductRequest req)
    {
        var p = await _svc.CreateAsync(req.Name, req.Description, req.Price);
        return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest req)
    {
        var ok = await _svc.UpdateAsync(id, req.Name, req.Description, req.Price);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _svc.DeleteAsync(id);
        return ok ? NoContent() : NotFound();
    }
}

public record CreateProductRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(0, 9999999.99)] decimal Price);

public record UpdateProductRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(0, 9999999.99)] decimal Price);
