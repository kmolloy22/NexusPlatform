using Microsoft.Extensions.Logging;
using Nexus.CustomerOrder.Application.Features.Orders.Ports;
using Nexus.CustomerOrder.Application.Shared.Results;
using Nexus.Infrastructure.StorageAccount.Tables.Client;
using Nexus.Shared.Kernel.Extensions;
using Nexus.Shared.Kernel.Serialization;

namespace Nexus.CustomerOrder.Application.Features.Orders.Infrastructure.StorageAccount;

public class OrderRepository : IOrderRepository
{
    private readonly ITableClient<OrdersTableStorageConfiguration, OrderTableEntity> _tableClient;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(
        ITableClient<OrdersTableStorageConfiguration, OrderTableEntity> tableClient,
        ILogger<OrderRepository> logger)
    {
        _tableClient = tableClient;
        _logger = logger;
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        var entity = new OrderTableEntity
        {
            PartitionKey = GeneratePartitionKey(order.AccountId, order.OrderedUtc),
            RowKey = order.Id.ToString("N"),
            AccountId = order.AccountId.ToString("N"),
            Status = order.Status.ToString(),
            LinesJson = SerializeOrderLines(order.Lines),
            ShippingAddress_Street1 = order.ShippingAddress.Street1,
            ShippingAddress_Street2 = order.ShippingAddress.Street2,
            ShippingAddress_City = order.ShippingAddress.City,
            ShippingAddress_State = order.ShippingAddress.State,
            ShippingAddress_PostalCode = order.ShippingAddress.PostalCode,
            ShippingAddress_Country = order.ShippingAddress.Country,
            SubTotal = order.SubTotal,
            Tax = order.Tax,
            Total = order.Total,
            OrderedUtc = order.OrderedUtc,
            ShippedUtc = order.ShippedUtc,
            DeliveredUtc = order.DeliveredUtc,
            TrackingNumber = order.TrackingNumber
        };

        await _tableClient.AddAsync(entity);

        _logger.LogInformation($"Order {order.Id} added for Account {order.AccountId}");
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return false;

        await _tableClient.DeleteAsync(existing);

        _logger.LogInformation($"Order {id} deleted.");
        return true;
    }

    public async Task<OrderTableEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (id.IsMissing())
            return null;

        var query = _tableClient.QueryAsync(e => e.RowKey == id);
        await foreach (var entity in query.WithCancellation(cancellationToken))
        {
            return entity;
        }

        return null;
    }

    public async Task<PagedResult<OrderTableEntity>> QueryByAccountAsync(string accountId, int pageSize, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var skip = continuationToken.IsMissing()
            ? 0
            : int.TryParse(continuationToken, out var token) ? token : 0;

        _logger.LogDebug(
           "Querying orders for account {AccountId}, skip={Skip}, pageSize={PageSize}",
           accountId,
           skip,
           pageSize);

        var allOrders = new List<OrderTableEntity>();
        var query = _tableClient.QueryAsync(e => e.AccountId == accountId);

        await foreach (var entity in query.WithCancellation(cancellationToken))
        {
            allOrders.Add(entity);
        }

        var sortedOrders = allOrders
            .OrderByDescending(o => o.OrderedUtc)
            .Skip(skip)
            .Take(pageSize + 1)
            .ToList();

        var hasMore = sortedOrders.Count > pageSize;
        var pagedOrders = sortedOrders.Take(pageSize).ToList();
        var nextToken = hasMore ? (skip + pageSize).ToString() : null;

        _logger.LogInformation(
           "Returning {Count} orders for account {AccountId}, hasMore={HasMore}",
           pagedOrders.Count,
           accountId,
           hasMore);

        return new PagedResult<OrderTableEntity>(pagedOrders, nextToken);
    }

    public async Task<bool> UpdateStatusAsync(string id, OrderStatus status, string? trackingNumber = null, CancellationToken cancellationToken = default)
    {
        var existing =  await GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return false;

        existing.Status = status.ToString();

        // Update timestamps based on status
        if (status == OrderStatus.Shipped && existing.ShippedUtc.IsNull())
        {
            existing.ShippedUtc = DateTime.UtcNow;
            if (trackingNumber.IsPresent())
                existing.TrackingNumber = trackingNumber;
        }

        if (status == OrderStatus.Delivered && existing.DeliveredUtc.IsNull())
        {
            existing.DeliveredUtc = DateTime.UtcNow;
        }

        _logger.LogInformation(
            "Updated order {OrderId} status to {Status}",
            id,
            status);

        return true;

    }

    private static string GeneratePartitionKey(Guid accountId, DateTime orderDate)
    {
        // Format: accountId-YYYYMM
        return $"{accountId:N}-{orderDate:yyyyMM}";
    }

    private static string SerializeOrderLines(IEnumerable<OrderLine> lines)
    {
        var dtos = lines.Select(l => new
        {
            ProductId = l.ProductId.ToString("N"),
            ProductSku = l.ProductSku,
            ProductName = l.ProductName,
            Quantity = l.Quantity,
            UnitPrice = l.UnitPrice
        });

        return Serializer.ToJson(dtos);
    }
}