using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

internal static class SemanticFileMaskedAlgorithim
{
    public static string GeneratFromObj<T>(this T record, List<string> fileLines) where T : new()
    {
        //Replace the Contents Again
        string cleanedCode = string.Empty;
        var singleProperties = GetObjProperties(record);
        fileLines = fileLines.RemoveLoopBlockCodeSpace();
        for (int i = 0; i < fileLines.Count; i++)
            cleanedCode = string.Format("{0}{1}", cleanedCode, fileLines[i].ReplaceWithObjProperties(singleProperties));
        return cleanedCode;
    }
    public static string GeneratFromObjCollection<T>(this List<T> dataRecords, List<string> fileLines) where T : new()
    {
        string cleanedCode = string.Empty;
        if (fileLines != null && fileLines.Count != 0)
        {
            bool startedBlock = false;
            string loopContent = string.Empty;
            foreach (string line in fileLines)
            {
                try
                {
                    string lineBlock = line.RemoveStylishWhitespaces();
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
                            cleanedCode = string.Format("{0}{1}{2}", cleanedCode, Environment.NewLine, loopContent.ReplaceWithObjProperties(properties));
                        }
                        loopContent = string.Empty;
                    }
                    else if (startedBlock)
                        loopContent = string.Format("{0}{1}{2}", loopContent, Environment.NewLine, lineBlock);
                    else
                        cleanedCode = string.Format("{0}{1}{2}", cleanedCode, Environment.NewLine, lineBlock);
                }
                catch (Exception ex)
                {
                    cleanedCode = string.Format("{0}{1}{2}", cleanedCode, Environment.NewLine, $"------EXCEPTION OCCURRED---- {ex.Message}");
                }

            }
        }
        return cleanedCode;
    }


    public static string ReplaceWithObjProperties(this string value, List<ExtractedObjProperty> properties)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;
        if (properties == null || properties.Count == 0)
            return value;
        foreach (ExtractedObjProperty p in properties)
        {
            string searchKey = Regex.Replace("{{--value--}}", "--value--", p.Name, RegexOptions.IgnoreCase);
            string searchValue = string.Format("{0}", p.StringFormatted);
            //Property is of Type of Enumerable
            if (typeof(IEnumerable).IsAssignableFrom(p.Type) && p.Type != typeof(string))
            {
                AssignObjectChildLoopForTypeEnumerable(ref value, p);
            }
            else
            {

                //Search Key With Custom Formatting e.g. 1,200
                string customValFormatting = Regex.Replace("{{--value--:", "--value--", p.Name, RegexOptions.IgnoreCase);
                string regexF = string.Format(@"\{0}(.+)\}}", customValFormatting);
                Match blockMatch = Regex.Match(value, regexF, RegexOptions.IgnoreCase);
                if (blockMatch.Success)
                {
                    Match regexMatch = Regex.Match(blockMatch.Value, @"{{(.+?)}}", RegexOptions.IgnoreCase);
                    while (regexMatch.Success)
                    {
                        string customFormatting = Regex.Replace("--value--:", "--value--", p.Name, RegexOptions.IgnoreCase);
                        string customFormattingValue = regexMatch.Groups[1].Value.ReplaceMany(new string[] { customFormatting, "}", "{" }, string.Empty);

                        if (p.Type.Equals(typeof(int)))
                            value = Regex.Replace(value, regexMatch.Value, int.Parse(p.StringFormatted).ToString(customFormattingValue), RegexOptions.IgnoreCase);
                        else if (p.Type.Equals(typeof(double)))
                            value = Regex.Replace(value, regexMatch.Value, double.Parse(p.StringFormatted).ToString(customFormattingValue), RegexOptions.IgnoreCase);
                        else if (p.Type.Equals(typeof(long)))
                            value = Regex.Replace(value, regexMatch.Value, long.Parse(p.StringFormatted).ToString(customFormattingValue), RegexOptions.IgnoreCase);
                        else if (p.Type.Equals(typeof(float)))
                            value = Regex.Replace(value, regexMatch.Value, float.Parse(p.StringFormatted).ToString(customFormattingValue), RegexOptions.IgnoreCase);
                        else if (p.Type.Equals(typeof(decimal)))
                            value = Regex.Replace(value, regexMatch.Value, decimal.Parse(p.StringFormatted).ToString(customFormattingValue), RegexOptions.IgnoreCase);
                        else if (p.Type.Equals(typeof(DateTime)))
                            value = Regex.Replace(value, regexMatch.Value, DateTime.Parse(p.StringFormatted).ToString(customFormattingValue), RegexOptions.IgnoreCase);
                        //Custom Formats
                        else if (customFormattingValue.ToLower().Equals("uppercase"))
                            value = Regex.Replace(value, regexMatch.Value, searchValue.ToUpper(), RegexOptions.IgnoreCase);
                        else if (customFormattingValue.ToLower().Equals("lowercase"))
                            value = Regex.Replace(value, regexMatch.Value, searchValue.ToLower(), RegexOptions.IgnoreCase);
                        else
                            value = Regex.Replace(value, regexMatch.Value, searchValue, RegexOptions.IgnoreCase);

                        //Proceed To Next
                        regexMatch = regexMatch.NextMatch();
                    }

                }
                else
                    value = Regex.Replace(value, searchKey, searchValue, RegexOptions.IgnoreCase);

            }

        }
        return value?.Trim();
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
            foreach (string lineBlock in fileLines)
            {
                try
                {
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
                                cleanedCode = string.Format("{0}{1}{2}", cleanedCode, Environment.NewLine, loopContent.ReplaceWithObjProperties(properties));
                            }
                        loopContent = string.Empty;
                    }
                    else if (startedBlock)
                        loopContent = string.Format("{0}{1}{2}", loopContent, Environment.NewLine, lineBlock);
                    else
                        cleanedCode = string.Format("{0}{1}{2}", cleanedCode, Environment.NewLine, lineBlock);
                }
                catch (Exception ex)
                {
                    cleanedCode = string.Format("{0}{1}{2}", cleanedCode, Environment.NewLine, $"------EXCEPTION OCCURRED---- {ex.Message}");
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