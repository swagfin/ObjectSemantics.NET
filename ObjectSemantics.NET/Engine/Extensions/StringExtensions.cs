using System;
using System.Text;

namespace ObjectSemantics.NET.Engine.Extensions
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

        public static string ToMD5String(this string input)
        {
            try
            {
                using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
                {
                    byte[] inputBytes = Encoding.ASCII.GetBytes(input ?? string.Empty);
                    byte[] hashBytes = md5.ComputeHash(inputBytes);
                    // Convert the byte array to hexadecimal string
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < hashBytes.Length; i++)
                        sb.Append(hashBytes[i].ToString("X2"));
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return string.Format("Error: [ToMD5] conversion, {0}", ex.Message);
            }
        }

        public static string ToBase64String(this string input)
        {
            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(input ?? string.Empty);
                return Convert.ToBase64String(byteArray);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return string.Format("Error: [ToBase64] conversion, {0}", ex.Message);
            }
        }

        public static string FromBase64String(this string base64Input)
        {
            try
            {
                if (string.IsNullOrEmpty(base64Input))
                    return string.Empty;
                byte[] byteArray = Convert.FromBase64String(base64Input);
                return Encoding.UTF8.GetString(byteArray);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                return string.Format("Error: [FromBase64] conversion, {0}", ex.Message);
            }
        }
    }
}
