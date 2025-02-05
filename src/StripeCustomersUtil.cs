using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Soenneker.Extensions.Enumerable;
using Soenneker.Extensions.Task;
using Soenneker.Extensions.ValueTask;
using Soenneker.Stripe.Client.Abstract;
using Soenneker.Stripe.Customers.Abstract;
using Soenneker.Utils.AsyncSingleton;
using Stripe;

namespace Soenneker.Stripe.Customers;

///<inheritdoc cref="IStripeCustomersUtil"/>
public class StripeCustomersUtil : IStripeCustomersUtil
{
    private readonly ILogger<StripeCustomersUtil> _logger;

    private readonly AsyncSingleton<CustomerService> _service;

    public StripeCustomersUtil(ILogger<StripeCustomersUtil> logger, IStripeClientUtil stripeUtil)
    {
        _logger = logger;

        _service = new AsyncSingleton<CustomerService>(async (cancellationToken, _) =>
        {
            StripeClient client = await stripeUtil.Get(cancellationToken).NoSync();

            return new CustomerService(client);
        });
    }

    public async ValueTask<Customer?> Create(string email, string name, string userId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(")) StripeCustomersUtil: Creating customer {email} ...", email);

        var options = new CustomerCreateOptions
        {
            Email = email,
            Name = name,
            Metadata = new Dictionary<string, string>
            {
                {"userId", userId}
            }
        };

        Customer? customer = await (await _service.Get(cancellationToken).NoSync()).CreateAsync(options, cancellationToken: cancellationToken).NoSync();

        _logger.LogDebug(")) StripeCustomersUtil: Created customer {email}", email);

        return customer;
    }

    public async ValueTask<List<Customer>?> GetAll(CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(")) StripeCustomersUtil: Getting all Stripe customers...");

        IAsyncEnumerable<Customer>? response = (await _service.Get(cancellationToken).NoSync()).ListAutoPagingAsync(cancellationToken: cancellationToken);

        if (response == null)
            return null;

        List<Customer> result = await response.ToListAsync(cancellationToken).NoSync();

        _logger.LogDebug(")) StripeCustomersUtil: Finished retrieving all Stripe customers");

        return result;
    }

    public async ValueTask<Customer?> Get(string id, CancellationToken cancellationToken = default)
    {
        Customer? result = await (await _service.Get(cancellationToken).NoSync()).GetAsync(id, cancellationToken: cancellationToken).NoSync();

        if (result == null)
        {
            _logger.LogWarning("Could not find Stripe customer from id ({customerId})", id);
            return null;
        }

        return result;
    }

    public async ValueTask<Customer?> GetByUserId(string userId, CancellationToken cancellationToken = default)
    {
        var options = new CustomerSearchOptions
        {
            Query = $"metadata[\"userId\"]:\"{userId}\""
        };

        StripeSearchResult<Customer>? stripeResponse = await (await _service.Get(cancellationToken).NoSync()).SearchAsync(options, cancellationToken: cancellationToken).NoSync();

        if (stripeResponse.Data.IsNullOrEmpty())
            return null;

        return stripeResponse.Data.FirstOrDefault();
    }

    public async ValueTask DeleteAll(CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(")) StripeCustomersUtil: Deleting all customers...");

        List<Customer>? customers = await GetAll(cancellationToken).NoSync();

        if (customers == null)
            return;

        foreach (Customer customer in customers)
        {
            await Delete(customer.Id, customer.Email, cancellationToken).NoSync();
        }

        _logger.LogWarning(")) StripeCustomersUtil: Finished deleting all customers");
    }

    public async ValueTask Delete(string stripeCustomerId, string email, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(")) StripeCustomersUtil: Deleting customer ({email}) ...", email);

        CustomerService service = await _service.Get(cancellationToken).NoSync();

        await service.DeleteAsync(stripeCustomerId, cancellationToken: cancellationToken).NoSync();
    }

    public async ValueTask DeleteByUserId(string userId, CancellationToken cancellationToken = default)
    {
        Customer? customer = await GetByUserId(userId, cancellationToken).NoSync();

        await Delete(customer.Id, customer.Email, cancellationToken).NoSync();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        _service.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        return _service.DisposeAsync();
    }
}