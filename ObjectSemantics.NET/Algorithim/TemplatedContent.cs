using System.Collections.Generic;

public class TemplatedContent
{
    public string Template { get; set; }
    public List<ReplaceObjLoopCode> ReplaceObjLoopCodes { get; set; } = new List<ReplaceObjLoopCode>();
    public List<ReplaceCode> ReplaceCodes { get; set; } = new List<ReplaceCode>();
    public List<ReplaceIfOperationCode> ReplaceIfConditionCodes { get; set; } = new List<ReplaceIfOperationCode>();
}
