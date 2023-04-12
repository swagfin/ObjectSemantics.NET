using System;
using System.Collections.Generic;
using System.Linq;
namespace ObjectSemantics.NET
{
    public static class ExtractedObjPropertyExtensions
    {
        private static T GetConvertibleValue<T>(string value) where T : IConvertible
        {
            return (string.IsNullOrEmpty(value) || value?.ToLower()?.Trim() == "null") ? default : (T)Convert.ChangeType(value, typeof(T));
        }
        public static bool IsPropertyValueConditionPassed(this ExtractedObjProperty property, string valueComparer, string criteria)
        {
            try
            {
                if (property == null) return false;
                else if (property.Type == typeof(string))
                {
                    string v1 = property.OriginalValue?.ToString()?.Trim().ToLower() ?? string.Empty;
                    string v2 = GetConvertibleValue<string>(valueComparer)?.Trim().ToLower() ?? string.Empty;
                    switch (criteria)
                    {
                        case "=": return v1 == v2;
                        case "!=": return v1 != v2;
                        default:
                            return string.Compare(v1, v2, true) == 0;
                    }
                }
                else if (property.Type == typeof(int) || property.Type == typeof(double) || property.Type == typeof(long) || property.Type == typeof(float) || property.Type == typeof(decimal))
                {
                    double v1 = Convert.ToDouble(property.OriginalValue ?? "0");
                    double v2 = Convert.ToDouble(GetConvertibleValue<double>(valueComparer));
                    switch (criteria)
                    {
                        case "=": return v1 == v2;
                        case "!=": return v1 != v2;
                        case ">": return v1 > v2;
                        case ">=": return v1 >= v2;
                        case "<": return v1 < v2;
                        case "<=": return v1 <= v2;
                        default: return false;
                    }
                }
                else if (property.Type == typeof(DateTime))
                {
                    DateTime v1 = Convert.ToDateTime(property.OriginalValue);
                    DateTime v2 = Convert.ToDateTime(GetConvertibleValue<DateTime>(valueComparer));
                    switch (criteria)
                    {
                        case "=": return v1 == v2;
                        case "!=": return v1 != v2;
                        case ">": return v1 > v2;
                        case ">=": return v1 >= v2;
                        case "<": return v1 < v2;
                        case "<=": return v1 <= v2;
                        default: return false;
                    }
                }
                else if (property.IsEnumerableObject)
                {
                    int v1 = (property.OriginalValue == null) ? 0 : ((IEnumerable<object>)property.OriginalValue).Count();
                    double v2 = Convert.ToDouble(GetConvertibleValue<double>(valueComparer));
                    switch (criteria)
                    {
                        case "=": return v1 == v2;
                        case "!=": return v1 != v2;
                        case ">": return v1 > v2;
                        case ">=": return v1 >= v2;
                        case "<": return v1 < v2;
                        case "<=": return v1 <= v2;
                        default: return false;
                    }
                }
                else
                    return false;
            }
            catch { return false; }
        }
    }
}
