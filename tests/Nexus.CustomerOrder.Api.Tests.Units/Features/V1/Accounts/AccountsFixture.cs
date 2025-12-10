using Nexus.CustomerOrder.Application.Features.Accounts.Ports;
using NSubstitute;

namespace Nexus.CustomerOrder.Api.Tests.Units.Features.V1.Accounts;

public class AccountsFixture : CustomerOrderFixture
{
    public IAccountRepository AccountRepository = Substitute.For<IAccountRepository>();
    public HttpClient HttpClient { get; }

    public AccountsFixture()
    {
        HttpClient = ApiFactory
            .Mock(AccountRepository)
            .CreateClient();
    }

    public static string GetUrl(string id) => $"/api/accounts/{id}";
    public static string CreateUrl() => "/api/accounts/";
    public static string GetAccountsUrl(string? queryString = null) => 
        string.IsNullOrEmpty(queryString) ? "/api/accounts/" : $"/api/accounts/{queryString}";
}