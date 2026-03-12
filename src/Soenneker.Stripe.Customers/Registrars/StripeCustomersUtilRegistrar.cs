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
    public static IServiceCollection AddStripeCustomersUtilAsSingleton(this IServiceCollection services)
    {
        services.AddStripeClientUtilAsSingleton().TryAddSingleton<IStripeCustomersUtil, StripeCustomersUtil>();

        return services;
    }

    /// <summary>
    /// Adds <see cref="IStripeCustomersUtil"/> as a scoped service. <para/>
    /// </summary>
    public static IServiceCollection AddStripeCustomersUtilAsScoped(this IServiceCollection services)
    {
        services.AddStripeClientUtilAsSingleton().TryAddScoped<IStripeCustomersUtil, StripeCustomersUtil>();

        return services;
    }
}