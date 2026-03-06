# Troubleshooting

## Quick Diagnostics

| Symptom | Likely Cause | What To Do |
| --- | --- | --- |
| `{{ PropertyName }}` remains in output | Unknown regular property | Check model/property spelling or add runtime parameter |
| Calculation renders empty | Invalid/unknown/non-numeric calculation path | Validate expression, path, and data type |
| Calculation renders `0` | Null source/property in math path | Confirm null is expected or initialize source values |
| `#if` block not matching | Type/operator mismatch | Verify property type and comparison value |
| Loop renders nothing | Source is null/non-enumerable/empty | Ensure collection exists and has items |

## Unknown Placeholder vs Calculation Behavior

These are intentionally different:

- Regular placeholder unknown: keeps unresolved text
  - Example: `{{ UnknownProp }}`
- Expression unknown/invalid: renders empty
  - Example: `{{ __sum(Unknown.Path) }}` -> empty

## Common Mistakes

### 1. Wrong nested path

```text
{{ Customer.BankDetails.BankName }}
```

If model uses `BankingDetail` (singular), this expression will fail.

### 2. Using numeric functions on string/date

```text
{{ __sum(Items.Name) }}
{{ __sum(OrderDate) }}
```

These render empty because values are not numeric.

### 3. Using calc with unresolved fields

```text
{{ __calc(PaidAmount - UnknownValue) }}
```

This renders empty.

## Performance Tips

- Reuse template strings to benefit from parser cache.
- Prefer specific property paths over deeply complex expressions.
- Keep heavy transformations outside templates when possible.
- Validate user-authored templates during save time, not only at send time.

## Testing Strategy

For production usage, include tests for:

- Success paths (valid values)
- Null source/property paths
- Unknown paths
- Invalid expressions
- Non-numeric value misuse in numeric functions
- Mixed templates (loop + if + calculation + formatting)
