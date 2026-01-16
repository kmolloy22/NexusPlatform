using MediatR;
using Nexus.CustomerOrder.Application.Features.Catalog.Ports;
using Nexus.CustomerOrder.Domain.Features.Catalog;

namespace Nexus.CustomerOrder.Application.Features.Catalog;

public record CreateProductCommand(
    string Sku,
    string Name,
    string? Description,
    double BasePrice,
    string Category,
    bool IsActive
) : IRequest<CreateProductResult>;

public sealed record CreateProductResult(Guid Id, DateTimeOffset CreatedAt);

public class CreateProductHandler(IProductRepository repository)
    : IRequestHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var createdAt = DateTimeOffset.UtcNow;

        // Check if SKU already exists
        var existingProduct = await repository.GetBySkuAsync(request.Sku, cancellationToken);
        if (existingProduct != null)
        {
            throw new InvalidOperationException($"A product with SKU '{request.Sku}' already exists.");
        }

        var product = new Product(
            Guid.NewGuid(),
            request.Sku,
            request.Name,
            request.Description,
            request.BasePrice,
            request.Category,
            request.IsActive,
            createdAt.UtcDateTime);

        await repository.AddAsync(product, cancellationToken);

        return new CreateProductResult(product.Id, createdAt);
    }
}