namespace Nexus.Infrastructure.StorageAccount;

public interface IStorageConfiguration
{
    string ConnectionString { get; }

    /// <summary>
    /// For Read-access geo-redundant storage use PrimaryThenSecondary, SecondaryThenPrimary.
    /// Otherwise set this to PrimaryOnly.
    /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.storage.blob.blobrequestoptions.locationmode?view=azure-dotnet-legacy
    /// </summary>
    string LocationMode { get; }

    /// <summary>
    /// Delay in milliseconds.
    /// Leave it null to take default configuration.
    /// </summary>
    int RetryDelay { get; }

    /// <summary>
    /// Max number of retries.
    /// Leave it null to take default configuration.
    /// </summary>
    int MaxRetries { get; }

    /// <summary>
    /// Maximum Execution Seconds before forced timeout
    /// Leave it null to take default configuration.
    /// </summary>
    int MaximumExecutionSeconds { get; }
}
