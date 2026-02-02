namespace Nexus.CustomerOrder.Application.Features.Orders.Models;

public record UpdateOrderStatusDto(
    string Status,
    string? TrackingNumber = null
);