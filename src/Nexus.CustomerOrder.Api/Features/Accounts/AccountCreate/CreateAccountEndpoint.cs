using MediatR;
using Microsoft.AspNetCore.Mvc;
using Nexus.CustomerOrder.Api.Infrastructure.Validation;
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

            var command = new CreateAccountCommand(
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.Phone,
                dto.IsActive,
                address);

            var result = await mediator.Send(command);

            var idN = result.Id.ToString("N");
            var location = $"/api/accounts/{idN}";

            var response = new CreateAccountResponseDto(
                Id: idN,
                Location: location,
                CreatedAt: result.CreatedAt
            );

            return Results.Created(location, response);
        })
        .AddEndpointFilter<ValidationFilter<CreateAccountDto>>()
        .WithName("CreateAccount")
        .WithSummary("Creates a new account")
        .WithDescription("Creates a new customer account with the provided details and returns the account identifier")
        .Produces<CreateAccountResponseDto>(StatusCodes.Status201Created)
        .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithOpenApi();
    }
}