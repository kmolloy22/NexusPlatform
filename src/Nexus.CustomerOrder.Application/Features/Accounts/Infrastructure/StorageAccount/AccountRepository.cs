using Azure;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.CustomerOrder.Domain.Features.Accounts;
using Nexus.Infrastructure.StorageAccount.Tables.Client;

namespace Nexus.CustomerOrder.Application.Features.Accounts.Infrastructure.StorageAccount;

internal class AccountRepository : IAccountRepository
{
    private readonly ITableClient<AccountsTableStorageConfiguration, AccountTableEntity> _tableClient;

    public AccountRepository(ITableClient<AccountsTableStorageConfiguration, AccountTableEntity> tableClient)
    {
        _tableClient = tableClient;
    }

    public async Task AddAsync(Account account, CancellationToken cancellationToken = default)
    {
        var entity = new AccountTableEntity
        {
            PartitionKey = AccountTableEntity.DefaultPartitionKey,
            RowKey = account.Id.ToString("N"),
            FirstName = account.FirstName,
            LastName = account.LastName,
            Email = account.Email,
            Address_Street1 = account.Address.Street1,
            Address_Street2 = account.Address.Street2,
            Address_City = account.Address.City,
            Address_State = account.Address.State,
            Address_PostalCode = account.Address.PostalCode,
            Address_Country = account.Address.Country
        };

        await _tableClient.AddAsync(entity);
    }

    public async Task<Page<AccountTableEntity>?> QueryAsync(int pageSize, string? continuationToken = null, CancellationToken cancellationToken = default)
    {
        var query = _tableClient.QueryAsync(
            e => e.PartitionKey == AccountTableEntity.DefaultPartitionKey, pageSize: pageSize);

        await foreach (var page in query.AsPages(continuationToken, pageSize).WithCancellation(cancellationToken))
        {
            return page;
        }

        return null;
    }

}