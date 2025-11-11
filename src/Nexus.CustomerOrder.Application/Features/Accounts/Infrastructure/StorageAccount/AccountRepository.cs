using Azure;
using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using Nexus.CustomerOrder.Domain.Features.Accounts;
using Nexus.Infrastructure.StorageAccount.Tables.Client;
using Nexus.Shared.Kernel.Extensions;

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
            PhoneNumber = account.Phone,
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

    public async Task<AccountTableEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var maybe = await _tableClient.GetByIdAsync(AccountTableEntity.DefaultPartitionKey, id);
        return maybe.HasValue ? maybe.Value : null;
    }

    public async Task<bool> UpdateAsync(string id, string firstName, string lastName, string? email, string? phone, Address address, CancellationToken cancellationToken = default)
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

        await _tableClient.UpsertAsync(existing);
        return true;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        var existing = await GetByIdAsync(id, cancellationToken);
        if (existing is null)
            return false;

        await _tableClient.DeleteAsync(existing);
        return true;
    }
}