using MediatR;
using Nexus.CustomerOrder.Application.Features.Catalog.Ports;

namespace Nexus.CustomerOrder.Application.Features.Catalog;

public record DeleteProductCommand(string ProductId) : IRequest<bool>;

internal class DeleteProductHandler(IProductRepository repository)
    : IRequestHandler<DeleteProductCommand, bool>
{
    public async Task<bool> Handle(
        DeleteProductCommand request,
        CancellationToken cancellationToken)
    {
        return await repository.DeleteAsync(request.ProductId, cancellationToken);
    }
}