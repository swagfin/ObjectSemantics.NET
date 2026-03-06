# Calculations

ObjectSemantics.NET supports lightweight calculation expressions directly in templates.

## Supported Functions

You can use function names with or without leading underscores.

| Function | Purpose | Example |
| --- | --- | --- |
| `sum(path)` | Sum numeric values in a path | `{{ __sum(Customer.Payments.Amount):N2 }}` |
| `avg(path)` | Average numeric values in a path | `{{ _avg(Customer.Payments.PaidAmount):N2 }}` |
| `count(path)` | Count non-null values in a path | `{{ __count(Customer.Payments.Amount) }}` |
| `min(path)` | Minimum numeric value in a path | `{{ __min(Customer.Payments.Amount):N2 }}` |
| `max(path)` | Maximum numeric value in a path | `{{ __max(Customer.Payments.Amount):N2 }}` |
| `calc(expr)` | Arithmetic over properties/literals | `{{ __calc(PaidAmount - Customer.CreditLimit):N2 }}` |

## Aggregates

Example model shape:

```csharp
Customer.Payments = [
  { Amount = 1000 },
  { Amount = 2000 },
  { Amount = 1500 }
];
```

Examples:

```text
{{ __sum(Customer.Payments.Amount) }}        -> 4500
{{ __avg(Customer.Payments.Amount):N2 }}     -> 1,500.00
{{ __count(Customer.Payments.Amount) }}      -> 3
{{ __min(Customer.Payments.Amount) }}        -> 1000
{{ __max(Customer.Payments.Amount) }}        -> 2000
```

## Arithmetic with `calc`

Operators:

- `+`
- `-`
- `*`
- `/`
- Parentheses `(...)`

Examples:

```text
{{ __calc(PaidAmount - Customer.CreditLimit):N2 }}
{{ __calc((Subtotal + Tax) * 0.5):N2 }}
{{ __calc(Quantity * UnitPrice) }}
```

## Works Inside Loops

```text
{{ #foreach(Items) }}
  [{{ Name }}={{ __calc(Quantity * UnitPrice):N2 }}]
{{ #endforeach }}
```

## Behavior Rules

| Scenario | Result |
| --- | --- |
| Valid numeric expression/path | Numeric output |
| Null source/property in math path | `0` |
| Unknown path/property in expression | Empty |
| Non-numeric data in numeric expression (e.g. string/date for `sum`) | Empty |
| Invalid expression syntax | Empty |

Notes:

- These rules apply to `sum`, `avg`, `count`, `min`, `max`, and `calc`.
- Standard placeholder behavior is different: unknown regular properties remain unresolved (`{{ UnknownProp }}`).

## Formatting Calculated Results

Calculation results can use normal format specifiers:

```text
{{ __sum(Customer.Payments.Amount):N2 }}
{{ __calc(PaidAmount - CreditLimit):#,##0 }}
```

Or no format at all:

```text
{{ __sum(Customer.Payments.Amount) }}
{{ __calc(PaidAmount - CreditLimit) }}
```
