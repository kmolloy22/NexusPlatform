using Azure;
using Azure.Core;
using Microsoft.Extensions.Logging;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.CustomerOrder.Application.Shared.Results;
using Nexus.CustomerOrder.Domain.Features.Accounts;
using Nexus.Infrastructure.StorageAccount.Partitioning;
using Nexus.Infrastructure.StorageAccount.Tables.Client;
using Nexus.Shared.Kernel.Extensions;
using System.Runtime.CompilerServices;

namespace Nexus.CustomerOrder.Application.Features.Accounts.Infrastructure.StorageAccount;

internal class AccountRepository : IAccountRepository
{
    private readonly ITableClient<AccountsTableStorageConfiguration, AccountTableEntity> _tableClient;
    private readonly IPartitionKeyStrategy _partitionStrategy;
    private readonly ILogger<AccountRepository> _logger;

    public AccountRepository(
        ITableClient<AccountsTableStorageConfiguration, AccountTableEntity> tableClient,
        IPartitionKeyStrategy partitionStrategy,
        ILogger<AccountRepository> logger)
    {
        _tableClient = tableClient;
        _partitionStrategy = partitionStrategy;
        _logger = logger;
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        var entity = new AccountTableEntity
        {
            PartitionKey = _partitionStrategy.GetPartitionKey(account.Id),
            RowKey = account.Id.ToString("N"),

            FirstName = account.FirstName,
            LastName = account.LastName,
            Email = account.Email,
            PhoneNumber = account.Phone,

            Address_Street1 = account.Address.Street1,
            Address_Street2 = account.Address.Street2,
            Address_City = account.Address.City,
            Address_State = account.Address.State,
            Address_PostalCode = account.Address.PostalCode,
            Address_Country = account.Address.Country,
            IsActive = account.IsActive,

            CreatedUtc = DateTime.UtcNow,
            ModifiedUtc = DateTime.UtcNow,

            PartitionStrategyVersion = 1
        };

        await _tableClient.AddAsync(entity);
    }

    public async Task<PagedResult<AccountTableEntity>> QueryAsync(
    int pageSize,
    string? continuationToken = null,
    CancellationToken cancellationToken = default)
    {
        var skip = string.IsNullOrWhiteSpace(continuationToken)
            ? 0
            : int.TryParse(continuationToken, out var s) ? s : 0;

        var allPartitions = _partitionStrategy.GetAllPartitionKeys().ToList();

        _logger.LogDebug(
            "Querying {PartitionCount} partitions, skip={Skip}, pageSize={PageSize}",
            allPartitions.Count,
            skip,
            pageSize);

        // Query all partitions in parallel
        var queryTasks = allPartitions.Select(async partition =>
        {
            var results = new List<AccountTableEntity>();
            var query = _tableClient.QueryAsync(
                filter: e => e.PartitionKey == partition);

            await foreach (var entity in query.WithCancellation(cancellationToken))
            {
                results.Add(entity);
            }
            return results;
        });

        var partitionResults = await Task.WhenAll(queryTasks);

        // Flatten, sort, and page
        var allEntities = partitionResults
            .SelectMany(r => r)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ThenBy(e => e.RowKey)
            .Skip(skip)
            .Take(pageSize + 1)
            .ToList();

        var hasMore = allEntities.Count > pageSize;
        var pagedEntities = allEntities.Take(pageSize).ToList();
        var nextToken = hasMore ? (skip + pageSize).ToString() : null;

        _logger.LogInformation(
            "Returning {Count} accounts, hasMore={HasMore}, nextToken={Token}",
            pagedEntities.Count,
            hasMore,
            nextToken ?? "null");

        // Return custom PagedResult
        return new PagedResult<AccountTableEntity>(pagedEntities, nextToken);
    }

    public async Task<AccountTableEntity?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        //Calculate partition key from ID
        var guid = Guid.Parse(id);
        var partitionKey = _partitionStrategy.GetPartitionKey(guid);

        _logger.LogDebug(
            "Getting account {AccountId} from partition {PartitionKey}",
            id,
            partitionKey);

        var maybe = await _tableClient.GetByIdAsync(partitionKey, id);
        return maybe.HasValue ? maybe.Value : null;
    }

    public async Task<bool> UpdateAsync(
        string id,
        string firstName,
        string lastName,
        string? email,
        string? phone,
        bool? isActive,
        Address address,
        CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return false;

        existing.FirstName = firstName;
        existing.LastName = lastName;
        existing.Email = email.IsMissing() ? null : email.Trim();
        existing.PhoneNumber = phone.IsMissing() ? null : phone.Trim();
        existing.Address_Street1 = address.Street1;
        existing.Address_Street2 = address.Street2;
        existing.Address_City = address.City;
        existing.Address_State = address.State;
        existing.Address_PostalCode = address.PostalCode;
        existing.Address_Country = address.Country;
        existing.IsActive = isActive;
        existing.ModifiedUtc = DateTime.UtcNow;

        await _tableClient.UpsertAsync(existing);

        _logger.LogInformation(
            "Updated account {AccountId} in partition {PartitionKey}",
            id,
            existing.PartitionKey);

        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return false;

        await _tableClient.DeleteAsync(existing);

        _logger.LogInformation(
            "Deleted account {AccountId} from partition {PartitionKey}",
            id,
            existing.PartitionKey);

        return true;
    }


    // Helper class for creating Page<T>
    private class MockResponse : Response
    {
        public override int Status => 200;
        public override string ReasonPhrase => "OK";
        public override Stream? ContentStream { get; set; }
        public override string ClientRequestId { get; set; } = string.Empty;

        public override void Dispose()
        { }

        protected override bool ContainsHeader(string name) => false;

        protected override IEnumerable<HttpHeader> EnumerateHeaders() => Enumerable.Empty<HttpHeader>();

        protected override bool TryGetHeader(string name, out string? value)
        {
            value = null;
            return false;
        }

        protected override bool TryGetHeaderValues(string name, out IEnumerable<string>? values)
        {
            values = null;
            return false;
        }
    }
}