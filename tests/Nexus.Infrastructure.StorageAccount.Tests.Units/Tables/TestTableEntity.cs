using Azure;
using Azure.Data.Tables;

namespace Nexus.Infrastructure.StorageAccount.Tests.Units.Tables;

public class TestTableEntity : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }

    public string TestData { get; set; }

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}