using MediatR;
using Nexus.CustomerOrder.Application.Features.Catalog.Ports;

namespace Nexus.CustomerOrder.Application.Features.Catalog;

public record UpdateProductCommand(
    string ProductId,
    string Name,
    string? Description,
    double BasePrice,
    string Category,
    bool IsActive
) : IRequest<bool>;

internal class UpdateProductHandler(IProductRepository repository)
    : IRequestHandler<UpdateProductCommand, bool>
{
    public async Task<bool> Handle(
        UpdateProductCommand request,
        CancellationToken cancellationToken)
    {
        return await repository.UpdateAsync(
            request.ProductId,
            request.Name,
            request.Description,
            request.BasePrice,
            request.Category,
            request.IsActive,
            cancellationToken);
    }
}