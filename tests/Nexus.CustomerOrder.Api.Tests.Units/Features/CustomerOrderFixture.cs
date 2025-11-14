using Nexus.CustomerOrder.Api.Tests.Units.Shared;

namespace Nexus.CustomerOrder.Api.Tests.Units.Features;

public class CustomerOrderFixture
{
    protected ApiFactory ApiFactory { get; }

    public CustomerOrderFixture()
    {
        ApiFactory = new ApiFactory(); // (new Dictionary<Type, object>());
    }

    public void ResetSubstitutes()
    {
        ApiFactory.ResetSubstitutes();
    }
}