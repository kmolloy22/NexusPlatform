using Microsoft.Extensions.DependencyInjection;
using Nexus.Infrastructure.StorageAccount.Tables.Client;

namespace Nexus.Infrastructure.StorageAccount.Tests.Units.Tables;

public class TableClientFixture : ClientFixtureBase
{
    public TableClientFixture()
    {
        var services = Initialize();

        services.AddSingleton<TestTableStorageConfiguration>();
        services.AddSingleton<TableClient<TestTableStorageConfiguration, TestTableEntity>>();
        services.AddSingleton<ITableClient<TestTableStorageConfiguration, TestTableEntity>>(ctx => ctx.GetService<TableClient<TestTableStorageConfiguration, TestTableEntity>>());

        ServiceProvider = services.BuildServiceProvider();
    }
}