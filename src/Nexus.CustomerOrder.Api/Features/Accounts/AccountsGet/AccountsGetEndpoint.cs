using MediatR;

namespace Nexus.CustomerOrder.Api.Features.Accounts.AccountsGet;

public static class AccountsGetEndpoint
{
    public static void MapGetAccountsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (
            int? take,
            string? continuationToken,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var pageSize = take.HasValue && take.Value > 0 ? take.Value : 50;

            var result = await mediator.Send(new GetAccountsCommand(pageSize, continuationToken), ct);

            return Results.Ok(result);
        })
        .WithName("GetAccounts")
        .WithSummary("Lists accounts.")
        .WithDescription("Returns a page of accounts. Use 'take' for page size and 'continuationToken' for pagination.");
    }
}