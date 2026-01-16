using Nexus.CustomerOrder.Application.Features.Catalog.Models;
using Nexus.CustomerOrder.Application.Shared.Results;
using Refit;

namespace Nexus.Web.Services;

/// <summary>
/// Type-safe HTTP client for Products API using Refit
/// </summary>
public interface IProductsApiClient
{
    [Get("/api/products")]
    Task<PagedResult<GetProductDto>> GetProductsAsync(
        [Query] int? take = null,
        [Query] string? category = null,
        [Query] bool? isActive = null,
        [Query] string? continuationToken = null,
        CancellationToken cancellationToken = default);

    [Get("/api/products/{id}")]
    Task<GetProductDto> GetProductAsync(
        string id,
        CancellationToken cancellationToken = default);

    [Post("/api/products")]
    Task<CreateProductResponseDto> CreateProductAsync(
        [Body] CreateProductDto product,
        CancellationToken cancellationToken = default);

    [Put("/api/products/{id}")]
    Task UpdateProductAsync(
        string id,
        [Body] UpdateProductDto product,
        CancellationToken cancellationToken = default);

    [Delete("/api/products/{id}")]
    Task DeleteProductAsync(
        string id,
        CancellationToken cancellationToken = default);
}