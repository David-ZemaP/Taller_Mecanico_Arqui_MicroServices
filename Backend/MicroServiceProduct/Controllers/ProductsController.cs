using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MicroServiceProduct.Application.Services;
using System.ComponentModel.DataAnnotations;
using MicroServiceProduct.Application.DTOs;
using System.Security.Claims;

namespace MicroServiceProduct.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
        var p = await _svc.CreateAsync(req.Name, req.Description, req.Price, req.Stock, GetCurrentUser());
        return CreatedAtAction(nameof(Get), new { id = p.Id }, p);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest req)
    {
        var ok = await _svc.UpdateAsync(id, req.Name, req.Description, req.Price, req.Stock);
        return ok ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _svc.DeleteAsync(id, GetCurrentUser());
        return ok ? NoContent() : NotFound();
    }

    private string? GetCurrentUser()
        => User.FindFirst(ClaimTypes.Email)?.Value
           ?? User.FindFirst("email")?.Value
           ?? User.Identity?.Name;
}

public record CreateProductRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(0, 9999999.99)] decimal Price,
    [Range(0, int.MaxValue)] int Stock);

public record UpdateProductRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Range(0, 9999999.99)] decimal Price,
    [Range(0, int.MaxValue)] int Stock);
