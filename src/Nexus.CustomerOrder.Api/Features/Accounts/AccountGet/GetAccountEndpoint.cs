using MediatR;
using Nexus.CustomerOrder.Application.Features.Accounts;

namespace Nexus.CustomerOrder.Api.Features.Accounts.AccountGet;

public static class GetAccountEndpoint
{
    public static void MapGetAccountEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{id}", async (
            string id,
            IMediator mediator,
            CancellationToken ct) =>
        {
            try
            {
                var dto = await mediator.Send(new GetAccountCommand(id), ct);
                return dto is null ? Results.NotFound() : Results.Ok(dto);
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("GetAccount")
        .WithSummary("Gets an account by id.")
        .WithDescription("Returns the account if it exists, otherwise 404.");
    }
}