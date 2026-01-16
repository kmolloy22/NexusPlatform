namespace Nexus.CustomerOrder.Application.Features.Catalog.Models;

/// <summary>
/// Data transfer object for updating a product
/// </summary>
public record UpdateProductDto(
    string Name,
    string? Description,
    double BasePrice,
    string Category,
    bool IsActive
);