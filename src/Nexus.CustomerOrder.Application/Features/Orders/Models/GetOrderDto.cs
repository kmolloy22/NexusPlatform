using Nexus.CustomerOrder.Application.Features.Accounts.Models;

namespace Nexus.CustomerOrder.Application.Features.Orders.Models;

public record GetOrderDto(
    string Id,
    string AccountId,
    string Status,
    List<GetOrderLineDto> Lines,
    AddressDto ShippingAddress,
    decimal SubTotal,
    decimal Tax,
    decimal Total,
    DateTime OrderedAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    string? TrackingNumber
);

public record GetOrderLineDto(
    string ProductId,
    string ProductSku,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal
);