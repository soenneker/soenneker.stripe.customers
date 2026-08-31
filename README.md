[![](https://img.shields.io/nuget/v/soenneker.stripe.customers.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.stripe.customers/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.stripe.customers/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.stripe.customers/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.stripe.customers.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.stripe.customers/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.stripe.customers/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.stripe.customers/actions/workflows/codeql.yml)

# ![](https://user-images.githubusercontent.com/4441470/224455560-91ed3ee7-f510-4041-a8d2-3fc093025112.png) Soenneker.Stripe.Customers

Creates, retrieves, searches, updates, and deletes Stripe customers, including mapping an internal user ID into metadata and managing the default invoice payment method.

## Installation

```bash
dotnet add package Soenneker.Stripe.Customers
```

## Configuration

```json
{
  "Stripe": {
    "SecretKey": "sk_test_..."
  }
}
```

## Usage

```csharp
using Soenneker.Stripe.Customers.Abstract;
using Soenneker.Stripe.Customers.Registrars;

services.AddStripeCustomersUtilAsSingleton();

var customer = await stripeCustomers.Create(
    email: "customer@example.com",
    name: "Example Customer",
    userId: "user-123",
    cancellationToken);

var sameCustomer = await stripeCustomers.GetByUserId(
    "user-123",
    cancellationToken);
```

`Create`, `Update`, payment-method setters, and delete methods change Stripe data. `DeleteAll()` deletes every customer visible to the configured account and should be reserved for deliberate cleanup workflows. Stripe API failures propagate as Stripe.net exceptions.
