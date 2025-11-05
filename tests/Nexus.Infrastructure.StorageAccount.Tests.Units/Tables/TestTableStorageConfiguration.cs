using Microsoft.Extensions.Configuration;
using Nexus.Infrastructure.StorageAccount.Tables;

namespace Nexus.Infrastructure.StorageAccount.Tests.Units.Tables;

internal class TestTableStorageConfiguration : TableStorageConfiguration
{
    public TestTableStorageConfiguration(IConfiguration configuration)
        : base(configuration)
    { }

    public override string TableName => "UnitTests";
}