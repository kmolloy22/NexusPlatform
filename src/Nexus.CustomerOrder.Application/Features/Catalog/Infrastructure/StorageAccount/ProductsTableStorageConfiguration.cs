using Microsoft.Extensions.Configuration;
using Nexus.Infrastructure.StorageAccount.Tables;

namespace Nexus.CustomerOrder.Application.Features.Catalog.Infrastructure.StorageAccount;

public class ProductsTableStorageConfiguration : TableStorageConfiguration
{
    public ProductsTableStorageConfiguration(IConfiguration configuration)
        : base(configuration) { }

    public override string TableName => "Products";
}