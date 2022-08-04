using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectSemantics.NET.Algorithim
{
    public class GeorgesPrincipleAlgorithim
    {

        public string GenerateFromTemplate<T>(T record, TemplateInitialization templateInitialization, List<ObjectSemanticsKeyValue> parameterKeyValues = null) where T : new()
        {
            StringBuilder finalResultBuilder = new StringBuilder();
            ObjPropertiesDescription description = GetObjPropertiesDescription(record, parameterKeyValues);
            foreach (TemplateLine line in templateInitialization.TemplateLines)
            {
                string cleanLine = ReplaceDataLineWithObjProperties(line.OriginalLine, description);
                finalResultBuilder.AppendLine(cleanLine);
            }
            return finalResultBuilder.ToString()?.Trim();
        }
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


        private static ObjPropertiesDescription GetObjPropertiesDescription<T>(T value, List<ObjectSemanticsKeyValue> parameters = null) where T : new()
        {
            ObjPropertiesDescription objPropertiesDescription = new ObjPropertiesDescription();
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                try
                {

                    if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string))
                    {

                    }
                    else if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                    {

                    }
                    else
                    {
                        objPropertiesDescription.CastableToStringProperties.Add(new ExtractedObjProperty
                        {
                            Type = prop.PropertyType,
                            Name = prop.Name,
                            OriginalValue = value == null ? null : prop.GetValue(value)
                        });
                    }

                }
                catch { }
            }
            //Append Parameters
            if (parameters != null && parameters.Count != 0)
                foreach (var param in parameters)
                    objPropertiesDescription.CastableToStringProperties.Add(new ExtractedObjProperty { Type = param.Type, Name = param.Key, OriginalValue = param.Value });
            return objPropertiesDescription;
        }


        public static string ReplaceDataLineWithObjProperties(string line, ObjPropertiesDescription objPropertiesDescription)
        {
            if (string.IsNullOrWhiteSpace(line))
                return string.Empty;
            if (objPropertiesDescription == null || objPropertiesDescription.CastableToStringProperties.Count == 0)
                return string.Empty;
            foreach (ExtractedObjProperty p in objPropertiesDescription.CastableToStringProperties)
            {
                string searchKey = Regex.Replace("{{--value--}}", "--value--", p.Name, RegexOptions.IgnoreCase);
                string searchValue = string.Format("{0}", p.StringFormatted);
                //Property is of Type of Enumerable
                //Search Key With Custom Formatting e.g. 1,200
                string customValFormatting = Regex.Replace("{{--value--:", "--value--", p.Name, RegexOptions.IgnoreCase);
                string regexF = string.Format(@"\{0}(.+)\}}", customValFormatting);
                Match blockMatch = Regex.Match(line, regexF, RegexOptions.IgnoreCase);
                if (blockMatch.Success)
                {
                    Match regexMatch = Regex.Match(blockMatch.Value, @"{{(.+?)}}", RegexOptions.IgnoreCase);
                    while (regexMatch.Success)
                    {
                        string customFormatting = Regex.Replace("--value--:", "--value--", p.Name, RegexOptions.IgnoreCase);
                        string customFormattingValue = regexMatch.Groups[1].Value.ReplaceMany(new string[] { customFormatting, "}", "{" }, string.Empty);

                        if (p.Type.Equals(typeof(int)))
                            line = Regex.Replace(line, regexMatch.Value, int.Parse(p.StringFormatted).ToString(customFormattingValue), RegexOptions.IgnoreCase);
                        else if (p.Type.Equals(typeof(double)))
                            line = Regex.Replace(line, regexMatch.Value, double.Parse(p.StringFormatted).ToString(customFormattingValue), RegexOptions.IgnoreCase);
                        else if (p.Type.Equals(typeof(long)))
                            line = Regex.Replace(line, regexMatch.Value, long.Parse(p.StringFormatted).ToString(customFormattingValue), RegexOptions.IgnoreCase);
                        else if (p.Type.Equals(typeof(float)))
                            line = Regex.Replace(line, regexMatch.Value, float.Parse(p.StringFormatted).ToString(customFormattingValue), RegexOptions.IgnoreCase);
                        else if (p.Type.Equals(typeof(decimal)))
                            line = Regex.Replace(line, regexMatch.Value, decimal.Parse(p.StringFormatted).ToString(customFormattingValue), RegexOptions.IgnoreCase);
                        else if (p.Type.Equals(typeof(DateTime)))
                            line = Regex.Replace(line, regexMatch.Value, DateTime.Parse(p.StringFormatted).ToString(customFormattingValue), RegexOptions.IgnoreCase);
                        //Custom Formats
                        else if (customFormattingValue.ToLower().Equals("uppercase"))
                            line = Regex.Replace(line, regexMatch.Value, searchValue.ToUpper(), RegexOptions.IgnoreCase);
                        else if (customFormattingValue.ToLower().Equals("lowercase"))
                            line = Regex.Replace(line, regexMatch.Value, searchValue.ToLower(), RegexOptions.IgnoreCase);
                        else
                            line = Regex.Replace(line, regexMatch.Value, searchValue, RegexOptions.IgnoreCase);

                        //Proceed To Next
                        regexMatch = regexMatch.NextMatch();
                    }

                }
                else
                    line = Regex.Replace(line, searchKey, searchValue, RegexOptions.IgnoreCase);
            }
            return line;
        }

    }


    public class ObjPropertiesDescription
    {
        public List<ExtractedObjProperty> CastableToStringProperties = new List<ExtractedObjProperty>();
        //public List<ObjPropertiesDescription> ObjPropertiesDescriptions = new List<ObjPropertiesDescri
        //ption>();
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
