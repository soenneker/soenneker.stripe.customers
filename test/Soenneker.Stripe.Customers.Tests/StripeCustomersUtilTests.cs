using Soenneker.Stripe.Customers.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.Stripe.Customers.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public class StripeCustomersUtilTests : HostedUnitTest
{
    private readonly IStripeCustomersUtil _util;

    public StripeCustomersUtilTests(Host host) : base(host)
    {
        _util = Resolve<IStripeCustomersUtil>(true);
    }

    [Test]
    public void Default()
    {

    }
}
