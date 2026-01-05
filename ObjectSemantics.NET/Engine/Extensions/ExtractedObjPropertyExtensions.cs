using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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

            Type t = p.Type;
            string val = p.StringFormatted;

            // avoid repeated ToLower calls
            string fmt = customFormat.Trim();
            // handle numeric and datetime formats first
            try
            {
                if (t == typeof(int) || t == typeof(int?))
                    return int.Parse(val, CultureInfo.InvariantCulture).ToString(fmt, CultureInfo.InvariantCulture);
                if (t == typeof(double) || t == typeof(double?))
                    return double.Parse(val, CultureInfo.InvariantCulture).ToString(fmt, CultureInfo.InvariantCulture);
                if (t == typeof(long) || t == typeof(long?))
                    return long.Parse(val, CultureInfo.InvariantCulture).ToString(fmt, CultureInfo.InvariantCulture);
                if (t == typeof(float) || t == typeof(float?))
                    return float.Parse(val, CultureInfo.InvariantCulture).ToString(fmt, CultureInfo.InvariantCulture);
                if (t == typeof(decimal) || t == typeof(decimal?))
                    return decimal.Parse(val, CultureInfo.InvariantCulture).ToString(fmt, CultureInfo.InvariantCulture);
                if (t == typeof(DateTime) || t == typeof(DateTime?))
                    return DateTime.Parse(val, CultureInfo.InvariantCulture).ToString(fmt, CultureInfo.InvariantCulture);
            }
            catch
            {
                // fall through if invalid format
            }

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

        public static bool IsPropertyValueConditionPassed(this ExtractedObjProperty property, string valueComparer, string criteria)
        {
            if (property == null)
                return false;

            try
            {
                Type t = property.Type;
                object original = property.OriginalValue;
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

                if (t == typeof(int) || t == typeof(double) || t == typeof(long) || t == typeof(float) || t == typeof(decimal))
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
                    int v1 = original is IEnumerable<object> enumerable ? enumerable.Count() : 0;
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
            catch
            {
                return false;
            }
        }
    }
}
