using System;

namespace ObjectSemantics.NET
{
    public static class StringExtensions
    {
        public static string RemoveLastInstanceOfString(this string value, string removeString)
        {
            int index = value.LastIndexOf(removeString, StringComparison.Ordinal);
            return index < 0 ? value : value.Remove(index, removeString.Length);
        }
        public static string RemoveLastInstanceOfString(this string value, params char[] chars)
        {
            foreach (char c in chars)
                value = value.RemoveLastInstanceOfString(c.ToString());
            return value;
        }
    }
}
