# Getting Started

## 1. Install

```powershell
Install-Package ObjectSemantics.NET
```

## 2. Basic Mapping

```csharp
using ObjectSemantics.NET;

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

var person = new Person { Name = "John", Age = 30 };
string result = person.Map("Hi {{ Name }}, age {{ Age }}");
// Hi John, age 30
```

You can also map from the template side:

```csharp
string result = "Hi {{ Name }}".Map(person);
```

## 3. Nested Property Mapping

```csharp
public class Customer
{
    public string CompanyName { get; set; }
}

public class Payment
{
    public decimal Amount { get; set; }
    public Customer Customer { get; set; }
}

var payment = new Payment
{
    Amount = 100000000m,
    Customer = new Customer { CompanyName = "CRUDSOFT TECHNOLOGIES" }
};

string result = payment.Map("Paid {{ Amount:N2 }} by {{ Customer.CompanyName }}");
// Paid 100,000,000.00 by CRUDSOFT TECHNOLOGIES
```

## 4. Additional Parameters

Pass runtime values that are not on your model:

```csharp
public class MessageItem
{
    public string Name { get; set; }
    public int Qty { get; set; }
}

public class MessageModel
{
    public string Name { get; set; }
    public bool IsPaid { get; set; }
    public List<MessageItem> Items { get; set; } = new List<MessageItem>();
}

var model = new MessageModel { Name = "Jane" };
var extra = new Dictionary<string, object>
{
    ["AppName"] = "ObjectSemantics.NET",
    ["Meta.Channel"] = "EMAIL"
};

string result = model.Map("{{ Name }} via {{ Meta.Channel }} in {{ AppName }}", extra);
```

## 5. XML Escaping Option

Use `TemplateMapperOptions` when rendering XML-safe output:

```csharp
var model = new MessageModel { Name = "Tom & Jerry <Ltd>" };
string result = model.Map("{{ Name }}", null, new TemplateMapperOptions
{
    XmlCharEscaping = true
});
// Tom &amp; Jerry &lt;Ltd&gt;
```

## 6. First Loop and Condition

```csharp
var model = new MessageModel
{
    Name = "John",
    IsPaid = true,
    Items = new List<MessageItem>
    {
        new MessageItem { Name = "Keyboard", Qty = 1 },
        new MessageItem { Name = "Mouse", Qty = 2 }
    }
};

string template = @"Status: {{ #if(IsPaid == true) }}PAID{{ #else }}UNPAID{{ #endif }}
{{ #foreach(Items) }}- {{ Qty }} x {{ Name }}
{{ #endforeach }}";

string result = model.Map(template);
```

## Next

- Continue with [Template Syntax](Template-Syntax.md)
- Learn expression functions in [Calculations](Calculations.md)
