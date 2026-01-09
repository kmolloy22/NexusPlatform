using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nexus.CustomerOrder.Api.Infrastructure.Validation;
using Nexus.CustomerOrder.Application.Features.Accounts;
using Nexus.CustomerOrder.Application.Features.Accounts.Models;

namespace Nexus.CustomerOrder.Api.Features.Accounts.AccountUpdate;

public static class UpdateAccountEndpoint
{
    public static void MapUpdateAccountEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPut("/{id}", async (
            string id,
            [FromBody] CreateAccountDto dto,
            [FromServices] IMediator mediator,
            CancellationToken ct) =>
        {
            var cmd = new UpdateAccountCommand(
                id,
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Phone,
                dto.IsActive,
                new Domain.Features.Accounts.Address(
                    dto.Address.Street1,
                    dto.Address.Street2,
                    dto.Address.City,
                    dto.Address.State,
                    dto.Address.PostalCode,
                    dto.Address.Country));

            var updated = await mediator.Send(cmd, ct);
            return updated ? Results.NoContent() : Results.NotFound();
        })
        .AddEndpointFilter<ValidationFilter<CreateAccountDto>>()
        .WithName("UpdateAccount")
        .WithSummary("Updates an account.")
        .WithDescription("Updates first, last name and/or address for the specified account.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi();
    }
}