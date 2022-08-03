using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace ObjectSemantics.NET.Algorithim
{
    public class GeorgesPrincipleAlgorithim
    {
        public TemplateInitialization GetTemplatInitialization(string templateFilePath)
        {
            if (!File.Exists(templateFilePath))
                throw new Exception($"Template doesn't seem to exist in directory: {templateFilePath}");
            TemplateInitialization initObj = new TemplateInitialization();
            using (var reader = new StreamReader(templateFilePath))
            {
                string line;
                long currentIndex = -1;
                bool startedRowsLoopCode = false;
                long startedRowsLoopCodeAtIndex = 0;
                //For Obj Loop
                bool startedObjLoopCode = false;
                long startedObjLoopCodeIndex = 0;
                string startedObjLoopCodePropertyName = null;
                while ((line = reader.ReadLine()) != null)
                {
                    currentIndex++;
                    TemplateLine templateLine = new TemplateLine
                    {
                        Index = currentIndex,
                        LineId = Guid.NewGuid(),
                        OriginalLine = RemoveStylishWhitespacesInExecutableBlocks(line)
                    };

                    //ICollection Of an Object [Loop]
                    Match regexObjStartLoop = Regex.Match(templateLine.OriginalLine, @"{{for-each-start:(.+?)}}");
                    Match regexObjEndLoop = Regex.Match(templateLine.OriginalLine, @"{{for-each-end:(.+?)}}");
                    if (regexObjStartLoop.Success)
                    {
                        startedObjLoopCodeIndex = currentIndex;
                        startedObjLoopCode = true;
                        startedObjLoopCodePropertyName = regexObjStartLoop.Groups[1].Value;
                    }
                    else if (regexObjEndLoop.Success && startedObjLoopCode)
                    {
                        initObj.TemplateObjLoopCodes.Add(new TemplateObjLoopCode { ProperyName = startedObjLoopCodePropertyName, StartIndex = startedObjLoopCodeIndex, EndIndex = currentIndex });
                        startedObjLoopCode = false;
                    }

                    //ICollection Of an Objects [Loop targeting all records]
                    if (templateLine.OriginalLine.Contains("{{for-each-start}}"))
                    {
                        startedRowsLoopCodeAtIndex = currentIndex;
                        startedRowsLoopCode = true;
                    }
                    else if (templateLine.OriginalLine.Contains("{{for-each-end}}") && startedRowsLoopCode)
                    {
                        //Find Row Loops Between Index
                        var rowObjLoops = FindRowObjLoops(initObj.TemplateLines.Where(x => x.Index >= startedRowsLoopCodeAtIndex && x.Index <= currentIndex).ToList());
                        initObj.TemplateRowLoopCodes.Add(new TemplateRowLoopCode { StartIndex = startedRowsLoopCodeAtIndex, EndIndex = currentIndex, TemplateObjLoopCodes = rowObjLoops });
                        startedRowsLoopCode = false;
                    }
                    //Finnaly Add Template Line
                    initObj.TemplateLines.Add(templateLine);
                }
            }

            return initObj;
        }


        private List<TemplateObjLoopCode> FindRowObjLoops(List<TemplateLine> templateLines)
        {
            List<TemplateObjLoopCode> rowLoops = new List<TemplateObjLoopCode>();
            if (templateLines == null || templateLines.Count == 0)
                return rowLoops;
            bool startedObjLoopCode = false;
            long startedObjLoopCodeIndex = 0;
            string startedObjLoopCodePropertyName = null;
            for (int i = 0; i < templateLines.Count; i++)
            {
                string templateLine = templateLines[i].OriginalLine;
                Match regexObjStartLoop = Regex.Match(templateLine, @"{{for-each-start:(.+?)}}");
                Match regexObjEndLoop = Regex.Match(templateLine, @"{{for-each-end:(.+?)}}");
                if (regexObjStartLoop.Success)
                {
                    startedObjLoopCodeIndex = templateLines[i].Index;
                    startedObjLoopCode = true;
                    startedObjLoopCodePropertyName = regexObjStartLoop.Groups[1].Value;
                }
                else if (regexObjEndLoop.Success && startedObjLoopCode)
                {
                    rowLoops.Add(new TemplateObjLoopCode { ProperyName = startedObjLoopCodePropertyName, StartIndex = startedObjLoopCodeIndex, EndIndex = templateLines[i].Index });
                    startedObjLoopCode = false;
                }
            }
            return rowLoops;
        }
        private string RemoveStylishWhitespacesInExecutableBlocks(string lineBlock)
        {
            if (string.IsNullOrWhiteSpace(lineBlock))
                return string.Empty;
            Match executableBlock = Regex.Match(lineBlock, @"{{(.+?)}}");
            if (executableBlock.Success && (executableBlock.Value.StartsWith("{{ ") || executableBlock.Value.EndsWith(" }}")))
            {
                while (executableBlock.Success)
                {
                    string stringToReplace = executableBlock.Value;
                    string replaceWithValue = string.Format(@"{0}{1}{2}", "{{", executableBlock.Groups[1].Value.Trim(), "}}");
                    lineBlock = Regex.Replace(lineBlock, stringToReplace, replaceWithValue, RegexOptions.RightToLeft);
                    executableBlock = executableBlock.NextMatch();
                }
            }
            return lineBlock;
        }
    }


    public class TemplateInitialization
    {
        public List<TemplateLine> TemplateLines { get; set; } = new List<TemplateLine>();
        public List<TemplateRowLoopCode> TemplateRowLoopCodes { get; set; } = new List<TemplateRowLoopCode>();
        public List<TemplateObjLoopCode> TemplateObjLoopCodes { get; set; } = new List<TemplateObjLoopCode>();
    }
    public class TemplateLine
    {
        public long Index { get; set; }
        public string OriginalLine { get; set; }
        public Guid LineId { get; set; } = Guid.NewGuid();
    }
    public class TemplateRowLoopCode
    {
        public long StartIndex { get; set; }
        public long EndIndex { get; set; }
        public List<TemplateObjLoopCode> TemplateObjLoopCodes { get; set; } = new List<TemplateObjLoopCode>();

    }
    public class TemplateObjLoopCode
    {
        public string ProperyName { get; set; }
        public long StartIndex { get; set; }
        public long EndIndex { get; set; }

    }
}
