using System;

namespace ObjectSemantics.NET
{
    public static class StringExtensions
    {
        public static string ReplaceFirstOccurrence(this string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
                return text;
            return string.Format("{0}{1}{2}", text.Substring(0, pos), replace, text.Substring(pos + search.Length));
        }
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
