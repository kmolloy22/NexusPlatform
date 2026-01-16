using Microsoft.Extensions.DependencyInjection;
using Nexus.CustomerOrder.Application.Features.Catalog.Infrastructure.StorageAccount;
using Nexus.CustomerOrder.Application.Features.Catalog.Ports;
using Nexus.Infrastructure.StorageAccount.Tables;

namespace Nexus.CustomerOrder.Application.Features.Catalog.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCatalogInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddTableClient<ProductsTableStorageConfiguration, ProductTableEntity>();

        return services;
    }
}