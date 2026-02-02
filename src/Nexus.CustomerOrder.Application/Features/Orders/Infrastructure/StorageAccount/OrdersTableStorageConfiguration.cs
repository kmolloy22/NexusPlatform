using Microsoft.Extensions.Configuration;
using Nexus.Infrastructure.StorageAccount.Tables;

namespace Nexus.CustomerOrder.Application.Features.Orders.Infrastructure.StorageAccount;

public class OrdersTableStorageConfiguration : TableStorageConfiguration
{
    public OrdersTableStorageConfiguration(IConfiguration configuration)
        : base(configuration) { }

    public override string TableName => "Orders";
}