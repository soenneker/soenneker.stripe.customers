using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Stripe;

namespace Soenneker.Stripe.Customers.Abstract;

/// <summary>
/// A .NET typesafe implementation of Stripe's Customers API.
/// </summary>
public interface IStripeCustomersUtil : IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Creates a new customer in Stripe with the specified email, name, and user identifier.
    /// </summary>
    /// <param name="email">The email address of the customer.</param>
    /// <param name="name">The name of the customer.</param>
    /// <param name="userId">Your system's unique identifier for the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous create operation. The task result contains the created <see cref="Customer"/>, or <c>null</c> if creation failed.
    /// </returns>
    ValueTask<Customer?> Create(string email, string name, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all customers from Stripe. Be cautious as this operation may return a large number of customers.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous retrieval operation. The task result contains a list of <see cref="Customer"/> objects, or <c>null</c> if retrieval failed.
    /// </returns>
    [Pure]
    ValueTask<List<Customer>?> GetAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a customer from Stripe using your system's user identifier (not the Stripe customer ID).
    /// </summary>
    /// <param name="userId">Your system's unique identifier for the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous retrieval operation. The task result contains the <see cref="Customer"/>, or <c>null</c> if not found.
    /// </returns>
    [Pure]
    ValueTask<Customer?> GetByUserId(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a customer from Stripe using the Stripe customer ID.
    /// </summary>
    /// <param name="id">The Stripe customer ID.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task that represents the asynchronous retrieval operation. The task result contains the <see cref="Customer"/>, or <c>null</c> if not found.
    /// </returns>
    [Pure]
    ValueTask<Customer?> Get(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all customers from Stripe. Use with caution as this operation cannot be undone.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    ValueTask DeleteAll(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a customer from Stripe using your system's user identifier.
    /// </summary>
    /// <param name="userId">Your system's unique identifier for the user.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    ValueTask DeleteByUserId(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a customer from Stripe using the Stripe customer ID and email.
    /// </summary>
    /// <param name="stripeCustomerId">The Stripe customer ID.</param>
    /// <param name="email">The email address of the customer.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    ValueTask Delete(string stripeCustomerId, string email, CancellationToken cancellationToken = default);
}