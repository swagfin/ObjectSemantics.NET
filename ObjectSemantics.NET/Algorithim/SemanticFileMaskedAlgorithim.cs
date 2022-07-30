using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

internal static class SemanticFileMaskedAlgorithim
{
    public static string GeneratFromObj<T>(this T record, List<string> fileLines) where T : new()
    {
        return new List<T> { record }.GeneratFromObjCollection(fileLines);
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
                    string lineBlock = line;
                    //Remove Whitespaces for stylish people e..g {{    code   }} instead if {{code}}
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
                    //Proceed
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

        //Replace the Contents Again
        try
        {
            var singleProperties = GetObjProperties(dataRecords.FirstOrDefault());
            cleanedCode = cleanedCode.ReplaceWithObjProperties(singleProperties);
        }
        catch { }
        return cleanedCode;
    }
    public static List<ExtractedObjProperty> GetObjProperties<T>(T value) where T : new()
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
        return value;
    }

    public static string ReplaceMany(this string inputString, string[] find, string replaceWith, RegexOptions regexOptions = RegexOptions.IgnoreCase)
    {
        if (find.Length == 0)
            return inputString;
        foreach (var pattern in find)
            inputString = Regex.Replace(inputString, pattern, replaceWith, regexOptions);
        return inputString;
    }
}