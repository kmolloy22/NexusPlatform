using Microsoft.Extensions.Configuration;

namespace Nexus.Infrastructure.StorageAccount.Tables;

public interface ITableStorageConfiguration : IStorageConfiguration
{
    string TableName { get; }

    int BulkUploadMaxParallelThreads { get; }
}

public abstract class TableStorageConfiguration : ITableStorageConfiguration
{
    private readonly IConfiguration _configuration;

    public TableStorageConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public abstract string TableName { get; }

    public string ConnectionString => _configuration.GetValue<string>("Storage:ConnectionString");
    public string LocationMode => _configuration.GetValue<string>("Storage:LocationMode", "PrimaryOnly");
    public int RetryDelay => _configuration.GetValue<int>("Storage:RetryDelay", 500);
    public int MaxRetries => _configuration.GetValue<int>("Storage:MaxRetries", 3);
    public int MaximumExecutionSeconds => _configuration.GetValue<int>("Storage:MaximumExecutionSeconds", 30);
    public int BulkUploadMaxParallelThreads => _configuration.GetValue<int>("Storage:BulkUploadMaxParallelThreads", 4);
}