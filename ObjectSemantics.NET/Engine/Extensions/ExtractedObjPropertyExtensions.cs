using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections;
using System.Globalization;
using System.Security;

namespace ObjectSemantics.NET.Engine.Extensions
{
    internal static class ExtractedObjPropertyExtensions
    {
        public static string GetPropertyDisplayString(this ExtractedObjProperty p, string stringFormatting, TemplateMapperOptions options)
        {
            if (p == null)
                return string.Empty;

            string formatted = p.GetAppliedPropertyFormatting(stringFormatting);
            if (options?.XmlCharEscaping == true && !string.IsNullOrEmpty(formatted))
                formatted = SecurityElement.Escape(formatted);

            return formatted;
        }

        private static string GetAppliedPropertyFormatting(this ExtractedObjProperty p, string customFormat)
        {
            if (string.IsNullOrEmpty(customFormat) || p.OriginalValue == null)
                return p.StringFormatted;

            Type t = Nullable.GetUnderlyingType(p.Type) ?? p.Type;

            // avoid repeated ToLower calls
            string fmt = customFormat.Trim();
            // handle numeric and datetime formats first
            try
            {
                if (t == typeof(int) || t == typeof(double) || t == typeof(long) || t == typeof(float) || t == typeof(decimal) || t == typeof(DateTime))
                    return ((IFormattable)p.OriginalValue).ToString(fmt, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                // Preserve the existing fallback for unsupported format strings.
            }

            string val = p.StringFormatted;
            // custom string-based formats (single switch to avoid multiple ToLower() checks)
            switch (fmt.ToLowerInvariant())
            {
                case "uppercase": return val?.ToUpperInvariant();
                case "lowercase": return val?.ToLowerInvariant();
                case "tomd5": return val?.ToMD5String();
                case "tobase64": return val?.ToBase64String();
                case "frombase64": return val?.FromBase64String();
                case "length": return val?.Length.ToString(CultureInfo.InvariantCulture);
                case "titlecase": return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(val?.ToLowerInvariant() ?? string.Empty);
                default: return val;
            }
        }

        private static T GetConvertibleValue<T>(string value) where T : IConvertible
        {
            if (string.IsNullOrWhiteSpace(value) || string.Equals(value.Trim(), "null", StringComparison.OrdinalIgnoreCase))
                return default;

            return (T)Convert.ChangeType(value, typeof(T), CultureInfo.InvariantCulture);
        }

        public static bool IsPropertyValueConditionPassed(this ExtractedObjProperty property, string valueComparer, string criteria, EngineRenderContext context)
        {
            if (property == null)
                return false;

            try
            {
                Type nullableType = Nullable.GetUnderlyingType(property.Type);
                Type t = nullableType ?? property.Type;
                object original = property.OriginalValue;
                if (nullableType != null && (original == null || string.Equals(valueComparer.Trim(), "null", StringComparison.OrdinalIgnoreCase)))
                {
                    bool bothNull = original == null && string.Equals(valueComparer.Trim(), "null", StringComparison.OrdinalIgnoreCase);
                    return criteria == "==" ? bothNull : criteria == "!=" && !bothNull;
                }
                string crit = criteria?.Trim() ?? string.Empty;

                if (t == typeof(string))
                {
                    string v1 = (original?.ToString() ?? string.Empty).Trim().ToLowerInvariant();
                    string v2 = (GetConvertibleValue<string>(valueComparer) ?? string.Empty).Trim().ToLowerInvariant();
                    switch (crit)
                    {
                        case "==":
                            return v1 == v2;
                        case "!=":
                            return v1 != v2;
                        default:
                            return string.Equals(v1, v2, StringComparison.OrdinalIgnoreCase);
                    }
                }

                if (t == typeof(int) || t == typeof(long) || t == typeof(decimal))
                {
                    decimal left = Convert.ToDecimal(original ?? 0, CultureInfo.InvariantCulture);
                    decimal right = GetConvertibleValue<decimal>(valueComparer);
                    switch (crit)
                    {
                        case "==": return left == right;
                        case "!=": return left != right;
                        case ">": return left > right;
                        case ">=": return left >= right;
                        case "<": return left < right;
                        case "<=": return left <= right;
                        default: return false;
                    }
                }

                if (t == typeof(double) || t == typeof(float))
                {
                    double v1 = Convert.ToDouble(original ?? 0, CultureInfo.InvariantCulture);
                    double v2 = Convert.ToDouble(GetConvertibleValue<double>(valueComparer), CultureInfo.InvariantCulture);

                    switch (crit)
                    {
                        case "==": return v1 == v2;
                        case "!=": return v1 != v2;
                        case ">": return v1 > v2;
                        case ">=": return v1 >= v2;
                        case "<": return v1 < v2;
                        case "<=": return v1 <= v2;
                        default: return false;
                    }
                }

                if (t == typeof(DateTime))
                {
                    DateTime v1 = Convert.ToDateTime(original, CultureInfo.InvariantCulture);
                    DateTime v2 = Convert.ToDateTime(GetConvertibleValue<DateTime>(valueComparer), CultureInfo.InvariantCulture);

                    switch (crit)
                    {
                        case "==": return v1 == v2;
                        case "!=": return v1 != v2;
                        case ">": return v1 > v2;
                        case ">=": return v1 >= v2;
                        case "<": return v1 < v2;
                        case "<=": return v1 <= v2;
                        default: return false;
                    }
                }

                if (t == typeof(bool))
                {
                    bool v1 = Convert.ToBoolean(original, CultureInfo.InvariantCulture);
                    bool v2 = Convert.ToBoolean(GetConvertibleValue<bool>(valueComparer));
                    return crit == "==" ? v1 == v2 : crit == "!=" && v1 != v2;
                }

                if (property.IsEnumerableObject)
                {
                    long v1 = 0;
                    if (original is ICollection collection) v1 = collection.Count;
                    else if (original is IEnumerable enumerable)
                    {
                        foreach (object item in enumerable)
                        {
                            context.CountIteration();
                            v1++;
                        }
                    }
                    double v2 = Convert.ToDouble(GetConvertibleValue<double>(valueComparer), CultureInfo.InvariantCulture);

                    switch (crit)
                    {
                        case "==": return v1 == v2;
                        case "!=": return v1 != v2;
                        case ">": return v1 > v2;
                        case ">=": return v1 >= v2;
                        case "<": return v1 < v2;
                        case "<=": return v1 <= v2;
                        default: return false;
                    }
                }

                return false;
            }
            catch (Exception exception) when (!(exception is OperationCanceledException) && !(exception is TemplateLimitExceededException))
            {
                return false;
            }
        }
    }
}
