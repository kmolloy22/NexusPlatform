namespace Nexus.CustomerOrder.Application.Features.Catalog.Models;

/// <summary>
/// Data transfer object for product information
/// </summary>
public record GetProductDto(
    string Id,
    string Sku,
    string Name,
    string? Description,
    double BasePrice,
    string Category,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? ModifiedAt
);