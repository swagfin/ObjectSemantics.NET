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

## Documentation
Detailed wiki files are available at the Wiki Page:
- [Go to Documentation](https://github.com/swagfin/ObjectSemantics.NET/wiki)

## Contributing
Contributions are welcome through issues and pull requests.

## License
[MIT](LICENSE)
