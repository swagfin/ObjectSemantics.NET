using System.Text;

namespace ObjectSemantics.NET.Engine.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder ReplaceFirstOccurrence(this StringBuilder sb, string search, string replace)
        {
            if (sb == null || string.IsNullOrEmpty(search))
                return sb;

            int len = sb.Length - search.Length + 1;
            for (int i = 0; i < len; i++)
            {
                bool match = true;
                for (int j = 0; j < search.Length; j++)
                {
                    if (sb[i + j] != search[j])
                    {
                        match = false;
                        break;
                    }
                }

                if (match)
                {
                    sb.Remove(i, search.Length);
                    sb.Insert(i, replace);
                    break;
                }
            }

            return sb;
        }
    }
}
