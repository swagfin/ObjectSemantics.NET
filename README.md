# ObjectSemantics.NET
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET?ref=badge_shield)

Object-to-template mapping for .NET with nested property support, loops, conditions, formatting, and lightweight calculations.

## Why ObjectSemantics.NET
Use it when you need fast, readable template mapping for:
- Email and SMS templates
- Receipts, invoices, and reports
- Notification payloads
- Config and log rendering

## Features
- Direct property mapping: `{{ Name }}`
- Nested property mapping: `{{ Customer.BankingDetail.BankName }}`
- Collection loops: `{{ #foreach(Items) }}...{{ #endforeach }}`
- Conditional blocks: `{{ #if(Age >= 18) }}...{{ #else }}...{{ #endif }}`
- Built-in formatting for number/date/string
- XML escaping option (`XmlCharEscaping = true`)
- Calculation functions:
  - `sum`, `avg`, `count`, `min`, `max`
  - `calc` arithmetic expressions
  - Function names accept optional leading underscores: `_sum(...)`, `__calc(...)`

## Installation
Install from [NuGet](https://www.nuget.org/packages/ObjectSemantics.NET):

```powershell
Install-Package ObjectSemantics.NET
```

## Quick Start
```csharp
using ObjectSemantics.NET;

var person = new Person { Name = "John Doe" };
string output = person.Map("Hello {{ Name }}");
// Hello John Doe
```

## Template Examples
### Nested mapping
```csharp
var payment = new CustomerPayment
{
    Amount = 100000000,
    Customer = new Customer
    {
        CompanyName = "CRUDSOFT TECHNOLOGIES"
    }
};

string result = payment.Map("Paid Amount: {{ Amount:N2 }} By {{ Customer.CompanyName }}");
// Paid Amount: 100,000,000.00 By CRUDSOFT TECHNOLOGIES
```

### Loop + format
```csharp
string template = "{{ #foreach(Items) }}[{{ Quantity }}x{{ Name }}={{ LineTotal:N2 }}]{{ #endforeach }}";
```

### Condition
```csharp
string template = "{{ #if(IsPaid == true) }}PAID{{ #else }}UNPAID{{ #endif }}";
```

### Calculations
```csharp
// Aggregates
"{{ __sum(Customer.Payments.Amount):N2 }}"
"{{ _avg(Customer.Payments.PaidAmount):N2 }}"
"{{ __count(Customer.Payments.Amount) }}"

// Arithmetic expression
"{{ __calc(PaidAmount - Customer.CreditLimit):N2 }}"
```

Calculation behavior:
- Null source/property in math path returns zero
- Unknown property/path returns empty
- Invalid/non-numeric math expression returns empty

## Documentation
Detailed wiki files are available in-repo:
- [Wiki Home](wiki/Home.md)
- [Getting Started](wiki/Getting-Started.md)
- [Template Syntax](wiki/Template-Syntax.md)
- [Calculations](wiki/Calculations.md)
- [Real-World Recipes](wiki/Recipes.md)
- [Troubleshooting](wiki/Troubleshooting.md)

These files can be copied directly into your GitHub Wiki repository.

## Validation
Current test suite covers:
- Nested object mapping
- Conditions and loops
- String/number/date formatting
- File template mapping
- Real-world email and messaging scenarios
- Expression functions and edge cases

## Contributing
Contributions are welcome through issues and pull requests.

## License
[MIT](LICENSE)
