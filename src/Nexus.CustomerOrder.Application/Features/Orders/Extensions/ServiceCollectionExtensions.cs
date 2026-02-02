using Microsoft.Extensions.DependencyInjection;

namespace Nexus.CustomerOrder.Application.Features.Orders.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersInfrastructure(this IServiceCollection services)
    {
        return services;
    }
}