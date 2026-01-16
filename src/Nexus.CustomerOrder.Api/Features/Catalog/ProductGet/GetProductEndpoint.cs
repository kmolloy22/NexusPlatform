using MediatR;
using Nexus.CustomerOrder.Application.Features.Catalog;

namespace Nexus.CustomerOrder.Api.Features.Catalog.ProductGet;

public static class GetProductEndpoint
{
    public static void MapGetProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}", async (
            string id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            try
            {
                var dto = await mediator.Send(new GetProductCommand(id), ct);
                return dto is null ? Results.NotFound() : Results.Ok(dto);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetProduct")
        .WithSummary("Gets a product by id")
        .WithDescription("Returns the product if it exists, otherwise 404")
        .WithOpenApi();
    }
}