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

        StripeSearchResult<Customer>? stripeResponse =
            await (await _service.Get(cancellationToken).NoSync()).SearchAsync(options, cancellationToken: cancellationToken).NoSync();

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

    public async ValueTask Delete(string id, string email, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning(")) StripeCustomersUtil: Deleting customer ({email}) ...", email);

        CustomerService service = await _service.Get(cancellationToken).NoSync();

        await service.DeleteAsync(id, cancellationToken: cancellationToken).NoSync();
    }

    public async ValueTask DeleteByUserId(string userId, CancellationToken cancellationToken = default)
    {
        Customer? customer = await GetByUserId(userId, cancellationToken).NoSync();

        await Delete(customer.Id, customer.Email, cancellationToken).NoSync();
    }

    public async ValueTask<Customer?> Update(Customer customer, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(")) StripeCustomersUtil: Updating customer {id} ...", customer.Id);

        var options = new CustomerUpdateOptions
        {
            Email = customer.Email,
            Name = customer.Name,
            Description = customer.Description,
            Metadata = customer.Metadata,
            Phone = customer.Phone,
            InvoicePrefix = customer.InvoicePrefix,
            PreferredLocales = customer.PreferredLocales?.ToList(),
            TaxExempt = customer.TaxExempt,
            Tax = customer.Tax is null
                ? null
                : new CustomerTaxOptions
                {
                    IpAddress = customer.Tax.IpAddress
                },
            Address = customer.Address is null
                ? null
                : new AddressOptions
                {
                    City = customer.Address.City,
                    Country = customer.Address.Country,
                    Line1 = customer.Address.Line1,
                    Line2 = customer.Address.Line2,
                    PostalCode = customer.Address.PostalCode,
                    State = customer.Address.State
                },
            Shipping = customer.Shipping is null
                ? null
                : new ShippingOptions
                {
                    Name = customer.Shipping.Name,
                    Phone = customer.Shipping.Phone,
                    Address = customer.Shipping.Address is null
                        ? null
                        : new AddressOptions
                        {
                            City = customer.Shipping.Address.City,
                            Country = customer.Shipping.Address.Country,
                            Line1 = customer.Shipping.Address.Line1,
                            Line2 = customer.Shipping.Address.Line2,
                            PostalCode = customer.Shipping.Address.PostalCode,
                            State = customer.Shipping.Address.State
                        }
                },
            InvoiceSettings = customer.InvoiceSettings is null
                ? null
                : new CustomerInvoiceSettingsOptions
                {
                    CustomFields = customer.InvoiceSettings.CustomFields?.Select(cf => new CustomerInvoiceSettingsCustomFieldOptions
                                           {
                                               Name = cf.Name,
                                               Value = cf.Value
                                           })
                                           .ToList(),
                    DefaultPaymentMethod = customer.InvoiceSettings.DefaultPaymentMethodId,
                    Footer = customer.InvoiceSettings.Footer,
                    RenderingOptions = customer.InvoiceSettings.RenderingOptions is null
                        ? null
                        : new CustomerInvoiceSettingsRenderingOptionsOptions
                        {
                            AmountTaxDisplay = customer.InvoiceSettings.RenderingOptions.AmountTaxDisplay
                        }
                }
        };

        CustomerService service = await _service.Get(cancellationToken).NoSync();

        Customer updated = await service.UpdateAsync(customer.Id, options, cancellationToken: cancellationToken).NoSync();

        _logger.LogInformation(")) StripeCustomersUtil: Updated customer {id}", customer.Id);

        return updated;
    }

    public async ValueTask<string?> GetDefaultPaymentMethodId(string id, CancellationToken cancellationToken = default)
    {
        CustomerService service = await _service.Get(cancellationToken).NoSync();
        Customer customer = await service.GetAsync(id, cancellationToken: cancellationToken).NoSync();

        return customer?.InvoiceSettings?.DefaultPaymentMethodId;
    }

    public async ValueTask UpdateDefaultPaymentMethod(string id, string paymentMethodId, CancellationToken cancellationToken = default)
    {
        CustomerService service = await _service.Get(cancellationToken).NoSync();

        var updateOptions = new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = paymentMethodId
            }
        };

        await service.UpdateAsync(id, updateOptions, cancellationToken: cancellationToken).NoSync();
    }

    public async ValueTask DeleteDefaultPaymentMethod(string id, CancellationToken cancellationToken = default)
    {
        CustomerService service = await _service.Get(cancellationToken).NoSync();

        var updateOptions = new CustomerUpdateOptions
        {
            InvoiceSettings = new CustomerInvoiceSettingsOptions
            {
                DefaultPaymentMethod = null
            }
        };

        await service.UpdateAsync(id, updateOptions, cancellationToken: cancellationToken).NoSync();
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