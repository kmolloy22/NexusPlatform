using Azure;
using Azure.Data.Tables;

namespace Nexus.CustomerOrder.Application.Features.Orders.Infrastructure.StorageAccount;

public class OrderTableEntity : ITableEntity
{
    /// <summary>
    /// AccountId-YearMonth (e.g., "abc123-202501") for partition
    /// </summary>
    public string PartitionKey { get; set; } = default!;

    /// <summary>
    /// OrderId as row key
    /// </summary>
    public string RowKey { get; set; } = default!;

    public string AccountId { get; set; } = default!;
    public string Status { get; set; } = default!;

    /// <summary>
    /// JSON serialized order lines
    /// </summary>
    public string LinesJson { get; set; } = "[]";

    // Flattened shipping address
    public string ShippingAddress_Street1 { get; set; } = default!;

    public string? ShippingAddress_Street2 { get; set; }
    public string ShippingAddress_City { get; set; } = default!;
    public string? ShippingAddress_State { get; set; }
    public string ShippingAddress_PostalCode { get; set; } = default!;
    public string ShippingAddress_Country { get; set; } = default!;

    public decimal SubTotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Total { get; set; }

    public DateTime OrderedUtc { get; set; }
    public DateTime? ShippedUtc { get; set; }
    public DateTime? DeliveredUtc { get; set; }
    public string? TrackingNumber { get; set; }

    // Required by ITableEntity
    public DateTimeOffset? Timestamp { get; set; }

    public ETag ETag { get; set; }
}