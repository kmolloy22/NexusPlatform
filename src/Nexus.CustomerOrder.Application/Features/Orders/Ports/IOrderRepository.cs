using Nexus.CustomerOrder.Application.Features.Orders.Infrastructure.StorageAccount;
using Nexus.CustomerOrder.Application.Shared.Results;

namespace Nexus.CustomerOrder.Application.Features.Orders.Ports;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken = default);

    Task<OrderTableEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<PagedResult<OrderTableEntity>> QueryByAccountAsync(
        string accountId,
        int pageSize,
        string? continuationToken = null,
        CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(
        string id,
        OrderStatus status,
        string? trackingNumber = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}