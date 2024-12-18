using Soenneker.Stripe.Customers.Abstract;
using Soenneker.Tests.FixturedUnit;
using Xunit;


namespace Soenneker.Stripe.Customers.Tests;

[Collection("Collection")]
public class StripeCustomersUtilTests : FixturedUnitTest
{
    private readonly IStripeCustomersUtil _util;

    public StripeCustomersUtilTests(Fixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
        _util = Resolve<IStripeCustomersUtil>();
    }
}
