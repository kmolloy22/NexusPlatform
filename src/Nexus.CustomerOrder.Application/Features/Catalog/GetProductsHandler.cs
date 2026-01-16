using MediatR;
using Nexus.CustomerOrder.Application.Features.Catalog.Models;
using Nexus.CustomerOrder.Application.Features.Catalog.Ports;
using Nexus.CustomerOrder.Application.Shared.Results;

namespace Nexus.CustomerOrder.Application.Features.Catalog;

public record GetProductsCommand(
    int PageSize,
    string? Category = null,
    bool? IsActive = null,
    string? ContinuationToken = null
) : IRequest<PagedResult<GetProductDto>>;

internal class GetProductsHandler(IProductRepository repository)
    : IRequestHandler<GetProductsCommand, PagedResult<GetProductDto>>
{
    public async Task<PagedResult<GetProductDto>> Handle(
        GetProductsCommand request,
        CancellationToken cancellationToken)
    {
        var pagedEntities = await repository.QueryAsync(
            request.PageSize,
            request.Category,
            request.IsActive,
            request.ContinuationToken,
            cancellationToken);

        var items = pagedEntities.Items.Select(e => new GetProductDto(
            e.RowKey,
            e.Sku,
            e.Name,
            e.Description,
            e.BasePrice,
            e.Category,
            e.IsActive,
            e.CreatedUtc,
            e.ModifiedUtc
        )).ToList();

        return new PagedResult<GetProductDto>(items, pagedEntities.ContinuationToken);
    }
}