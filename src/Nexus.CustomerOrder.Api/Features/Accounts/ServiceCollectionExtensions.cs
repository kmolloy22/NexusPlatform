//using Nexus.CustomerOrder.Api.Features.Accounts.Infrastructure.StorageAccounts;
//using Nexus.Infrastructure.StorageAccount.Tables;

//namespace Nexus.CustomerOrder.Api.Features.Accounts;

//public static class ServiceCollectionExtensions
//{
//    // Reusable by any Accounts-* feature
//    public static IServiceCollection AddAccountsFeature(this IServiceCollection services)
//    {
//        services.AddTableClient<AccountsTableStorageConfiguration, AccountTableEntity>();
//        return services;
//    }
//}