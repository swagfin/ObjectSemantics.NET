using System.Collections.Generic;

public class ReplaceObjLoopCode
{
    public string ReplaceRef { get; set; }
    public string TargetObjectName { get; set; }
    public string ObjLoopTemplate { get; set; }
    public List<ReplaceCode> ReplaceObjCodes { get; set; } = new List<ReplaceCode>();
}
