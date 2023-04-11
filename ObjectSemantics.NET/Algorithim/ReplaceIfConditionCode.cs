public class ReplaceIfConditionCode
{
    public string IfPropertyName { get; set; }
    public string IfConditionType { get; set; }
    public string IfConditionValue { get; set; }
    public string ReplaceRef { get; set; }
    public string IfConditionTrueTemplate { get; set; } = string.Empty;
    public string IfConditionFalseTemplate { get; set; } = string.Empty;
}

