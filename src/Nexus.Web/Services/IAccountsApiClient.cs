using Nexus.CustomerOrder.Application.Features.Accounts.Models;
using Nexus.CustomerOrder.Application.Shared.Results;
using Refit;

namespace Nexus.Web.Services;

/// <summary>
/// Type-safe HTTP client for Accounts API using Refit
/// </summary>
public interface IAccountsApiClient
{
    [Get("/api/accounts")]
    Task<PagedResult<GetAccountDto>> GetAccountsAsync(
        [Query] int? take = null,
        [Query] string? continuationToken = null,
        CancellationToken cancellationToken = default);

    [Get("/api/accounts/{id}")]
    Task<GetAccountDto> GetAccountAsync(
        string id,
        CancellationToken cancellationToken = default);

    [Post("/api/accounts")]
    Task<CreateAccountResponseDto> CreateAccountAsync(
        [Body] CreateAccountDto account,
        CancellationToken cancellationToken = default);

    [Put("/api/accounts/{id}")]
    Task UpdateAccountAsync(
        string id,
        [Body] CreateAccountDto account,
        CancellationToken cancellationToken = default);

    [Delete("/api/accounts/{id}")]
    Task DeleteAccountAsync(
        string id,
        CancellationToken cancellationToken = default);
}