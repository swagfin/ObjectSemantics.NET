using ObjectSemantics.NET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

internal static class SemanticFileMaskedAlgorithimv2
{
    public static string GenerateFromObjRecord<T>(this T record, List<string> fileLines, List<ObjectSemanticsKeyValue> parameterKeyValues = null) where T : new()
    {
        List<ExtractedObjProperty> objProperties = GetObjProperties(record);
        if (parameterKeyValues != null && parameterKeyValues.Count > 0)
            objProperties.AddRange(parameterKeyValues.ToObjProperties()); //Append Custom
        StringBuilder sbText = new StringBuilder();
        for (int i = 0; i < fileLines.Count; i++)
        {
            string lineBlock = fileLines[i].RemoveStylishWhitespaces();
            lineBlock = ReplaceDataLineWithObjProperties(lineBlock, objProperties);
            sbText.AppendLine(lineBlock);
        }
        return sbText.ToString()?.Trim();
    }
    public static string GenerateFromObjCollection<T>(this List<T> dataRecords, List<string> fileLines, List<ObjectSemanticsKeyValue> parameterKeyValues = null) where T : new()
    {
        string cleanedCode = string.Empty;
        if (fileLines != null && fileLines.Count != 0)
        {
            List<ExtractedObjProperty> additionalParameters = parameterKeyValues.ToObjProperties();
            bool startedBlock = false;
            string loopContent = string.Empty;
            for (int i = 0; i < fileLines.Count; i++)
            {
                try
                {
                    string lineBlock = fileLines[i].RemoveStylishWhitespaces();
                    if (lineBlock.Contains("{{for-each-start}}"))
                    {
                        startedBlock = true;
                        loopContent = string.Empty;
                    }
                    else if (lineBlock.Contains("{{for-each-end}}") && startedBlock)
                    {
                        startedBlock = false;
                        foreach (T record in dataRecords)
                        {
                            List<ExtractedObjProperty> properties = GetObjProperties(record);
                            if (additionalParameters != null && additionalParameters.Count > 0)
                                properties.AddRange(additionalParameters);
                            cleanedCode = string.Format("{0}{1}{2}", cleanedCode, ReplaceDataLineWithObjProperties(loopContent, properties), Environment.NewLine);
                        }
                        loopContent = string.Empty;
                    }
                    else if (startedBlock)
                        loopContent = string.Format("{0}{1}{2}", loopContent, lineBlock, Environment.NewLine);
                    else
                        cleanedCode = string.Format("{0}{1}{2}", cleanedCode, lineBlock, Environment.NewLine);
                }
                catch (Exception ex)
                {
                    cleanedCode = string.Format("{0}{1}{2}", cleanedCode, $"------EXCEPTION OCCURRED---- {ex.Message}", Environment.NewLine);
                }

            }
        }
        return cleanedCode?.Trim();
    }


