# Recipes

## 1. Order Confirmation Email

```text
Subject: Order {{ OrderNo }} Confirmed

Hi {{ Customer.FullName:titlecase }},

Order Date: {{ OrderDate:yyyy-MM-dd }}
Payment Status: {{ #if(IsPaid == true) }}PAID{{ #else }}UNPAID{{ #endif }}

Items:
{{ #foreach(Items) }}
- {{ Quantity }} x {{ Name }} = {{ LineTotal:N2 }}
{{ #endforeach }}

Total: {{ Total:N2 }}
```

## 2. Marketing A/B Subject

```text
{{ #if(CampaignVariant == A) }}Special offer for {{ Customer.FullName:titlecase }}{{ #else }}Do not miss out {{ Customer.FullName:titlecase }}{{ #endif }}
```

## 3. Channel Compliance Block (Email vs SMS)

```text
Compliance: {{ #if(Meta.Channel == EMAIL) }}Unsubscribe via {{ Meta.UnsubscribeUrl }}{{ #else }}Reply STOP to unsubscribe{{ #endif }}
```

## 4. Ledger/Balance Snippet with Calculations

```text
Paid: {{ PaidAmount:N2 }}
Credit Limit: {{ Customer.CreditLimit:N2 }}
Difference: {{ __calc(PaidAmount - Customer.CreditLimit):N2 }}
Payments Total: {{ __sum(Customer.Payments.Amount):N2 }}
```

## 5. XML-Safe Payload

When producing XML templates:

```csharp
string output = model.Map(template, extra, new TemplateMapperOptions
{
    XmlCharEscaping = true
});
```

This safely escapes values like `&`, `<`, `>`, quotes, and apostrophes.

## 6. Dot-Key Metadata for Messaging

```csharp
var extra = new Dictionary<string, object>
{
    ["Meta.Channel"] = "EMAIL",
    ["Meta.UnsubscribeUrl"] = "https://example.com/unsubscribe/abc"
};
```

```text
Channel: {{ Meta.Channel }}
Unsubscribe: {{ Meta.UnsubscribeUrl }}
```

## 7. Graceful Fallback for Personalization

```text
{{ #if(Customer.FullName == null) }}Hello Customer{{ #else }}Hello {{ Customer.FullName:titlecase }}{{ #endif }}
```
