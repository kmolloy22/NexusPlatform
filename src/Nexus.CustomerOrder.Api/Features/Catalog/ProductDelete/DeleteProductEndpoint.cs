using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nexus.CustomerOrder.Application.Features.Catalog;
using Nexus.Shared.Kernel.Extensions;

namespace Nexus.CustomerOrder.Api.Features.Catalog.ProductDelete;

public static class DeleteProductEndpoint
{
    public static void MapDeleteProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", async (
            [FromRoute] string id,
            [FromServices] IMediator mediator,
            CancellationToken ct) =>
        {
            if (id.IsMissing())
            {
                return Results.BadRequest(new
                {
                    error = "Product ID is required and cannot be empty."
                });
            }

            var deleted = await mediator.Send(new DeleteProductCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProduct")
        .WithSummary("Deletes a product")
        .WithDescription("Deletes the specified product from the catalog. Returns 204 No Content if successful, 404 Not Found if the product doesn't exist.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<object>(StatusCodes.Status400BadRequest, "application/json")
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi();
    }
}