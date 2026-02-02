using Nexus.Shared.Kernel.Extensions;

namespace Nexus.CustomerOrder.Application.Features.Orders;

public class OrderLine
{
    public Guid ProductId { get; }
    public string ProductSku { get; }
    public string ProductName { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal LineTotal => Quantity * UnitPrice;

    public OrderLine(
        Guid productId,
        string productSku,
        string productName,
        int quantity,
        decimal unitPrice)
    {
        if (productSku.IsMissing())
            throw new ArgumentException("Product SKU is required", nameof(productSku));

        if (productName.IsMissing())
            throw new ArgumentException("Product name is required", nameof(productName));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        if (unitPrice <= 0)
            throw new ArgumentException("Unit price must be greater than zero", nameof(unitPrice));

        ProductId = productId;
        ProductSku = productSku.Trim();
        ProductName = productName.Trim();
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}