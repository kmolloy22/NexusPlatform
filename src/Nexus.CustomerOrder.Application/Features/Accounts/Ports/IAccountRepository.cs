using Azure;
using Nexus.CustomerOrder.Application.Features.Accounts.Infrastructure.StorageAccount;
using Nexus.CustomerOrder.Domain.Features.Accounts;

namespace Nexus.CustomerOrder.Application.Features.Accounts.Ports;

public interface IAccountRepository
{
    Task<Page<AccountTableEntity>?> QueryAsync(int pageSize, string? continuationToken = null, CancellationToken cancellationToken = default);
    Task AddAsync(Account account, CancellationToken cancellationToken = default);
    Task<AccountTableEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(string id, string firstName, string lastName, string? email, string? phone, Domain.Features.Accounts.Address address, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}