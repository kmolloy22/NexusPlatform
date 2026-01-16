using Nexus.CustomerOrder.Application.Features.Catalog.Infrastructure.StorageAccount;
using Nexus.CustomerOrder.Application.Shared.Results;
using Nexus.CustomerOrder.Domain.Features.Catalog;

namespace Nexus.CustomerOrder.Application.Features.Catalog.Ports;

/// <summary>
/// Repository interface for product data access
/// </summary>
public interface IProductRepository
{
    /// <summary>
    /// Adds a new product to the catalog
    /// </summary>
    Task AddAsync(Product product, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by its unique identifier
    /// </summary>
    Task<ProductTableEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by its SKU
    /// </summary>
    Task<ProductTableEntity?> GetBySkuAsync(string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries products with pagination support
    /// </summary>
    Task<PagedResult<ProductTableEntity>> QueryAsync(
        int pageSize,
        string? category = null,
        bool? isActive = null,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product
    /// </summary>
    Task<bool> UpdateAsync(
        string id,
        string name,
        string? description,
        double basePrice,
        string category,
        bool isActive,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a product
    /// </summary>
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}