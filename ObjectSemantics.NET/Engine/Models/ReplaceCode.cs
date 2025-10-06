namespace ObjectSemantics.NET.Engine.Models
{
    public class ReplaceCode
    {
        public string ReplaceRef { get; set; }
        public string ReplaceCommand { get; set; }
        public string TargetPropertyName
        {
            get
            {
                if (string.IsNullOrEmpty(ReplaceCommand))
                    return string.Empty;
                int colonIndex = ReplaceCommand.IndexOf(':');
                return colonIndex > 0 ? ReplaceCommand.Substring(0, colonIndex).Trim() : ReplaceCommand.Trim();
            }
        }
    }
}