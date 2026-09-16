<div align="center">

# ObjectSemantics.NET

### Your objects. Your templates. Messages that feel personal.

Turn everyday .NET data into payment reminders, order confirmations, receipts, and reports—with a small, readable template language.

[![NuGet](https://img.shields.io/nuget/v/ObjectSemantics.NET.svg)](https://www.nuget.org/packages/ObjectSemantics.NET)
[![Target](https://img.shields.io/badge/.NET%20Standard-2.0-512BD4)](ObjectSemantics.NET/ObjectSemantics.NET.csproj)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2Fswagfin%2FObjectSemantics.NET?ref=badge_shield)

**Nested properties · Loops · Conditions · Calculations · Reusable templates**

[Get started](#get-started) · [Everyday examples](#everyday-examples) · [Performance](#performance) · [Production controls](#production-controls)

</div>

---

A customer has an outstanding balance. An order has five line items. A receipt needs a different message when payment is complete.

ObjectSemantics.NET lets you express those messages in templates and fill them from your existing objects. Keep the wording separate from application code, format values where they appear, and reuse the same template for the next customer.

```text
Hi {{ Name }}, your balance is KES {{ Balance:N2 }}. Due {{ DueDate:dd MMM }}.
```

**Becomes:**

```text
Hi Amina, your balance is KES 4,500.00. Due 05 Oct.
```

| When you need to… | The library gives you… |
|---|---|
| Personalize an SMS or email | Readable placeholders mapped to object properties |
| Build a receipt with a changing number of items | Collection loops and per-item formatting |
| Show “Paid” or “Payment due” | Conditional blocks, including nested conditions |
| Calculate a subtotal or display a balance | Aggregates and arithmetic expressions |
| Generate the same message for many customers | Cached parsing and reusable compiled templates |
| Catch mistakes before sending | Syntax diagnostics and optional strict rendering |

The core library targets **.NET Standard 2.0** and has **no explicit third-party package dependencies**. It renders text that your application can send, save, or display.

## Get started

```sh
dotnet add package ObjectSemantics.NET
```

Create a model, write a template, and call `Map()`:

```csharp
using System;
using ObjectSemantics.NET;

Customer customer = new Customer
{
    Name = "Amina",
    Balance = 4500m,
    DueDate = new DateTime(2026, 10, 5)
};

string template = "Hi {{ Name }}, your balance is KES {{ Balance:N2 }}. Due {{ DueDate:dd MMM }}.";
string message = customer.Map(template);

Console.WriteLine(message);

public class Customer
{
    public string Name { get; set; }
    public decimal Balance { get; set; }
    public DateTime DueDate { get; set; }
}
```

Prefer starting from the template? Both forms work:

```csharp
string message = customer.Map("Hello {{ Name }}!");
string sameMessage = "Hello {{ Name }}!".Map(customer);
```

## Everyday examples

### 1. Send the right payment reminder

Use the `customer` from the quick start. One template handles both outstanding balances and settled accounts:

```csharp
string template = "{{ #if(Balance > 0) }}Hi {{ Name }}, please pay KES {{ Balance:N2 }} by {{ DueDate:dd MMM }}.{{ #else }}Thanks {{ Name }}! Your account is fully paid.{{ #endif }}";
string message = customer.Map(template);
```

```text
Hi Amina, please pay KES 4,500.00 by 05 Oct.
```

Set `customer.Balance` to `0m`, and the same template produces:

```text
Thanks Amina! Your account is fully paid.
```

### 2. Turn an order into a receipt

Read the customer's name from a nested object, loop over purchased items, calculate each line, and add up the total.

```csharp
using System.Collections.Generic;

SalesOrder order = new SalesOrder
{
    Number = "ORD-1042",
    Customer = customer,
    IsPaid = true,
    Items = new List<OrderItem>
    {
        new OrderItem { Name = "Notebook", Quantity = 2, UnitPrice = 250m },
        new OrderItem { Name = "Pen", Quantity = 3, UnitPrice = 50m }
    }
};

string template = @"Order {{ Number }} for {{ Customer.Name }}
{{ #foreach(Items) }}- {{ Quantity }} x {{ Name }}: KES {{ __calc(Quantity * UnitPrice):N2 }}
{{ #endforeach }}Total: KES {{ __sum(Items.LineTotal):N2 }}
{{ #if(IsPaid == true) }}Paid. Thank you for shopping with us!{{ #else }}Payment is due on collection.{{ #endif }}";

string receipt = order.Map(template);
```

```text
Order ORD-1042 for Amina
- 2 x Notebook: KES 500.00
- 3 x Pen: KES 150.00
Total: KES 650.00
Paid. Thank you for shopping with us!
```

<details>
<summary><strong>The order models</strong></summary>

Reuse the `Customer` class from the quick start.

```csharp
public class SalesOrder
{
    public string Number { get; set; }
    public Customer Customer { get; set; }
    public bool IsPaid { get; set; }
    public List<OrderItem> Items { get; set; }
}

public class OrderItem
{
    public string Name { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get { return Quantity * UnitPrice; } }
}
```

</details>

### 3. Add campaign details without changing your model

Supply extra values for a branch name, support number, campaign code, or other information that belongs to the message.

```csharp
Dictionary<string, object> extras = new Dictionary<string, object>
{
    ["BranchName"] = "Westlands",
    ["SupportNumber"] = "+254 700 000 001"
};

string template = "Hi {{ Name }}, your {{ BranchName }} team is here to help. Call {{ SupportNumber }}.";
string message = customer.Map(template, extras);
```

```text
Hi Amina, your Westlands team is here to help. Call +254 700 000 001.
```

Extra keys must be unique and must not duplicate model property names. Matching is case-insensitive.

## The template language at a glance

| Task | Example |
|---|---|
| Read a property | `{{ Name }}` |
| Read a nested property | `{{ Customer.Name }}` |
| Format a number | `{{ Balance:N2 }}` |
| Format a date | `{{ DueDate:dd MMM yyyy }}` |
| Change text case | `{{ Name:uppercase }}` |
| Loop over objects | `{{ #foreach(Items) }}{{ Name }}{{ #endforeach }}` |
| Loop over scalar values | `{{ #foreach(Tags) }}{{ . }} {{ #endforeach }}` |
| Choose a message | `{{ #if(IsPaid == true) }}Paid{{ #else }}Due{{ #endif }}` |
| Sum projected values | `{{ __sum(Items.LineTotal):N2 }}` |
| Find an average | `{{ __avg(Items.UnitPrice):N2 }}` |
| Count non-null projected values | `{{ __count(Items.Name) }}` |
| Find a minimum or maximum | `{{ __min(Items.UnitPrice) }}` / `{{ __max(Items.UnitPrice) }}` |
| Calculate a value | `{{ __calc(Quantity * UnitPrice):N2 }}` |

Conditions support `==`, `!=`, `>`, `>=`, `<`, and `<=`. Arithmetic supports `+`, `-`, `*`, `/`, parentheses, and unary minus. Loops and conditions can be nested; inside a loop, properties refer to the current item.

Number and date formatting use invariant culture. String commands include `uppercase`, `lowercase`, `titlecase`, `length`, `tobase64`, and `frombase64`. Title casing uses the current culture.

## Compile once. Personalize repeatedly.

For a notification worker or a recurring report, retain the prepared template and supply a different model for each render:

```csharp
CompiledTemplate reminder = TemplateMapper.Compile("Hi {{ Name }}, your balance is KES {{ Balance:N2 }}.");

foreach (Customer recipient in customers)
{
    string message = reminder.Render(recipient);
    // Pass the message to your application's email or SMS service.
}
```

Here, `customers` is your application's collection of `Customer` objects. Compiled templates can be shared across concurrent renders with independent models and writers. They retain template structure, not customer values.

For large reports, write directly to a `TextWriter`:

```csharp
reminder.RenderTo(writer, customer);
```

The caller supplies and owns `writer`. It remains open after rendering. A failed render can leave partial output, so use `Render()` when you need a complete string before writing anything.

> The compilation, validation, and rendering controls below describe the current repository implementation. They may require building from source until the corresponding NuGet release is published.

## Performance

**Prepare the template once. Spend subsequent renders filling it with data.**

The engine parses templates into structured nodes, prepares arithmetic expressions and property paths, caches property getters, and writes output in order. `Map()` automatically uses a bounded template cache; `Compile()` lets your application retain a template directly.

Measured locally with BenchmarkDotNet against the earlier renderer:

| Workload | Earlier renderer | Updated renderer | Allocation change |
|---|---:|---:|---:|
| 200 repeated placeholders | 7.77 ms | **4.07 µs** | 37.61 KB → **14.10 KB** |
| Arithmetic across 100 rows | 38.59 µs | **16.45 µs** | 227.13 KB → **87.59 KB** |

The repeated-placeholder case exposes a particularly expensive path in the earlier renderer. These are specific workload results, not a general speed multiplier or a comparison with other libraries.

<details>
<summary><strong>Measurement details and how to reproduce</strong></summary>

Measurements used BenchmarkDotNet 0.15.8, its ShortRun job (one launch, three warmups, three measured iterations), an Apple M5 Pro, macOS 27.0, and .NET 10.0.8. The earlier library was revision `e463bda`, measured with the same two workload definitions before the engine changes. Results are rounded; KB means 1,024 bytes. Parser validation and whitespace compatibility received additional review after the measurements.

Additional observations from the updated renderer:

| Workload | Mean | Allocated per operation |
|---|---:|---:|
| Retained compiled template, 200 placeholders | 3.52 µs | 14,288 B |
| Same template written to `TextWriter.Null` | 2.49 µs | 528 B |
| Two aggregates over 100 items, default evaluation | 4.93 µs | 12,448 B |
| Same aggregates, lazy reads and streaming enabled | 4.44 µs | 7,792 B |

Writer measurements exclude transport costs. Short runs are directional measurements; use representative application load tests to assess throughput and tail latency.

```sh
dotnet run --project ObjectSemantics.NET.Benchmarks -c Release -- --filter '*' --job short
```

Omit `--job short` for a longer BenchmarkDotNet run. The suite covers warm rendering, arithmetic loops, aggregates, 4,096-template cache churn, cold parsing, compiled templates, writer output, and concurrency. Cold parsing excludes process startup and first-JIT costs; concurrent measurements include scheduling overhead.

</details>

## Production controls

### Catch template mistakes early

Validate syntax before saving or publishing a template:

```csharp
IReadOnlyList<TemplateDiagnostic> diagnostics = TemplateMapper.Validate("Hello {{ Name");

foreach (TemplateDiagnostic diagnostic in diagnostics)
{
    Console.WriteLine($"Line {diagnostic.Line}, column {diagnostic.Column}: {diagnostic.Message}");
}
```

Validation reports syntax problems without executing model getters. Enable `StrictMode` during rendering to throw when an executed expression cannot be resolved. Missing values in an unselected branch are not evaluated.

### Set a budget for each render

```csharp
TemplateMapperOptions options = new TemplateMapperOptions
{
    StrictMode = true,
    XmlCharEscaping = true,
    MaximumTemplateCharacters = 100000,
    MaximumNestingDepth = 32,
    MaximumIterations = 10000,
    MaximumOutputCharacters = 1000000
};

string message = customer.Map("Hello {{ Name }}", options: options);
```

The limits above are examples to tune for your workload. You can also supply `CancellationToken`. `XmlCharEscaping` escapes special characters in formatted values; for example, `Amina & Sons` becomes `Amina &amp; Sons`.

| Option | Default | Purpose |
|---|---|---|
| `StrictMode` | `false` | Reject syntax diagnostics and unresolved expressions during rendering |
| `XmlCharEscaping` | `false` | Escape XML special characters in inserted values |
| `MaximumNestingDepth` | `128` | Bound block nesting and recursive collection traversal |
| `MaximumTemplateCharacters` | `0` | Bound template source length |
| `MaximumIterations` | `0` | Bound total enumerated items across loops, conditions, and expressions |
| `MaximumOutputCharacters` | `0` | Bound written output length |
| `LazyPropertyAccess` | `false` | Read only requested top-level properties, once per scope |
| `UseStreamingEvaluation` | `false` | Traverse aggregate paths without intermediate lists |

Zero disables a limit. Character limits count UTF-16 characters. Negative limits are rejected. Options passed to `Compile()` validate compilation; pass render options separately to `Render()` or `RenderTo()`.

For data-only models with pure getters and stable collections, opt into `LazyPropertyAccess` and `UseStreamingEvaluation` to reduce unnecessary work. These options change getter timing and traversal order. Nested getters still resolve as needed, and repeated aggregates enumerate separately.

### Keep caching predictable

Configure the process-wide cache during application startup:

```csharp
TemplateMapper.ConfigureCache(capacity: 4096, maximumSourceCharacters: 33554432);
```

Defaults are 2,048 entries and 16,777,216 retained source characters. Entries are evicted incrementally in insertion order. Oversized templates bypass the cache. The source-character budget is not a total managed-memory limit; parsed nodes and metadata have additional overhead. Existing compiled templates remain valid when the cache is reconfigured.

<details>
<summary><strong>Execution boundaries</strong></summary>

Each render owns its values, scope, counters, and an options snapshot. Keep models, parameter dictionaries, and options stable while a render starts and executes.

Cancellation is checked before/after compilation and throughout rendering and aggregate traversal. It cannot interrupt an individual parse operation or arbitrary getter, formatter, enumerator, or writer code. Output limits are checked before writes; formatting may allocate before that check. Use dedicated data-only models and suitable source limits when accepting templates from outside your application. These controls do not make application objects a security sandbox.

</details>

## Compatibility and upgrade notes

<details>
<summary><strong>Existing templates: what stays the same, and what changes</strong></summary>

Existing `Map()` overloads remain available. They accept classes with parameterless constructors; the compiled API also accepts classes without one. Property matching remains case-insensitive, additional keys cannot override model properties, and XML escaping remains opt-in.

The updated renderer executes nodes in source order. It keeps eager top-level property reads by default, with separate eager scopes for selected conditional branches. Review templates that depend on getter side effects or mutation during enumeration.

Correctness improvements include:

- Literal text and values containing old internal markers such as `RP_1` remain intact. Inserted values are never reparsed as template source.
- Nested blocks work in their current scope; conditions inside loops evaluate the row. Root/parent aliases are not provided.
- Formatted dates retain fractional seconds and `DateTime.Kind`.
- Nullable conditions work, and integral/decimal comparisons preserve precision.
- Collection conditions correctly count value-type collections.
- Expression overflow renders empty by default and throws `FormatException` in strict mode.
- Malformed arithmetic is rejected. Validate malformed block syntax before upgrading; its output may differ from the earlier parser.

Retained permissive-mode rules:

- Unknown ordinary values remain `{{ Name }}` at the root and `Name` inside loops.
- Unknown or invalid expression paths render empty.
- Empty aggregate paths render empty; null aggregate sources yield zero.
- A null arithmetic operand makes the result zero unless evaluation fails, such as division by zero.
- `__count(Items.Name)` counts non-null projected names. `__count(Items)` retains the historical result of one non-null collection object rather than the number of elements.

</details>

## Build, test, and contribute

The library targets .NET Standard 2.0. Tests and benchmarks require the .NET 10 SDK.

```sh
dotnet build ObjectSemantics.NET.sln -c Release
dotnet test ObjectSemantics.NET.sln -c Release
```

Found an edge case or have an idea? [Open an issue](https://github.com/swagfin/ObjectSemantics.NET/issues) or submit a pull request. A small template, sample model, and expected output make a great starting point.

[Project wiki](https://github.com/swagfin/ObjectSemantics.NET/wiki) · [NuGet package](https://www.nuget.org/packages/ObjectSemantics.NET) · [MIT license](LICENSE)
