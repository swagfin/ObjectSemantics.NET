# Template Syntax

## Placeholders

Use double curly braces:

```text
{{ PropertyName }}
```

Examples:

- `{{ Name }}`
- `{{ Amount:N2 }}`
- `{{ Customer.BankingDetail.BankName }}`

## Nested Paths

Dot notation is supported:

```text
{{ Customer.CompanyName }}
{{ Customer.BankingDetail.AccountNumber }}
```

If any intermediate nested object is null, output is empty for that placeholder.

## Loops

```text
{{ #foreach(Items) }}
  {{ Name }} x {{ Quantity }}
{{ #endforeach }}
```

### Looping primitive arrays

Use `.` for the current item:

```text
{{ #foreach(Tags) }}[{{ . }}]{{ #endforeach }}
{{ #foreach(Tags) }}[{{ .:uppercase }}]{{ #endforeach }}
```

## Conditions

```text
{{ #if(Age >= 18) }}Adult{{ #else }}Minor{{ #endif }}
```

Supported operators:

- `==`
- `!=`
- `>`
- `>=`
- `<`
- `<=`

Conditions work with numbers, strings, booleans, dates, and collection counts.

## Formatting

### Standard .NET style formats

- Number: `N2`, `#,##0`
- DateTime: `yyyy-MM-dd`, `dd-MMM-yyyy hh:mm tt`

Examples:

- `{{ Price:N2 }}`
- `{{ Price:#,##0 }}`
- `{{ PaymentDate:yyyy-MM-dd }}`

### Built-in string transforms

| Format | Description |
| --- | --- |
| `uppercase` | Uppercase value |
| `lowercase` | Lowercase value |
| `titlecase` | Title case value |
| `length` | Character count |
| `ToMD5` | MD5 hash |
| `ToBase64` | Base64 encode |
| `FromBase64` | Base64 decode |

Examples:

- `{{ Name:uppercase }}`
- `{{ Value:ToBase64 }}`

## Unknown Placeholder Behavior

Regular placeholders with unknown property names remain unresolved:

```text
{{ UnknownProperty }}
```

This helps content authors detect template mistakes quickly.

Calculation expressions are handled differently; see [Calculations](Calculations.md).
