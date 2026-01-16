using MediatR;
using Nexus.CustomerOrder.Application.Features.Catalog.Models;
using Nexus.CustomerOrder.Application.Features.Catalog.Ports;
using Nexus.Shared.Kernel.Extensions;

namespace Nexus.CustomerOrder.Application.Features.Catalog;

public record GetProductCommand(string ProductId) : IRequest<GetProductDto?>;

internal class GetProductHandler(IProductRepository repository)
    : IRequestHandler<GetProductCommand, GetProductDto?>
{
    public async Task<GetProductDto?> Handle(
        GetProductCommand cmd,
        CancellationToken cancellationToken)
    {
        if (cmd.ProductId.IsMissing())
            throw new ArgumentNullException("Product id cannot be empty.", nameof(cmd.ProductId));

        var entity = await repository.GetByIdAsync(cmd.ProductId, cancellationToken);
        if (entity is null)
            return null;

        return new GetProductDto(
            entity.RowKey,
            entity.Sku,
            entity.Name,
            entity.Description,
            entity.BasePrice,
            entity.Category,
            entity.IsActive,
            entity.CreatedUtc,
            entity.ModifiedUtc
        );
    }
}