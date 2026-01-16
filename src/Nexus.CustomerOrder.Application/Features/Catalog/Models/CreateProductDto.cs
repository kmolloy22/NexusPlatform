namespace Nexus.CustomerOrder.Application.Features.Catalog.Models;

/// <summary>
/// Data transfer object for creating a new product
/// </summary>
public record CreateProductDto(
    string Sku,
    string Name,
    string? Description,
    double BasePrice,
    string Category,
    bool IsActive = true
);