    public static string ReplaceDataLineWithObjProperties(string line, List<ExtractedObjProperty> properties)
    {
        if (string.IsNullOrWhiteSpace(line))
            return string.Empty;
        if (properties == null || properties.Count == 0)
            return string.Empty;
        foreach (ExtractedObjProperty p in properties)
        {
            string searchKey = Regex.Replace("{{--value--}}", "--value--", p.Name, RegexOptions.IgnoreCase);
            string searchValue = string.Format("{0}", p.StringFormatted);
            //Property is of Type of Enumerable
            if (typeof(IEnumerable).IsAssignableFrom(p.Type) && p.Type != typeof(string))
            {
                AssignObjectChildLoopForTypeEnumerable(ref line, p);
            }
            else
            {

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
        }
        return line;
    }

    private static void AssignObjectChildLoopForTypeEnumerable(ref string value, ExtractedObjProperty p)
    {
        string cleanedCode = string.Empty;
        string[] fileLines = Regex.Split(value, "\r\n|\r|\n");
        if (fileLines != null && fileLines.Length > 0)
        {
            bool startedBlock = false;
            string loopContent = string.Empty;
            string startCodeBlock = Regex.Replace("{{for-each-start:--value--}}", "--value--", p.Name, RegexOptions.IgnoreCase);
            string endCodeBlock = Regex.Replace("{{for-each-end:--value--}}", "--value--", p.Name, RegexOptions.IgnoreCase);
            string lineBlock;
            for (int i = 0; i < fileLines.Length; i++)
            {
                try
                {
                    lineBlock = fileLines[i];
                    if (lineBlock.ToLower().Contains(startCodeBlock.ToLower()))
                    {
                        startedBlock = true;
                        loopContent = string.Empty;
                    }
                    else if (lineBlock.ToLower().Contains(endCodeBlock.ToLower()) && startedBlock)
                    {
                        startedBlock = false;
                        if (p.OriginalValue != null)
                            foreach (object record in (IEnumerable)p.OriginalValue)
                            {
                                List<ExtractedObjProperty> properties = GetObjPropertiesFromUnknown(record);
                                cleanedCode = string.Format("{0}{1}{2}", cleanedCode, ReplaceDataLineWithObjProperties(loopContent, properties), Environment.NewLine);
                            }
                        loopContent = string.Empty;
                    }
                    else if (startedBlock)
                        loopContent = string.Format("{0}{1}{2}", loopContent, lineBlock, Environment.NewLine);
                    else
                        cleanedCode = string.Format("{0}{1}{2}", cleanedCode, lineBlock, Environment.NewLine);
                }
                catch (Exception ex)
                {
                    cleanedCode = string.Format("{0}{1}{2}", cleanedCode, $"------EXCEPTION OCCURRED---- {ex.Message}", Environment.NewLine);
                }

            }
        }
        value = cleanedCode;
    }


    private static List<ExtractedObjProperty> GetObjProperties<T>(T value) where T : new()
    {
        List<ExtractedObjProperty> list = new List<ExtractedObjProperty>();
        foreach (PropertyInfo prop in typeof(T).GetProperties())
        {
            try
            {
                list.Add(new ExtractedObjProperty
                {
                    Type = prop.PropertyType,
                    Name = prop.Name,
                    OriginalValue = value == null ? null : prop.GetValue(value)
                });
            }
            catch { }
        }
        return list;
    }
    private static List<ExtractedObjProperty> GetObjPropertiesFromUnknown(object value)
    {
        List<ExtractedObjProperty> list = new List<ExtractedObjProperty>();
        if (value == null)
            return list;
        Type myType = value.GetType();
        IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());
        foreach (PropertyInfo prop in props)
        {
            try
            {
                list.Add(new ExtractedObjProperty
                {
                    Type = prop.PropertyType,
                    Name = prop.Name,
                    OriginalValue = value == null ? null : prop.GetValue(value)
                });
            }
            catch { }
        }
        return list;
    }
    private static List<ExtractedObjProperty> ToObjProperties(this List<ObjectSemanticsKeyValue> parameterKeyValues)
    {
        //Also Insert Parameter Keys
        List<ExtractedObjProperty> list = new List<ExtractedObjProperty>();
        if (parameterKeyValues == null)
            return list;
        foreach (ObjectSemanticsKeyValue param in parameterKeyValues)
        {
            try
            {
                list.Add(new ExtractedObjProperty
                {
                    Type = param.GetType(),
                    Name = param.Key,
                    OriginalValue = param.Value
                });
            }
            catch { }
        }
        return list;
    }
    public static string ReplaceMany(this string inputString, string[] find, string replaceWith, RegexOptions regexOptions = RegexOptions.IgnoreCase)
    {
        if (find.Length == 0)
            return inputString;
        foreach (var pattern in find)
            inputString = Regex.Replace(inputString, pattern, replaceWith, regexOptions);
        return inputString;
    }

    public static string RemoveStylishWhitespaces(this string lineBlock)
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
    public static List<string> RemoveStylishWhitespaces(this List<string> lineBlocks)
    {
        if (lineBlocks == null || lineBlocks.Count == 0)
            return new List<string>();
        return lineBlocks.Select(x => RemoveStylishWhitespaces(x)).ToList();
    }
    public static List<string> RemoveLoopBlockCodeSpace(this List<string> lineBlocks)
    {
        List<string> cleanedBlock = new List<string>();
        if (lineBlocks != null && lineBlocks.Count != 0)
        {
            bool startedBlock = false;
            string lineBlock;
            foreach (string line in lineBlocks)
            {
                try
                {
                    lineBlock = line.RemoveStylishWhitespaces();
                    if (lineBlock.Contains("{{for-each-start}}"))
                        startedBlock = true;
                    else if (lineBlock.Contains("{{for-each-end}}") && startedBlock)
                        startedBlock = false;
                    else if (startedBlock) { }
                    else { cleanedBlock.Add(lineBlock); }
                }
                catch (Exception ex) { cleanedBlock.Add($"------EXCEPTION OCCURRED---- {ex.Message}"); }

            }
        }
        return cleanedBlock;
    }
}