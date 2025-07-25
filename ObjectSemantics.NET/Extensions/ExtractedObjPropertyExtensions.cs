﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security;

namespace ObjectSemantics.NET
{
    public static class ExtractedObjPropertyExtensions
    {
        public static string GetPropertyDisplayString(this ExtractedObjProperty p, string stringFormatting, TemplateMapperOptions templateMapperOptions)
        {
            string formattedPropertyString = GetAppliedPropertyFormatting(p, stringFormatting);
            //Apply Options to Property value string
            if (templateMapperOptions == null) return formattedPropertyString;
            if (templateMapperOptions.XmlCharEscaping)
                formattedPropertyString = SecurityElement.Escape(formattedPropertyString);
            return formattedPropertyString;
        }
        private static string GetAppliedPropertyFormatting(this ExtractedObjProperty p, string customFormattingValue)
        {
            if (string.IsNullOrWhiteSpace(customFormattingValue) || p.OriginalValue == null)
                return p.StringFormatted;
            if (p.Type.Equals(typeof(int)) || p.Type.Equals(typeof(int?)))
                return int.Parse(p.StringFormatted).ToString(customFormattingValue);
            else if (p.Type.Equals(typeof(double)) || p.Type.Equals(typeof(double?)))
                return double.Parse(p.StringFormatted).ToString(customFormattingValue);
            else if (p.Type.Equals(typeof(long)) || p.Type.Equals(typeof(long?)))
                return long.Parse(p.StringFormatted).ToString(customFormattingValue);
            else if (p.Type.Equals(typeof(float)) || p.Type.Equals(typeof(float?)))
                return float.Parse(p.StringFormatted).ToString(customFormattingValue);
            else if (p.Type.Equals(typeof(decimal)) || p.Type.Equals(typeof(decimal?)))
                return decimal.Parse(p.StringFormatted).ToString(customFormattingValue);
            else if (p.Type.Equals(typeof(DateTime)) || p.Type.Equals(typeof(DateTime?)))
                return DateTime.Parse(p.StringFormatted).ToString(customFormattingValue);
            //Custom Formats
            else if (customFormattingValue.ToLower().Equals("uppercase"))
                return p.StringFormatted?.ToUpper();
            else if (customFormattingValue.ToLower().Equals("lowercase"))
                return p.StringFormatted?.ToLower();
            else if (customFormattingValue.ToLower().Equals("tomd5"))
                return p.StringFormatted.ToMD5String();
            else if (customFormattingValue.ToLower().Equals("tobase64"))
                return p.StringFormatted.ToBase64String();
            else if (customFormattingValue.ToLower().Equals("frombase64"))
                return p.StringFormatted.FromBase64String();
            else if (customFormattingValue.ToLower().Equals("length"))
                return p.StringFormatted?.Length.ToString();
            else if (customFormattingValue.ToLower().Equals("titlecase"))
                return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(p.StringFormatted?.ToLower() ?? string.Empty);
            else
                return p.StringFormatted;
        }

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
                        case "==": return v1 == v2;
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
                        case "==": return v1 == v2;
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
                        case "==": return v1 == v2;
                        case "!=": return v1 != v2;
                        case ">": return v1 > v2;
                        case ">=": return v1 >= v2;
                        case "<": return v1 < v2;
                        case "<=": return v1 <= v2;
                        default: return false;
                    }
                }
                else if (property.Type == typeof(bool))
                {
                    bool v1 = Convert.ToBoolean(property.OriginalValue);
                    bool v2 = Convert.ToBoolean(GetConvertibleValue<bool>(valueComparer));
                    switch (criteria)
                    {
                        case "==": return v1 == v2;
                        case "!=": return v1 != v2;
                        default: return false;
                    }
                }
                else if (property.IsEnumerableObject)
                {
                    int v1 = (property.OriginalValue == null) ? 0 : ((IEnumerable<object>)property.OriginalValue).Count();
                    double v2 = Convert.ToDouble(GetConvertibleValue<double>(valueComparer));
                    switch (criteria)
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
                else
                    return false;
            }
            catch { return false; }
        }
    }
}
