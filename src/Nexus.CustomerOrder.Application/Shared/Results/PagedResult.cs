namespace Nexus.CustomerOrder.Application.Shared.Results;

public record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    string? ContinuationToken
);