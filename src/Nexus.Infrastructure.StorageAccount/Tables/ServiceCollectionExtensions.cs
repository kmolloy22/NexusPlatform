using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Nexus.Infrastructure.StorageAccount.Tables.Client;

namespace Nexus.Infrastructure.StorageAccount.Tables;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTableClient<TTableStorageConfiguration, TTableEntity>(this IServiceCollection services)
            where TTableStorageConfiguration : class, ITableStorageConfiguration
            where TTableEntity : class, ITableEntity, new()
    {
        services.TryAddSingleton<TTableStorageConfiguration>();
        services.TryAddSingleton<TableClient<TTableStorageConfiguration, TTableEntity>>();
        services.TryAddSingleton<ITableClient<TTableStorageConfiguration, TTableEntity>>(ctx => ctx.GetService<TableClient<TTableStorageConfiguration, TTableEntity>>());

        return services;
    }
}