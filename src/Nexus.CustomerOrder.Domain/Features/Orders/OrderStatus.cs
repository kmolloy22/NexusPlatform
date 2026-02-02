namespace Nexus.CustomerOrder.Application.Features.Orders;

public enum OrderStatus
{
    Draft = 0,
    Submitted = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 99
}