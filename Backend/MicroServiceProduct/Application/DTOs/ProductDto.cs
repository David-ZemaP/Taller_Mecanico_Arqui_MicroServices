namespace MicroServiceProduct.Application.DTOs;

public record ProductDto(Guid Id, string Name, string? Description, decimal Price, DateTime CreatedAt);
