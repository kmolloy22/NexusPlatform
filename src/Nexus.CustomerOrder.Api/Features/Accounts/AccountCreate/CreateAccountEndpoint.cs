using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nexus.CustomerOrder.Application.Features.Accounts;
using Nexus.CustomerOrder.Application.Features.Accounts.Models;
using Nexus.CustomerOrder.Domain.Features.Accounts;

namespace Nexus.CustomerOrder.Api.Features.Accounts.AccountCreate;

public static class CreateAccountEndpoint
{
    public static void MapCreateAccountEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/", async (
            [FromBody] CreateAccountDto dto,
            [FromServices] IMediator mediator) =>
        {
            var address = new Address(
                dto.Address.Street1,
                dto.Address.Street2,
                dto.Address.City,
                dto.Address.State,
                dto.Address.PostalCode,
                dto.Address.Country);

            var command = new CreateAccountCommand(dto.FirstName, dto.LastName, dto.Email, dto.Phone, address);

            var id = await mediator.Send(command);

            return Results.Created($"/accounts/{id}", new { id });
        })
        .WithName("CreateAccount")
        .WithSummary("Creates a new account");
    }
}