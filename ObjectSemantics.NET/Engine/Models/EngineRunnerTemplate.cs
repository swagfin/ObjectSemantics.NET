using System.Collections.Generic;

namespace ObjectSemantics.NET.Engine.Models
{
    internal class EngineRunnerTemplate
    {
        public string Template { get; set; }
        public List<ReplaceObjLoopCode> ReplaceObjLoopCodes { get; set; } = new List<ReplaceObjLoopCode>();
        public List<ReplaceCode> ReplaceCodes { get; set; } = new List<ReplaceCode>();
        public List<ReplaceIfOperationCode> ReplaceIfConditionCodes { get; set; } = new List<ReplaceIfOperationCode>();
    }
}
