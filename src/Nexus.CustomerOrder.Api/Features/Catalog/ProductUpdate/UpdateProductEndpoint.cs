using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nexus.CustomerOrder.Api.Infrastructure.Validation;
using Nexus.CustomerOrder.Application.Features.Catalog;
using Nexus.CustomerOrder.Application.Features.Catalog.Models;

namespace Nexus.CustomerOrder.Api.Features.Catalog.ProductUpdate;

public static class UpdateProductEndpoint
{
    public static void MapUpdateProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}", async (
            string id,
            [FromBody] UpdateProductDto dto,
            [FromServices] IMediator mediator,
            CancellationToken ct) =>
        {
            var cmd = new UpdateProductCommand(
                id,
                dto.Name,
                dto.Description,
                dto.BasePrice,
                dto.Category,
                dto.IsActive);

            var updated = await mediator.Send(cmd, ct);
            return updated ? Results.NoContent() : Results.NotFound();
        })
        .AddEndpointFilter<ValidationFilter<UpdateProductDto>>()
        .WithName("UpdateProduct")
        .WithSummary("Updates a product")
        .WithDescription("Updates the specified product's details")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi();
    }
}