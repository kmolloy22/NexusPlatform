namespace Nexus.Infrastructure.StorageAccount.Partitioning;

public class HashPartitionKeyStrategy : IPartitionKeyStrategy
{
    private readonly int _partitionCount;
    private readonly string _prefix;

    public HashPartitionKeyStrategy(int partitionCount = 100, string prefix = "ACC")
    {
        if (partitionCount <= 0 || partitionCount > 1000)
            throw new ArgumentOutOfRangeException(nameof(partitionCount), "Partition count must be between 1 and 1000.");

        _partitionCount = partitionCount;
        _prefix = prefix;
    }

    public string GeneratePartitionKey(Guid entityId)
    {
        var hashCode = GetStableHashCode(entityId.ToString());
        var partitionNumber = Math.Abs(hashCode % _partitionCount);
        return $"{_prefix}-{partitionNumber:D3}";
    }

    public IEnumerable<string> GetAllPartitionKeys()
    {
        return Enumerable.Range(0, _partitionCount)
            .Select(i => $"{_prefix}-{i:D3}");
    }

    public string GetPartitionKey(Guid entityId)
    {
        return GeneratePartitionKey(entityId);
    }

    private static int GetStableHashCode(string str)
    {
        unchecked
        {
            int hash1 = 5381;
            int hash2 = hash1;

            for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
            {
                hash1 = ((hash1 << 5) + hash1) ^ str[i];
                if (i == str.Length - 1 || str[i + 1] == '\0')
                    break;
                hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
            }

            return hash1 + (hash2 * 1566083941);
        }
    }
}