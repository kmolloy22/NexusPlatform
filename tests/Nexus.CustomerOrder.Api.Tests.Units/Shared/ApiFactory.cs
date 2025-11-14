using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexus.Shared.Core.Tests.Logging;
using NSubstitute;
using NSubstitute.ClearExtensions;
using NSubstitute.Core;

namespace Nexus.CustomerOrder.Api.Tests.Units.Shared;

/// <summary>
/// Test web application factory for API integration/unit style tests.
/// Allows replacing registered services with NSubstitute mocks before the
/// ASP.NET Minimal API host is built, and configures a test logger.
/// </summary>
/// <remarks>
/// Usage:
/// 1. Instantiate: <c>var factory = new ApiFactory().Mock<IAccountRepository>()</c>
/// 2. Optionally chain multiple <see cref="Mock{T}"/> calls.
/// 3. Call <c>CreateClient()</c> to obtain an <see cref="HttpClient"/> wired to the in‑memory server.
/// 4. Use <see cref="ResetSubstitutes"/> between tests if you reuse the factory instance to clear recorded calls.
/// </remarks>
public class ApiFactory : WebApplicationFactory<Program>
{
    // Holds service type -> mock instance mappings to be injected.
    private readonly Dictionary<Type, object> _mocks = new Dictionary<Type, object>();

    // Captured test logger implementation; injected into the host logging pipeline.
    private ITestLogger _logger;

    /// <summary>
    /// Initializes the factory with a default <see cref="TestLogger"/>.
    /// </summary>
    public ApiFactory()
    {
        AddLogger(new TestLogger());
    }

    /// <summary>
    /// Creates a NSubstitute mock for <typeparamref name="T"/> and registers it
    /// to replace the application's original service during host configuration.
    /// </summary>
    /// <typeparam name="T">The service interface/class to mock.</typeparam>
    /// <returns>The current <see cref="ApiFactory"/> to allow fluent chaining.</returns>
    public ApiFactory Mock<T>() where T : class
    {
        var mock = Substitute.For<T>();
        return Mock(mock);
    }

    /// <summary>
    /// Registers a supplied mock instance for <typeparamref name="T"/> so that
    /// when the test host is built the existing service descriptor (if present) is removed
    /// and replaced by this singleton instance.
    /// </summary>
    /// <typeparam name="T">The service type being replaced.</typeparam>
    /// <param name="mock">Your preconfigured mock/substitute instance.</param>
    /// <returns>The current <see cref="ApiFactory"/> for chaining.</returns>
    public ApiFactory Mock<T>(T mock) where T : class
    {
        _mocks[typeof(T)] = mock;
        return this;
    }

    /// <summary>
    /// Assigns a custom test logger implementation that will be wired as a provider.
    /// Call before <c>CreateClient()</c>.
    /// </summary>
    /// <param name="logger">Logger collecting emitted log statements in tests.</param>
    /// <returns>The current <see cref="ApiFactory"/> instance.</returns>
    public ApiFactory AddLogger(ITestLogger logger)
    {
        _logger = logger;
        return this;
    }

    /// <summary>
    /// Overrides web host configuration to:
    /// 1. Replace registered services with provided mocks.
    /// 2. Remove <see cref="BackgroundService"/> registrations (avoids side effects / delays in tests).
    /// 3. Replace logging providers with a deterministic test logger + debug output.
    /// </summary>
    /// <param name="builder">The host builder supplied by the test framework.</param>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Apply each mock: remove existing descriptor if it exists; then add singleton of mock.
            foreach (var mock in _mocks)
            {
                var descriptor = services.FirstOrDefault(p => p.ServiceType == mock.Key);
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton(mock.Key, mock.Value);
            }

            // Remove hosted background services (e.g., log dispatchers, timers) to keep tests fast & isolated.
            var hostedServices = services
                .Where(sd => sd.ImplementationType != null)
                .Where(sd => typeof(BackgroundService).IsAssignableFrom(sd.ImplementationType))
                .ToArray();

            foreach (var serviceDescriptor in hostedServices)
            {
                services.Remove(serviceDescriptor);
            }
        });

        // Configure logging only if a test logger was provided.
        if (_logger != null)
        {
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddProvider(new TestLoggerProvider(_logger));
                logging.AddDebug(); // Optional additional visibility during local runs.
            });
        }
    }

    /// <summary>
    /// Clears recorded calls on all injected NSubstitute mocks that implement <see cref="ICallRouterProvider"/>.
    /// Use this if the same <see cref="ApiFactory"/> instance serves multiple tests to avoid call history bleed.
    /// </summary>
    public void ResetSubstitutes()
    {
        foreach (var injectedMock in _mocks.Values)
        {
            if (injectedMock is ICallRouterProvider provider)
                provider.ClearSubstitute();
        }
    }
}

//internal class InMemoryAccountRepository : IAccountRepository
//{
//    private readonly ConcurrentDictionary<string, AccountTableEntity> _store = new();

//    public Task AddAsync(Account account, CancellationToken cancellationToken = default)
//    {
//        var entity = new AccountTableEntity
//        {
//            RowKey = account.Id.ToString("N"),
//            FirstName = account.FirstName,
//            LastName = account.LastName,
//            Email = account.Email,
//            PhoneNumber = account.Phone,
//            Address_Street1 = account.Address.Street1,
//            Address_Street2 = account.Address.Street2,
//            Address_City = account.Address.City,
//            Address_State = account.Address.State,
//            Address_PostalCode = account.Address.PostalCode,
//            Address_Country = account.Address.Country
//        };
//        _store[entity.RowKey] = entity;
//        return Task.CompletedTask;
//    }

//    public Task<AccountTableEntity?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
//        => Task.FromResult(_store.TryGetValue(id, out var e) ? e : null);

//    public Task<bool> UpdateAsync(string id, string firstName, string lastName, string? email, string? phone, Address address, CancellationToken cancellationToken = default)
//    {
//        if (!_store.TryGetValue(id, out var e)) return Task.FromResult(false);
//        e.FirstName = firstName;
//        e.LastName = lastName;
//        e.Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
//        e.PhoneNumber = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
//        e.Address_Street1 = address.Street1;
//        e.Address_Street2 = address.Street2;
//        e.Address_City = address.City;
//        e.Address_State = address.State;
////        e.Address_PostalCode = address.PostalCode;
//        e.Address_Country = address.Country;
//        return Task.FromResult(true);
//    }

//    public Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
//        => Task.FromResult(_store.TryRemove(id, out _));

//    public Task<Page<AccountTableEntity>?> QueryAsync(int pageSize, string? continuationToken = null, CancellationToken cancellationToken = default)
//    {
//        var items = _store.Values.Take(pageSize).ToList();
//        // Smoke tests don’t need real continuation semantics
//        var page = Page<AccountTableEntity>.FromValues(items, null, response: null);
//        return Task.FromResult<Page<AccountTableEntity>?>(page);
//    }
//}