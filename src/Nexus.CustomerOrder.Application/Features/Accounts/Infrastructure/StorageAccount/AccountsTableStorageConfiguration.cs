using Microsoft.Extensions.Configuration;
using Nexus.Infrastructure.StorageAccount.Tables;

namespace Nexus.CustomerOrder.Application.Features.Accounts.Infrastructure.StorageAccount;

public class AccountsTableStorageConfiguration : TableStorageConfiguration
{
    public AccountsTableStorageConfiguration(IConfiguration configuration)
        : base(configuration) { }

    public override string TableName => "Accounts";
}