using Nexus.CustomerOrder.Domain.Features.Accounts;
using Nexus.Shared.Kernel.Extensions;

namespace Nexus.CustomerOrder.Application.Features.Orders;

public class Order
{
    public Guid Id { get; }
    public Guid AccountId { get; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyList<OrderLine> Lines { get; }
    public Address ShippingAddress { get; }
    public decimal SubTotal { get; }
    public decimal Tax { get; }
    public decimal Total { get; }
    public DateTime OrderedUtc { get; }
    public DateTime? ShippedUtc { get; private set; }
    public DateTime? DeliveredUtc { get; private set; }
    public string? TrackingNumber { get; private set; }

    public Order(
        Guid id,
        Guid accountId,
        IEnumerable<OrderLine> lines,
        Address shippingAddress,
        DateTime orderedUtc,
        OrderStatus status = OrderStatus.Submitted)
    {
        if (lines.IsNullOrEmpty())
            throw new ArgumentException("Order must have at least one order line item.", nameof(lines));

        Id = id;
        AccountId = accountId;
        Lines = lines.ToList();
        ShippingAddress = shippingAddress ?? throw new ArgumentNullException(nameof(shippingAddress));
        OrderedUtc = orderedUtc;
        Status = status;

        // calculate totals
        SubTotal = Lines.Sum(l => l.LineTotal);
        Tax = CalculateTax(SubTotal);
        Total = SubTotal + Tax;
    }

    /// <summary>
    /// Updates order status with validation
    /// </summary>
    public void UpdateStatus(OrderStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
            throw new InvalidOperationException($"Cannot transition from {Status} to {newStatus}");

        Status = newStatus;

        // Set timestamps based on status
        if (newStatus == OrderStatus.Shipped && !ShippedUtc.HasValue)
            ShippedUtc = DateTime.UtcNow;
        if (newStatus == OrderStatus.Delivered && !DeliveredUtc.HasValue)
            DeliveredUtc = DateTime.UtcNow;
    }

    private bool CanTransitionTo(OrderStatus newStatus)
    {
        return (Status, newStatus) switch
        {
            (OrderStatus.Draft, OrderStatus.Submitted) => true,
            (OrderStatus.Submitted, OrderStatus.Processing) => true,
            (OrderStatus.Processing, OrderStatus.Shipped) => true,
            (OrderStatus.Shipped, OrderStatus.Delivered) => true,
            (_, OrderStatus.Cancelled) => Status != OrderStatus.Delivered,
            _ => false
        };
    }

    private static decimal CalculateTax(decimal subTotal)
    {
        // Simple tax calculation -8 % sales tax
        // In real system, this would be based on shipping address
        return Math.Round(subTotal * 0.08m, 2);
    }

    /// <summary>
    /// Sets tracking information when order is shipped
    /// </summary>
    public void SetTracking(string trackingNumber)
    {
        if (trackingNumber.IsMissing())
            throw new ArgumentException("Tracking number cannot be empty", nameof(trackingNumber));

        TrackingNumber = trackingNumber;
    }
}