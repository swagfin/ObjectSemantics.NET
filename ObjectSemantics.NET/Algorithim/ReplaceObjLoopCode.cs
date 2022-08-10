using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class ReplaceObjLoopCode
{
    public string ReplaceRef { get; set; }
    public string TargetObjectName { get; set; }
    public string ObjLoopTemplate { get; set; }
    public List<ReplaceCode> ReplaceObjCodes { get; set; } = new List<ReplaceCode>();
}
public class ReplaceCode
{
    public string ReplaceRef { get; set; }
    public string ReplaceCommand { get; set; }
    public string TargetPropertyName
    {
        get
        {
            return ReplaceCommand.Replace("{", string.Empty).Replace("}", string.Empty).Trim().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        }
    }
    public string FormattingCommand
    {
        get
        {
            Match hasFormatting = Regex.Match(ReplaceCommand, "{{--command--:(.+)}}".Replace("--command--", TargetPropertyName), RegexOptions.IgnoreCase);
            if (hasFormatting.Success)
                return hasFormatting.Groups[1].Value;
            return string.Empty;
        }
    }
}
