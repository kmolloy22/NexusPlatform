namespace Nexus.CustomerOrder.Application.Features.Orders.Models;

public sealed record CreateOrderResponseDto(
    string Id,
    string Location,
    DateTimeOffset CreatedAt,
    decimal Total
);