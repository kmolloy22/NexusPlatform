namespace Nexus.CustomerOrder.Application.Features.Catalog.Models;

/// <summary>
/// Response model for product creation operations
/// </summary>
public sealed record CreateProductResponseDto(
    /// <summary>
    /// The unique identifier of the created product
    /// </summary>
    string Id,

    /// <summary>
    /// The URI location of the created product resource
    /// </summary>
    string Location,

    /// <summary>
    /// Timestamp when the product was created (UTC)
    /// </summary>
    DateTimeOffset CreatedAt
);