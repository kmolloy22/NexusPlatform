namespace Nexus.CustomerOrder.Application.Features.Accounts.Models;

/// <summary>
/// Response model for account creation operations
/// </summary>
public sealed record CreateAccountResponseDto(
    /// <summary>
    /// The unique identifier of the created account
    /// </summary>
    string Id,
    
    /// <summary>
    /// The URI location of the created account resource
    /// </summary>
    string Location,
    
    /// <summary>
    /// Timestamp when the account was created (UTC)
    /// </summary>
    DateTimeOffset CreatedAt
);