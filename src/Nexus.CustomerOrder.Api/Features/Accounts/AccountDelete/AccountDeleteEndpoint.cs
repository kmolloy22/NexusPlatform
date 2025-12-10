using MediatR;
using Nexus.CustomerOrder.Application.Features.Accounts;
using Microsoft.AspNetCore.Mvc;
using Nexus.Shared.Kernel.Extensions;

namespace Nexus.CustomerOrder.Api.Features.Accounts.AccountDelete;

public static class AccountDeleteEndpoint
{
    public static void MapDeleteAccountEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", async (
            [FromRoute] string id,
            [FromServices] IMediator mediator,
            CancellationToken ct) =>
        {
            // Basic validation for empty/whitespace account ID
            if (id.IsMissing())
            {
                return Results.BadRequest(new { error = "Account ID is required and cannot be empty." });
            }

            var deleted = await mediator.Send(new DeleteAccountCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteAccount")
        .WithSummary("Deletes an account.")
        .WithDescription("Deletes the specified account if it exists. Returns 204 No Content if successful, 404 Not Found if the account doesn't exist, or 400 Bad Request for invalid input.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .Produces<object>(StatusCodes.Status400BadRequest, "application/json")
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi();
    }
}