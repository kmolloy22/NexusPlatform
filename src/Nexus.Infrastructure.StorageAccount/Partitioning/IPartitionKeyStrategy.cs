namespace Nexus.Infrastructure.StorageAccount.Partitioning;

public interface IPartitionKeyStrategy
{
    /// <summary>
    /// Generates a partition key for a given entity identifier
    /// </summary>
    string GeneratePartitionKey(Guid entityId);

    /// <summary>
    /// Returns all possible partition keys for scatter-gather queries
    /// </summary>
    IEnumerable<string> GetAllPartitionKeys();

    /// <summary>
    /// Extracts partition key from an entity (for updates/deletes)
    /// </summary>
    string GetPartitionKey(Guid entityId);
}