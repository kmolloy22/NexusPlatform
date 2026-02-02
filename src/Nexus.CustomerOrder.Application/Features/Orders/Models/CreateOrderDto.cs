using Nexus.CustomerOrder.Application.Features.Accounts.Models;

namespace Nexus.CustomerOrder.Application.Features.Orders.Models;

public record CreateOrderDto(
    string AccountId,
    List<OrderLineDto> Lines,
    AddressDto ShippingAddress);

public record OrderLineDto(
    string ProductId,
    int QUantity);