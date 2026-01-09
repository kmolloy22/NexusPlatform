using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Nexus.Infrastructure.StorageAccount.Partitioning;

namespace Nexus.Infrastructure.StorageAccount;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPartitioningStrategy(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register partition strategy configuration
        services.Configure<PartitionStrategyOptions>(
            configuration.GetSection("PartitionStrategy"));

        // Register hash-based partition strategy
        services.AddSingleton<IPartitionKeyStrategy>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PartitionStrategyOptions>>();
            return new HashPartitionKeyStrategy(
                options.Value.PartitionCount,
                options.Value.PartitionPrefix);
        });

        return services;
    }
}