using MediatR;
using Nexus.CustomerOrder.Application.Features.Accounts;

namespace Nexus.CustomerOrder.Api.Features.Accounts.AccountDelete;

public static class AccountDeleteEndpoint
{
    public static void MapDeleteAccountEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapDelete("/{id}", async (
            string id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var deleted = await mediator.Send(new DeleteAccountCommand(id), ct);
            return deleted ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteAccount")
        .WithSummary("Deletes an account.")
        .WithDescription("Deletes the specified account if it exists.");
    }
}