using Microsoft.Extensions.Logging;
using Nexus.CustomerOrder.Application.Features.Catalog.Ports;
using Nexus.CustomerOrder.Application.Shared.Results;
using Nexus.CustomerOrder.Domain.Features.Catalog;
using Nexus.Infrastructure.StorageAccount.Tables.Client;
using Nexus.Shared.Kernel.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nexus.CustomerOrder.Application.Features.Catalog.Infrastructure.StorageAccount;

internal class ProductRepository : IProductRepository
{
    private readonly ITableClient<ProductsTableStorageConfiguration, ProductTableEntity> _tableClient;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(
        ITableClient<ProductsTableStorageConfiguration, ProductTableEntity> tableClient,
        ILogger<ProductRepository> logger)
    {
        _tableClient = tableClient;
        _logger = logger;
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken = default)
    {
        var entity = new ProductTableEntity
        {
            PartitionKey = NormalizeCategory(product.Category),
            RowKey = product.Id.ToString("N"),
            Sku = product.Sku,
            Name = product.Name,
            Description = product.Description,
            BasePrice = product.BasePrice,
            Category = product.Category,
            IsActive = product.IsActive,
            CreatedUtc = product.CreatedUtc,
            ModifiedUtc = DateTime.UtcNow
        };

        await _tableClient.AddAsync(entity);

        _logger.LogInformation(
            "Product {ProductId} ({Sku}) added to catalog in category {Category}",
            product.Id,
            product.Sku,
            product.Category);
    }

    public async Task<ProductTableEntity?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        if (id.IsMissing())
            return null;

        // We need to scan all categories since we don't know which partition the product is in
        // Alternative: Store a separate lookup table or use partition key in the ID
        _logger.LogDebug("Searching for product {ProductId} across all categories", id);

        var query = _tableClient.QueryAsync(e => e.RowKey == id);

        await foreach (var entity in query.WithCancellation(cancellationToken))
        {
            return entity;
        }

        return null;
    }

    public async Task<ProductTableEntity?> GetBySkuAsync(
        string sku,
        CancellationToken cancellationToken = default)
    {
        if (sku.IsMissing())
            return null;

        _logger.LogDebug("Searching for product with SKU {Sku}", sku);

        var query = _tableClient.QueryAsync(e => e.Sku == sku);

        await foreach (var entity in query.WithCancellation(cancellationToken))
        {
            return entity;
        }

        return null;
    }

    public async Task<PagedResult<ProductTableEntity>> QueryAsync(
        int pageSize,
        string? category = null,
        bool? isActive = null,
        string? continuationToken = null,
        CancellationToken cancellationToken = default)
    {
        var skip = continuationToken.IsMissing()
            ? 0
            : int.TryParse(continuationToken, out var s) ? s : 0;

        _logger.LogDebug(
            "Querying products: pageSize={PageSize}, category={Category}, isActive={IsActive}, skip={Skip}",
            pageSize,
            category ?? "all",
            isActive?.ToString() ?? "all",
            skip);

        // Build query based on filters
        var allEntities = new List<ProductTableEntity>();

        if (!string.IsNullOrWhiteSpace(category))
        {
            // Query specific category partition
            var normalizedCategory = NormalizeCategory(category);
            var query = _tableClient.QueryAsync(
                e => e.PartitionKey == normalizedCategory);

            await foreach (var entity in query.WithCancellation(cancellationToken))
            {
                if (!isActive.HasValue || entity.IsActive == isActive.Value)
                {
                    allEntities.Add(entity);
                }
            }
        }
        else
        {
            // Query all categories
            var query = _tableClient.QueryAsync(e => true);

            await foreach (var entity in query.WithCancellation(cancellationToken))
            {
                if (!isActive.HasValue || entity.IsActive == isActive.Value)
                {
                    allEntities.Add(entity);
                }
            }
        }

        // Sort and page
        var sortedEntities = allEntities
            .OrderBy(e => e.Category)
            .ThenBy(e => e.Name)
            .Skip(skip)
            .Take(pageSize + 1)
            .ToList();

        var hasMore = sortedEntities.Count > pageSize;
        var pagedEntities = sortedEntities.Take(pageSize).ToList();
        var nextToken = hasMore ? (skip + pageSize).ToString() : null;

        _logger.LogInformation(
            "Returning {Count} products, hasMore={HasMore}, nextToken={Token}",
            pagedEntities.Count,
            hasMore,
            nextToken ?? "null");

        return new PagedResult<ProductTableEntity>(pagedEntities, nextToken);
    }

    public async Task<bool> UpdateAsync(
        string id,
        string name,
        string? description,
        double basePrice,
        string category,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return false;

        // Check if category changed (requires moving to different partition)
        var newNormalizedCategory = NormalizeCategory(category);
        var categoryChanged = existing.PartitionKey != newNormalizedCategory;

        if (categoryChanged)
        {
            // Delete old entity
            await _tableClient.DeleteAsync(existing);

            // Create new entity in new partition
            existing.PartitionKey = newNormalizedCategory;
        }

        // Update properties
        existing.Name = name;
        existing.Description = description;
        existing.BasePrice = basePrice;
        existing.Category = category;
        existing.IsActive = isActive;
        existing.ModifiedUtc = DateTime.UtcNow;

        await _tableClient.UpsertAsync(existing);

        _logger.LogInformation(
            "Updated product {ProductId} in category {Category}",
            id,
            category);

        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return false;

        await _tableClient.DeleteAsync(existing);

        _logger.LogInformation(
            "Deleted product {ProductId} from category {Category}",
            id,
            existing.Category);

        return true;
    }

    /// <summary>
    /// Normalizes category name for consistent partition keys
    /// </summary>
    private static string NormalizeCategory(string category)
    {
        return category.Trim().ToLowerInvariant();
    }
}
