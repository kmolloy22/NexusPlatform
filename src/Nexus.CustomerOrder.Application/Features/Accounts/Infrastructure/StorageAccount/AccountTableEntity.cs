using Azure;
using Azure.Data.Tables;

namespace Nexus.CustomerOrder.Application.Features.Accounts.Infrastructure.StorageAccount;

public sealed class AccountTableEntity : ITableEntity
{
    //public const string DefaultPartitionKey = "accounts";

    public string PartitionKey { get; set; } = default;
    public string RowKey { get; set; } = default!;

    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string Address_Street1 { get; set; } = default!;
    public string? Address_Street2 { get; set; }
    public string Address_City { get; set; } = default!;
    public string? Address_State { get; set; }
    public string Address_PostalCode { get; set; } = default!;
    public string Address_Country { get; set; } = default!;
    public bool? IsActive { get; set; }

    //Add metadata fields
    public DateTime CreatedUtc { get; set; }
    public DateTime? ModifiedUtc { get; set; }
    public int PartitionStrategyVersion { get; set; } = 1;

    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}