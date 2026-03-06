# ObjectSemantics.NET Wiki

Fast, readable object-to-template mapping for .NET applications.

ObjectSemantics.NET helps you build email, SMS, receipt, invoice, and notification templates from your domain objects with minimal code.

## Start Here

| Guide | Purpose |
| --- | --- |
| [Getting Started](Getting-Started.md) | Install, map first template, use options |
| [Template Syntax](Template-Syntax.md) | Placeholders, nested paths, loops, if/else, formatting |
| [Calculations](Calculations.md) | `sum`, `avg`, `count`, `min`, `max`, `calc` |
| [Recipes](Recipes.md) | Real-world email/message patterns |
| [Troubleshooting](Troubleshooting.md) | Fix unresolved values, empty outputs, formatting issues |

## Quick Example

```csharp
using ObjectSemantics.NET;

public class QuickCustomer
{
    public string Name { get; set; }
}

public class QuickOrder
{
    public string Number { get; set; }
    public QuickCustomer Customer { get; set; }
    public decimal Total { get; set; }
}

var order = new QuickOrder
{
    Number = "ORD-1001",
    Customer = new QuickCustomer { Name = "Jane Doe" },
    Total = 2900m
};

string template = "Order {{ Number }} for {{ Customer.Name }} is {{ Total:N2 }}";
string result = order.Map(template);
// Order ORD-1001 for Jane Doe is 2,900.00
```

## What You Can Do

- Map direct properties: `{{ Name }}`
- Map nested properties: `{{ Customer.BankingDetail.BankName }}`
- Loop collections: `{{ #foreach(Items) }}...{{ #endforeach }}`
- Evaluate conditions: `{{ #if(IsPaid == true) }}...{{ #endif }}`
- Format values: `{{ Amount:N2 }}`, `{{ Name:uppercase }}`
- Run calculations: `{{ __sum(Payments.Amount):N2 }}`, `{{ __calc(PaidAmount - CreditLimit):N2 }}`

## Notes

- Regular unknown placeholders stay unresolved so template authors can see them, for example: `{{ UnknownProp }}`.
- Calculation expressions (`sum/avg/count/min/max/calc`) return empty on invalid or unknown paths.
- Null sources/properties in calculation paths return zero.
