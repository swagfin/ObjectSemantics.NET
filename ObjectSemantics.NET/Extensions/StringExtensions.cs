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
        public static string GetSubstringByIndexStartAndEnd(this string str, int startIndex, int endIndex)
        {
            if (startIndex < 0 || endIndex < 0 || startIndex >= str.Length || endIndex >= str.Length || startIndex > endIndex)
                throw new ArgumentException("Invalid start and/or end index");
            return str.Substring(startIndex, endIndex - startIndex + 1);
        }
        public static string ReplaceByIndexStartAndEnd(this string str, int startIndex, int endIndex, string replacement)
        {
            if (startIndex < 0 || endIndex > str.Length || startIndex > endIndex)
                throw new ArgumentOutOfRangeException("Invalid start and/or end index.");
            return (endIndex == str.Length)
                    ? string.Format("{0}{1}{2}", str.Substring(0, startIndex), replacement, str.Substring(endIndex))
                    : string.Format("{0}{1}{2}", str.Substring(0, startIndex), replacement, str.Substring(endIndex + 1));
        }
    }
}
