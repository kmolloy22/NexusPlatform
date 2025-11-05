//using Nexus.CustomerOrder.Api.Features.Accounts.Infrastructure.StorageAccounts;
//using Nexus.Infrastructure.StorageAccount.Tables.Client;

//namespace Nexus.CustomerOrder.Api.Features.Accounts.AccountDelete;

//public static class AccountDeleteEndpoint
//{
//    public static void MapDeleteAccountEndpoint(this IEndpointRouteBuilder app)
//    {
//        app.MapDelete("/{id}", async (
//            string id,
//            ITableClient<AccountsTableStorageConfiguration, AccountTableEntity> tableClient) =>
//        {
//            var maybe = await tableClient.GetByIdAsync(AccountTableEntity.DefaultPartitionKey, id);
//            if (!maybe.HasValue)
//                return Results.NotFound();

//            await tableClient.DeleteAsync(maybe.Value);
//            return Results.NoContent();
//        })
//        .WithName("DeleteAccount")
//        .WithSummary("Deletes an account.")
//        .WithDescription("Deletes the specified account if it exists.");
//    }
//}