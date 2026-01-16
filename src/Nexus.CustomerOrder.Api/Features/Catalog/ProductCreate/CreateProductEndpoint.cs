using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nexus.CustomerOrder.Api.Infrastructure.Validation;
using Nexus.CustomerOrder.Application.Features.Catalog;
using Nexus.CustomerOrder.Application.Features.Catalog.Models;

namespace Nexus.CustomerOrder.Api.Features.Catalog.ProductCreate;

public static class CreateProductEndpoint
{
    public static void MapCreateProductEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            [FromBody] CreateProductDto dto,
            [FromServices] IMediator mediator) =>
        {
            var command = new CreateProductCommand(
                dto.Sku,
                dto.Name,
                dto.Description,
                dto.BasePrice,
                dto.Category,
                dto.IsActive);

            var result = await mediator.Send(command);

            var idN = result.Id.ToString("N");
            var location = $"/api/products/{idN}";

            var response = new CreateProductResponseDto(
                Id: idN,
                Location: location,
                CreatedAt: result.CreatedAt
            );

            return Results.Created(location, response);
        })
        .AddEndpointFilter<ValidationFilter<CreateProductDto>>()
        .WithName("CreateProduct")
        .WithSummary("Creates a new product in the catalog")
        .WithDescription("Creates a new product with the provided details and returns the product identifier")
        .Produces<CreateProductResponseDto>(StatusCodes.Status201Created)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi();
    }
}