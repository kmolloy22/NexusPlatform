using Azure;
using Azure.Core;
using Azure.Data.Tables;
using Nexus.Shared.Core;
using System.Linq.Expressions;

namespace Nexus.Infrastructure.StorageAccount.Tables.Client;

public interface ITableClient<TTableStorageConfiguration, TTableEntity>
        where TTableStorageConfiguration : ITableStorageConfiguration
        where TTableEntity : ITableEntity
{
    AsyncPageable<TTableEntity> QueryAsync(Expression<Func<TTableEntity, bool>> filter, int? pageSize = null, IEnumerable<string> select = null);

    Task<Maybe<TTableEntity>> GetByIdAsync(string partitionKey, string rowKey);

    Task<Response> AddAsync(TTableEntity entity);

    Task<Response> UpsertAsync(TTableEntity entity);

    Task<Response> DeleteAsync(TTableEntity entity);

    Task<Response> DeleteAsync(string partitionKey, string rowKey, ETag eTag);

    Task<bool> ExistsAsync(string partitionKey, string rowKey);
}

public class TableClient<TTableStorageConfiguration, TTableEntity> : ITableClient<TTableStorageConfiguration, TTableEntity>
        where TTableStorageConfiguration : ITableStorageConfiguration
        where TTableEntity : class, ITableEntity, new()
{
    private readonly ITableStorageConfiguration _configuration;
    private readonly Lazy<TableServiceClient> _lazyServiceClient;

    public TableClient(TTableStorageConfiguration configuration)
    {
        _configuration = configuration;

        _lazyServiceClient = new Lazy<TableServiceClient>(() =>
        {
            var options = new TableClientOptions();
            options.Retry.Mode = RetryMode.Fixed;
            options.Retry.Delay = TimeSpan.FromMilliseconds(_configuration.RetryDelay);
            options.Retry.MaxRetries = _configuration.MaxRetries;
            // Fix: RetryOptions does not have TryTimeout property. Remove or handle timeout elsewhere.
            // If you need to set a timeout, consider configuring it at the request level or via TableServiceClient options if available.

            var service = new TableServiceClient(_configuration.ConnectionString, options);
            return service;
        }, LazyThreadSafetyMode.PublicationOnly);
    }

    public async Task<Maybe<TTableEntity>> GetByIdAsync(string partitionKey, string rowKey)
    {
        var table = GetTableClient();
        var result = await table.GetEntityIfExistsAsync<TTableEntity>(partitionKey, rowKey);

        if (result.HasValue)
            return Maybe<TTableEntity>.With(result.Value);

        return Maybe<TTableEntity>.Empty;
    }

    public Task<Response> AddAsync(TTableEntity entity)
    {
        var table = GetTableClient();
        return table.AddEntityAsync(entity);
    }

    public Task<Response> UpsertAsync(TTableEntity entity)
    {
        var table = GetTableClient();
        return table.UpsertEntityAsync(entity);
    }

    private TableClient GetTableClient()
    {
        var table = _lazyServiceClient.Value.GetTableClient(_configuration.TableName);

        // To avoid error in App Insights, more details see:
        // https://stackoverflow.com/questions/48893519/azure-table-storage-exception-409-conflict-unexpected
        table.CreateIfNotExists();

        return table;
    }

    public Task<Response> DeleteAsync(string partitionKey, string rowKey, ETag eTag)
    {
        var entity = new TTableEntity
        {
            PartitionKey = partitionKey,
            RowKey = rowKey,
            ETag = eTag
        };

        return DeleteAsync(entity);
    }

    public Task<Response> DeleteAsync(TTableEntity entity)
    {
        var table = GetTableClient();
        var etag = entity.ETag.Equals(default(ETag)) ? ETag.All : entity.ETag;
        return table.DeleteEntityAsync(entity.PartitionKey, entity.RowKey, etag);
    }

    public async Task<bool> ExistsAsync(string partitionKey, string rowKey)
    {
        var maybe = await GetByIdAsync(partitionKey, rowKey);
        return maybe.HasValue;
    }

    public AsyncPageable<TTableEntity> QueryAsync(Expression<Func<TTableEntity, bool>> filter, int? pageSize = null, IEnumerable<string> select = null)
    {
        var table = GetTableClient();
        return table.QueryAsync(filter, pageSize, select);
    }
}