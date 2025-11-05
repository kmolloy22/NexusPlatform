//using Nexus.CustomerOrder.Api.Features.Accounts.AccountCreate;
//using Nexus.CustomerOrder.Api.Features.Accounts.Infrastructure.StorageAccounts;
//using Nexus.Infrastructure.StorageAccount.Tables.Client;

//namespace Nexus.CustomerOrder.Api.Features.Accounts.AccountUpdate;

//public static class UpdateAccountEndpoint
//{
//    public static void MapUpdateAccountEndpoint(this IEndpointRouteBuilder app)
//    {
//        app.MapPut("/{id}", async (
//            string id,
//            CreateAccountDto dto,
//            ITableClient<AccountsTableStorageConfiguration, AccountTableEntity> tableClient) =>
//        {
//            var maybe = await tableClient.GetByIdAsync(AccountTableEntity.DefaultPartitionKey, id);
//            if (!maybe.HasValue)
//                return Results.NotFound();

//            var entity = maybe.Value;
//            if (!string.IsNullOrWhiteSpace(dto.FirstName))
//                entity.FirstName = dto.FirstName.Trim();
//            if (!string.IsNullOrWhiteSpace(dto.LastName))
//                entity.LastName = dto.LastName.Trim();

//            // If address is supplied, update flattened address fields
//            if (dto.Address is not null)
//            {
//                if (!string.IsNullOrWhiteSpace(dto.Address.Street1))
//                    entity.Address_Street1 = dto.Address.Street1.Trim();
//                if (!string.IsNullOrWhiteSpace(dto.Address.Street2))
//                    entity.Address_Street2 = dto.Address.Street2.Trim();
//                if (!string.IsNullOrWhiteSpace(dto.Address.City))
//                    entity.Address_City = dto.Address.City.Trim();
//                if (!string.IsNullOrWhiteSpace(dto.Address.State))
//                    entity.Address_State = dto.Address.State.Trim();
//                if (!string.IsNullOrWhiteSpace(dto.Address.PostalCode))
//                    entity.Address_PostalCode = dto.Address.PostalCode.Trim();
//                if (!string.IsNullOrWhiteSpace(dto.Address.Country))
//                    entity.Address_Country = dto.Address.Country.Trim();
//            }

//            await tableClient.UpsertAsync(entity);
//            return Results.NoContent();
//        })
//        .WithName("UpdateAccount")
//        .WithSummary("Updates an account.")
//        .WithDescription("Updates first, last name and/or address for the specified account.");
//    }
//}