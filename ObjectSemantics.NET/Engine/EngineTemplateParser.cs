using ObjectSemantics.NET.Engine.Models;
using System.Text.RegularExpressions;

namespace ObjectSemantics.NET.Engine
{
    internal static class EngineTemplateParser
    {
        private static readonly Regex IfConditionRegex = new Regex(@"{{\s*#\s*if\s*\(\s*(?<param>[\w\.]+)\s*(?<operator>==|!=|>=|<=|>|<)\s*(?<value>[^)]+?)\s*\)\s*}}(?<code>[\s\S]*?)(?:{{\s*#\s*else\s*}}(?<else>[\s\S]*?))?{{\s*#\s*endif\s*}}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex LoopBlockRegex = new Regex(@"{{\s*#\s*foreach\s*\(\s*(?<target>[\w\.]+)\s*\)\s*}}(?<body>[\s\S]*?){{\s*#\s*endforeach\s*}}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DirectParamRegex = new Regex(@"{{(.+?)}}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static EngineRunnerTemplate Parse(string templateContent)
        {
            EngineRunnerTemplate templatedContent = new EngineRunnerTemplate { Template = templateContent ?? string.Empty };
            long key = 0;

            templatedContent.Template = IfConditionRegex.Replace(templatedContent.Template, m =>
            {
                key++;
                string refKey = "RIB_" + key;
                templatedContent.ReplaceIfConditionCodes.Add(new ReplaceIfOperationCode
                {
                    ReplaceRef = refKey,
                    IfPropertyName = m.Groups["param"].Value,
                    IfOperationType = m.Groups["operator"].Value,
                    IfOperationValue = m.Groups["value"].Value,
                    IfOperationTrueTemplate = m.Groups["code"].Value,
                    IfOperationFalseTemplate = m.Groups["else"].Success ? m.Groups["else"].Value : string.Empty
                });
                return refKey;
            });

            templatedContent.Template = LoopBlockRegex.Replace(templatedContent.Template, m =>
            {
                key++;
                string refKey = "RLB_" + key;
                ReplaceObjLoopCode objLoop = new ReplaceObjLoopCode
                {
                    ReplaceRef = refKey,
                    TargetObjectName = m.Groups["target"].Value?.Trim() ?? string.Empty
                };

                string loopBlock = m.Groups["body"].Value;
                loopBlock = DirectParamRegex.Replace(loopBlock, pm =>
                {
                    key++;
                    string loopRef = "RLBR_" + key;
                    objLoop.ReplaceObjCodes.Add(CreateReplaceCode(loopRef, pm.Groups[1].Value));
                    return loopRef;
                });

                objLoop.ObjLoopTemplate = loopBlock;
                templatedContent.ReplaceObjLoopCodes.Add(objLoop);
                return refKey;
            });

            templatedContent.Template = DirectParamRegex.Replace(templatedContent.Template, m =>
            {
                key++;
                string refKey = "RP_" + key;
                templatedContent.ReplaceCodes.Add(CreateReplaceCode(refKey, m.Groups[1].Value));
                return refKey;
            });

            return templatedContent;
        }

        private static ReplaceCode CreateReplaceCode(string replaceRef, string replaceCommand)
        {
            string command = replaceCommand?.Trim() ?? string.Empty;
            ParseReplaceCommand(command, out string targetPropertyName, out string formattingCommand);

            return new ReplaceCode
            {
                ReplaceRef = replaceRef,
                ReplaceCommand = command,
                TargetPropertyName = targetPropertyName,
                FormattingCommand = formattingCommand
            };
        }

        private static void ParseReplaceCommand(string replaceCommand, out string targetPropertyName, out string formattingCommand)
        {
            if (string.IsNullOrEmpty(replaceCommand))
            {
                targetPropertyName = string.Empty;
                formattingCommand = string.Empty;
                return;
            }

            int colonIndex = replaceCommand.IndexOf(':');
            if (colonIndex > 0)
            {
                targetPropertyName = replaceCommand.Substring(0, colonIndex).Trim();
                formattingCommand = colonIndex < replaceCommand.Length - 1 ? replaceCommand.Substring(colonIndex + 1).Trim() : string.Empty;
                return;
            }

            targetPropertyName = replaceCommand.Trim();
            formattingCommand = string.Empty;
        }
    }
}
