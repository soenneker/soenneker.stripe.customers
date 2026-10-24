using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Stripe.Client;
using Soenneker.Stripe.Client.Abstract;
using Soenneker.Stripe.OpenApiClient.Models;
using Soenneker.Stripe.Customers.Abstract;

namespace Soenneker.Stripe.Customers;

public sealed class StripeCustomersUtil(IStripeClientUtil client) : IStripeCustomersUtil
{
    public async ValueTask<Customer?> Create(string email, string name, string userId, CancellationToken cancellationToken = default)
    {
        var api = await client.Get(cancellationToken);
        var options = new PostCustomersXWwwFormUrlencodedRequest { Email = email, Name = name };
        options.Metadata = new() { PostCustomersXWwwFormUrlencodedRequestMetadataAnyOf1 = new() { AdditionalData = new Dictionary<string, object> { ["userId"] = userId } } };
        return await api.V1.Customers.PostAsync(options, cancellationToken: cancellationToken);
    }

    public async ValueTask<List<Customer>?> GetAll(CancellationToken cancellationToken = default)
    {
        var api = await client.Get(cancellationToken);
        var result = new List<Customer>();
        string? cursor = null;
        do
        {
            var page = await api.V1.Customers.GetAsync(new(), c => { c.QueryParameters.Limit = 100; c.QueryParameters.StartingAfter = cursor; }, cancellationToken);
            if (page?.Data is not { Count: > 0 }) break;
            result.AddRange(page.Data);
            cursor = page.HasMore == true ? page.Data[^1].Id : null;
        } while (cursor != null);
        return result;
    }

    public async ValueTask<Customer?> Get(string id, CancellationToken cancellationToken = default)
    {
        var api = await client.Get(cancellationToken);
        return (await api.V1.Customers[id].GetAsync(new(), cancellationToken: cancellationToken))?.Customer;
    }

    public async ValueTask<Customer?> GetByUserId(string userId, CancellationToken cancellationToken = default)
    {
        var api = await client.Get(cancellationToken);
        var page = await api.V1.Customers.Search.GetAsync(new(), c => c.QueryParameters.Query = $"metadata[\"userId\"]:\"{userId}\"", cancellationToken);
        return page?.Data?.FirstOrDefault();
    }

    public async ValueTask Delete(string id, string email, CancellationToken cancellationToken = default)
    {
        var api = await client.Get(cancellationToken);
        await api.V1.Customers[id].DeleteAsync(new(), cancellationToken: cancellationToken);
    }

    public async ValueTask DeleteAll(CancellationToken cancellationToken = default)
    {
        foreach (var customer in await GetAll(cancellationToken) ?? [])
            await Delete(customer.Id!, customer.Email ?? "", cancellationToken);
    }

    public async ValueTask DeleteByUserId(string userId, CancellationToken cancellationToken = default)
    {
        var customer = await GetByUserId(userId, cancellationToken);
        if (customer?.Id is not null) await Delete(customer.Id, customer.Email ?? "", cancellationToken);
    }

    public async ValueTask<Customer?> Update(Customer customer, CancellationToken cancellationToken = default)
    {
        var options = StripeModelUtil.SelectFields(customer, PostCustomersCustomerXWwwFormUrlencodedRequest.CreateFromDiscriminatorValue,
            "email", "name", "description", "metadata", "phone", "invoice_prefix", "preferred_locales", "tax_exempt", "address", "shipping");
        if (customer.InvoiceSettings != null)
        {
            options.InvoiceSettings = StripeModelUtil.SelectFields(customer.InvoiceSettings,
                PostCustomersCustomerXWwwFormUrlencodedRequestInvoiceSettings.CreateFromDiscriminatorValue,
                "custom_fields", "footer", "rendering_options");
            options.InvoiceSettings.DefaultPaymentMethod = customer.InvoiceSettings.DefaultPaymentMethod.GetId();
        }
        var api = await client.Get(cancellationToken);
        return await api.V1.Customers[customer.Id!].PostAsync(options, cancellationToken: cancellationToken);
    }

    public async ValueTask<string?> GetDefaultPaymentMethodId(string id, CancellationToken cancellationToken = default) =>
        (await Get(id, cancellationToken))?.InvoiceSettings?.DefaultPaymentMethod.GetId();

    public async ValueTask UpdateDefaultPaymentMethod(string id, string paymentMethodId, CancellationToken cancellationToken = default)
    {
        var api = await client.Get(cancellationToken);
        var options = new PostCustomersCustomerXWwwFormUrlencodedRequest { InvoiceSettings = new() };
        options.InvoiceSettings.DefaultPaymentMethod = paymentMethodId;
        await api.V1.Customers[id].PostAsync(options, cancellationToken: cancellationToken);
    }

    public ValueTask DeleteDefaultPaymentMethod(string id, CancellationToken cancellationToken = default) => UpdateDefaultPaymentMethod(id, "", cancellationToken);
    public void Dispose() { }
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
