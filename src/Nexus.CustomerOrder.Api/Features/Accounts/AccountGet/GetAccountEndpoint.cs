//using Nexus.CustomerOrder.Api.Features.Accounts.Infrastructure.StorageAccounts;
//using Nexus.Infrastructure.StorageAccount.Tables.Client;

//namespace Nexus.CustomerOrder.Api.Features.Accounts.AccountGet;

//public static class GetAccountEndpoint
//{
//    public static void MapGetAccountEndpoint(this IEndpointRouteBuilder app)
//    {
//        app.MapGet("/{id}", async (
//            string id,
//            ITableClient<AccountsTableStorageConfiguration, AccountTableEntity> tableClient) =>
//        {
//            var maybe = await tableClient.GetByIdAsync(AccountTableEntity.DefaultPartitionKey, id);
//            if (!maybe.HasValue)
//                return Results.NotFound();

//            var e = maybe.Value;
//            return Results.Ok(new
//            {
//                id,
//                e.FirstName,
//                e.LastName,
//                e.Email,
//                address = new
//                {
//                    street1 = e.Address_Street1,
//                    street2 = e.Address_Street2,
//                    city = e.Address_City,
//                    state = e.Address_State,
//                    postalCode = e.Address_PostalCode,
//                    country = e.Address_Country
//                }
//            });
//        })
//        .WithName("GetAccount")
//        .WithSummary("Gets an account by id.")
//        .WithDescription("Returns the account if it exists, otherwise 404.");
//    }
//}