using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Soenneker.Stripe.Client.Registrars;
using Soenneker.Stripe.Customers.Abstract;

namespace Soenneker.Stripe.Customers.Registrars;

/// <summary>
/// A .NET typesafe implementation of Stripe's Customers API
/// </summary>
public static class StripeCustomersUtilRegistrar
{
    /// <summary>
    /// Adds <see cref="IStripeCustomersUtil"/> as a singleton service. <para/>
    /// </summary>
    public static void AddStripeCustomersUtilAsSingleton(this IServiceCollection services)
    {
        services.AddStripeClientUtilAsSingleton();
        services.TryAddSingleton<IStripeCustomersUtil, StripeCustomersUtil>();
    }

    /// <summary>
    /// Adds <see cref="IStripeCustomersUtil"/> as a scoped service. <para/>
    /// </summary>
    public static void AddStripeCustomersUtilAsScoped(this IServiceCollection services)
    {
        services.AddStripeClientUtilAsSingleton();
        services.TryAddScoped<IStripeCustomersUtil, StripeCustomersUtil>();
    }
}
