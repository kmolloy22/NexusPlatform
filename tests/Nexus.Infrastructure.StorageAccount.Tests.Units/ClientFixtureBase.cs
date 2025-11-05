using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Nexus.Infrastructure.StorageAccount.Tests.Units;

public abstract class ClientFixtureBase
{
    public ServiceProvider ServiceProvider { get; protected set; }

    public static IServiceCollection Initialize()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        var services = new ServiceCollection();
        //services.AddSingleton<IScopeContext, TestScopeContext>();
        services.AddSingleton<IConfiguration>(configuration);

        return services;
    }
}