using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Stripe;

namespace Soenneker.Stripe.Customers.Abstract;

/// <summary>
/// A utility for working with Stripe Customer objects, including creation, retrieval, deletion, and managing default payment methods.
/// </summary>
public interface IStripeCustomersUtil : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Creates a new Stripe customer with the specified email, name, and user ID (stored in metadata).
    /// </summary>
    /// <param name="email">The customer's email address.</param>
    /// <param name="name">The customer's full name.</param>
    /// <param name="userId">A unique identifier for the user in your system, stored in metadata.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created <see cref="Customer"/> object, or null if creation failed.</returns>
    [Pure]
    ValueTask<Customer?> Create(string email, string name, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all Stripe customers from the account.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of <see cref="Customer"/> objects, or null if none are found.</returns>
    [Pure]
    ValueTask<List<Customer>?> GetAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a Stripe customer by their Stripe-assigned ID.
    /// </summary>
    /// <param name="id">The Stripe customer ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The <see cref="Customer"/> object if found, or null otherwise.</returns>
    [Pure]
    ValueTask<Customer?> Get(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for a Stripe customer using your system's user ID stored in metadata.
    /// </summary>
    /// <param name="userId">The user ID stored in customer metadata.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The matched <see cref="Customer"/> if found, or null.</returns>
    [Pure]
    ValueTask<Customer?> GetByUserId(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all customers in the Stripe account. Use with caution.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    ValueTask DeleteAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a Stripe customer by their Stripe ID and logs their email.
    /// </summary>
    /// <param name="id">The Stripe customer ID.</param>
    /// <param name="email">The email associated with the customer (for logging).</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    ValueTask Delete(string id, string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a Stripe customer by looking them up using your internal user ID stored in metadata.
    /// </summary>
    /// <param name="userId">The user ID stored in metadata.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    ValueTask DeleteByUserId(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the default payment method ID set in the customer's invoice settings.
    /// </summary>
    /// <param name="id">The Stripe customer ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The default payment method ID if set, or null.</returns>
    [Pure]
    ValueTask<string?> GetDefaultPaymentMethodId(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the customer's invoice settings to use the specified payment method as the default.
    /// </summary>
    /// <param name="id">The Stripe customer ID.</param>
    /// <param name="paymentMethodId">The Stripe payment method ID to set as default.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    ValueTask UpdateDefaultPaymentMethod(string id, string paymentMethodId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the default payment method from a customer's invoice settings.
    /// </summary>
    /// <param name="id">The Stripe customer ID.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    ValueTask DeleteDefaultPaymentMethod(string id, CancellationToken cancellationToken = default);
}