using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;
using Nexus.CustomerOrder.Application.Features.Accounts.Infrastructure.StorageAccount;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.Infrastructure.StorageAccount.Tables;

namespace Nexus.CustomerOrder.Application.Features.Accounts.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAccountsInfrastructure(this IServiceCollection services)
    {
        //var tableClient = new TableClient(connectionString, "Accounts");
        //services.AddSingleton(tableClient);
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddTableClient<AccountsTableStorageConfiguration, AccountTableEntity>();

        return services;
    }
}