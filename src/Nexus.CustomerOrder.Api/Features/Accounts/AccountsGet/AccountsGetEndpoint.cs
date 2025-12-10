using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nexus.CustomerOrder.Application.Features.Accounts.Models;
using Nexus.CustomerOrder.Application.Shared.Results;
using Nexus.Shared.Kernel.Extensions;

namespace Nexus.CustomerOrder.Api.Features.Accounts.AccountsGet;

public static class AccountsGetEndpoint
{
    public static void MapGetAccountsEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", async (
            [FromQuery] int? take,
            [FromQuery] string? continuationToken,
            [FromServices] IMediator mediator,
            CancellationToken ct) =>
        {
            // Validation for 'take' parameter
            if (take.HasValue)
            {
                if (take is <= 0 or > 1000)
                {
                    return Results.BadRequest(new
                    {
                        error = "The 'take' parameter must be between 1 and 1000."
                    });
                }
            }

            // Validation for 'continuationToken' parameter
            if (continuationToken != null && continuationToken.IsMissing())
            {
                return Results.BadRequest(new { error = "The 'continuationToken' parameter cannot be whitespace only." });
            }

            var pageSize = take ?? 50;

            var result = await mediator.Send(
                new GetAccountsCommand(pageSize, continuationToken),
                ct
            );

            return Results.Ok(result);
        })
        .WithName("GetAccounts")
        .WithSummary("Lists accounts with pagination support.")
        .WithDescription("Returns a paginated list of accounts. Use 'take' (1-1000) for page size and 'continuationToken' for pagination. Default page size is 50. The response includes a continuationToken for fetching the next page - when null, no more pages are available.")
        .Produces<PagedResult<GetAccountDto>>(StatusCodes.Status200OK, "application/json")
        .Produces<object>(StatusCodes.Status400BadRequest, "application/json")
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.Parameters[0].Description = "Number of accounts to retrieve per page (1-1000). Default: 50";
            operation.Parameters[1].Description = "Continuation token from previous response for pagination. Pass null or omit for first page.";
            return operation;
        });
    }
}