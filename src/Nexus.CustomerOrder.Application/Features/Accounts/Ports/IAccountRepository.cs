using Azure;
using Nexus.CustomerOrder.Application.Features.Accounts.Infrastructure.StorageAccount;
using Nexus.CustomerOrder.Domain.Features.Accounts;
using System.Runtime.CompilerServices;

namespace Nexus.CustomerOrder.Application.Features.Accounts.Ports;

public interface IAccountRepository
{
    Task<Page<AccountTableEntity>?> QueryAsync(int pageSize, string? continuationToken = null, [EnumeratorCancellation] CancellationToken cancellationToken = default);

    Task AddAsync(Account account, CancellationToken cancellationToken = default);

    Task<AccountTableEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default);

    Task<bool> UpdateAsync(string id, string firstName, string lastName, string? email, string? phone, bool? isActive, Address address, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
}