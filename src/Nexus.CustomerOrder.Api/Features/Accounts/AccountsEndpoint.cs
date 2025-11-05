using Nexus.CustomerOrder.Api.Features.Accounts.AccountCreate;
using Nexus.CustomerOrder.Api.Features.Accounts.AccountsGet;

namespace Nexus.CustomerOrder.Api.Features.Accounts;

public static class AccountsEndpoint
{
    public static void MapAccounts(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts").WithTags("Accounts");

        group.MapCreateAccountEndpoint();
        //group.MapGetAccountEndpoint();
        //group.MapUpdateAccountEndpoint();
        group.MapGetAccountsEndpoint();
        //group.MapDeleteAccountEndpoint();
    }
}