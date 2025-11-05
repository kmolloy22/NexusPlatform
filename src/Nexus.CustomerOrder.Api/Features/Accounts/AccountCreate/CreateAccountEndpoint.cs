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

//public static class CreateAccountEndpoint
//{
//    public static void MapCreateAccountEndpoint(this IEndpointRouteBuilder app)
//    {
//        app.MapPost("/", async (
//            CreateAccountDto dto,
//            ITableClient<AccountsTableStorageConfiguration, AccountTableEntity> tableClient) =>
//        {
//            if (dto is null || string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
//                return Results.BadRequest("FirstName and LastName are required.");

//            // If you require an address at creation enforce it here.
//            if (dto.Address is null || string.IsNullOrWhiteSpace(dto.Address.Street1) || string.IsNullOrWhiteSpace(dto.Address.City) || string.IsNullOrWhiteSpace(dto.Address.PostalCode) || string.IsNullOrWhiteSpace(dto.Address.Country))
//                return Results.BadRequest("Address (Street1, City, PostalCode, Country) is required.");

//            var accountId = Guid.NewGuid().ToString("N");

//            var entity = new AccountTableEntity
//            {
//                PartitionKey = AccountTableEntity.DefaultPartitionKey,
//                RowKey = accountId,
//                FirstName = dto.FirstName.Trim(),
//                LastName = dto.LastName.Trim(),
//                Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim(),

//                // Flattened address properties
//                Address_Street1 = dto.Address.Street1.Trim(),
//                Address_Street2 = string.IsNullOrWhiteSpace(dto.Address.Street2) ? null : dto.Address.Street2.Trim(),
//                Address_City = dto.Address.City.Trim(),
//                Address_State = string.IsNullOrWhiteSpace(dto.Address.State) ? null : dto.Address.State.Trim(),
//                Address_PostalCode = dto.Address.PostalCode.Trim(),
//                Address_Country = dto.Address.Country.Trim()
//            };

//            await tableClient.AddAsync(entity);

//            return Results.Created($"/accounts/{accountId}", new
//            {
//                id = accountId,
//                entity.FirstName,
//                entity.LastName,
//                entity.Email,
//                address = new
//                {
//                    street1 = entity.Address_Street1,
//                    street2 = entity.Address_Street2,
//                    city = entity.Address_City,
//                    state = entity.Address_State,
//                    postalCode = entity.Address_PostalCode,
//                    country = entity.Address_Country
//                }
//            });
//        })
//        .WithName("CreateAccount")
//        .WithSummary("Creates a new account.")
//        .WithDescription("This endpoint creates a new account with the provided first and last name and address.");
//    }
//}