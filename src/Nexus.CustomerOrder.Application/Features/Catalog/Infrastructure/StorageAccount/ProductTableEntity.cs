using Azure;
using Azure.Data.Tables;

namespace Nexus.CustomerOrder.Application.Features.Catalog.Infrastructure.StorageAccount;

/// <summary>
/// Azure Table Storage entity for products
/// Partitioned by Category for efficient queries
/// </summary>
public sealed class ProductTableEntity : ITableEntity
{
    /// <summary>
    /// Category used as partition key for logical grouping
    /// </summary>
    public string PartitionKey { get; set; } = default!;

    /// <summary>
    /// Product ID (GUID as string) used as row key
    /// </summary>
    public string RowKey { get; set; } = default!;

    public string Sku { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public double BasePrice { get; set; }
    public string Category { get; set; } = default!;
    public bool IsActive { get; set; }

    // Metadata fields
    public DateTime CreatedUtc { get; set; }

    public DateTime? ModifiedUtc { get; set; }

    // Required by ITableEntity
    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }
}