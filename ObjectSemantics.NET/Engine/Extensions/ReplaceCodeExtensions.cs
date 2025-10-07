using ObjectSemantics.NET.Engine.Models;

namespace ObjectSemantics.NET.Engine.Extensions
{
    internal static class ReplaceCodeExtensions
    {
        public static string GetTargetPropertyName(this ReplaceCode code)
        {
            if (code == null) return string.Empty;

            if (string.IsNullOrEmpty(code.ReplaceCommand))
                return string.Empty;

            int colonIndex = code.ReplaceCommand.IndexOf(':');
            return colonIndex > 0 ? code.ReplaceCommand.Substring(0, colonIndex).Trim() : code.ReplaceCommand.Trim();
        }

        public static string GetFormattingCommand(this ReplaceCode code)
        {
            if (code == null) return string.Empty;

            if (string.IsNullOrEmpty(code.ReplaceCommand))
                return string.Empty;

            // Find the colon separating the target and the formatting command
            int colonIndex = code.ReplaceCommand.IndexOf(':');
            if (colonIndex < 0 || colonIndex >= code.ReplaceCommand.Length - 1)
                return string.Empty;

            // Extract everything after the first colon
            string afterColon = code.ReplaceCommand.Substring(colonIndex + 1).Trim();

            return afterColon;
        }
    }
}
