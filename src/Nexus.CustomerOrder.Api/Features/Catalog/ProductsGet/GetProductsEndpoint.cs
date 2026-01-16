using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nexus.CustomerOrder.Application.Features.Catalog;
using Nexus.CustomerOrder.Application.Features.Catalog.Models;
using Nexus.CustomerOrder.Application.Shared.Results;
using Nexus.Shared.Kernel.Extensions;

namespace Nexus.CustomerOrder.Api.Features.Catalog.ProductsGet;

public static class GetProductsEndpoint
{
    public static void MapGetProductsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (
            [FromQuery] int? take,
            [FromQuery] string? category,
            [FromQuery] bool? isActive,
            [FromQuery] string? continuationToken,
            [FromServices] IMediator mediator,
            CancellationToken ct) =>
        {
            // Validation for 'take' parameter
            if (take.HasValue && (take is <= 0 or > 1000))
            {
                return Results.BadRequest(new
                {
                    error = "The 'take' parameter must be between 1 and 1000."
                });
            }

            // Validation for 'continuationToken' parameter
            if (continuationToken != null && continuationToken.IsMissing())
            {
                return Results.BadRequest(new
                {
                    error = "The 'continuationToken' parameter cannot be whitespace only."
                });
            }

            var pageSize = take ?? 50;

            var result = await mediator.Send(
                new GetProductsCommand(pageSize, category, isActive, continuationToken),
                ct
            );

            return Results.Ok(result);
        })
        .WithName("GetProducts")
        .WithSummary("Lists products with pagination and filtering")
        .WithDescription("Returns a paginated list of products. Use 'take' (1-1000) for page size, 'category' to filter by category, 'isActive' to filter by status, and 'continuationToken' for pagination. Default page size is 50.")
        .Produces<PagedResult<GetProductDto>>(StatusCodes.Status200OK, "application/json")
        .Produces<object>(StatusCodes.Status400BadRequest, "application/json")
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "Number of products to retrieve per page (1-1000). Default: 50";
            operation.Parameters[1].Description = "Filter by category (optional)";
            operation.Parameters[2].Description = "Filter by active status (optional)";
            operation.Parameters[3].Description = "Continuation token from previous response for pagination";
            return operation;
        });
    }
}