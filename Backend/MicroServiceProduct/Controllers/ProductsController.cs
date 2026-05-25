using Microsoft.AspNetCore.Mvc;
using MicroServiceProduct.Application.Services;
using MicroServiceProduct.Application.DTOs;

namespace MicroServiceProduct.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _svc;

    public ProductsController(ProductService svc)
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

public record CreateProductRequest(string Name, string? Description, decimal Price);
public record UpdateProductRequest(string Name, string? Description, decimal Price);
