namespace Nexus.Infrastructure.StorageAccount.Partitioning;

public class PartitionStrategyOptions
{
    public int PartitionCount { get; set; } = 100;
    public string PartitionPrefix { get; set; } = "ACC";
}