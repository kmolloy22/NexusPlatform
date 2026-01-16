namespace Nexus.CustomerOrder.Domain.Features.Catalog;

using global::Nexus.Shared.Kernel.Extensions;

/// <summary>
/// Represents a product in the catalog
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier for the product
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Stock Keeping Unit - unique product code
    /// </summary>
    public string Sku { get; }

    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Detailed product description
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Base price for the product
    /// </summary>
    public double BasePrice { get; }

    /// <summary>
    /// Product category for organization
    /// </summary>
    public string Category { get; }

    /// <summary>
    /// Whether the product is available for purchase
    /// </summary>
    public bool IsActive { get; }

    /// <summary>
    /// When the product was created
    /// </summary>
    public DateTime CreatedUtc { get; }

    /// <summary>
    /// Initializes a new product instance
    /// </summary>
    /// <param name="id">Product identifier</param>
    /// <param name="sku">Stock keeping unit</param>
    /// <param name="name">Product name</param>
    /// <param name="description">Product description</param>
    /// <param name="basePrice">Base price</param>
    /// <param name="category">Product category</param>
    /// <param name="isActive">Whether product is active</param>
    /// <param name="createdUtc">Creation timestamp</param>
    /// <exception cref="ArgumentException">Thrown when required fields are missing or invalid</exception>
    public Product(
        Guid id,
        string sku,
        string name,
        string? description,
        double basePrice,
        string category,
        bool isActive,
        DateTime createdUtc)
    {
        if (sku.IsMissing())
            throw new ArgumentException("SKU is required", nameof(sku));

        if (name.IsMissing())
            throw new ArgumentException("Product name is required", nameof(name));

        if (basePrice <= 0)
            throw new ArgumentException("Base price must be greater than zero", nameof(basePrice));

        if (category.IsMissing())
            throw new ArgumentException("Category is required", nameof(category));

        Id = id;
        Sku = sku.Trim();
        Name = name.Trim();
        Description = description?.Trim();
        BasePrice = basePrice;
        Category = category.Trim();
        IsActive = isActive;
        CreatedUtc = createdUtc;
    }
}