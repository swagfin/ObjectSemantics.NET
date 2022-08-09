using ObjectSemantics.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

public static class GavinsAlgorithim
{

    public static string GenerateFromTemplate<T>(T record, TemplatedContent originTemplateContent, List<ObjectSemanticsKeyValue> parameterKeyValues = null) where T : new()
    {
        //To Prevent from Modifying 
        TemplatedContent clonedTemplate = new TemplatedContent { Template = originTemplateContent.Template, ReplaceCodes = originTemplateContent.ReplaceCodes };
        //Get Object's Properties
        List<ExtractedObjProperty> objProperties = GetObjectProperties(record, parameterKeyValues);

        ReplaceObjectTemplateExecutables(clonedTemplate, objProperties);

        return clonedTemplate.Template;
    }

    private static void ReplaceObjectTemplateExecutables(TemplatedContent clonedTemplate, List<ExtractedObjProperty> objProperties)
    {
        foreach (ReplaceCode replaceCode in clonedTemplate.ReplaceCodes)
        {
            ExtractedObjProperty property = objProperties.FirstOrDefault(x => x.Name.ToUpper().Equals(replaceCode.TargetPropertyName.ToUpper()));
            if (property != null)
                clonedTemplate.Template = ReplaceFirstOccurrence(clonedTemplate.Template, replaceCode.ReplaceRef, GetValueFromPropertyFormatted(property, replaceCode.FormattingCommand));
            else
                clonedTemplate.Template = ReplaceFirstOccurrence(clonedTemplate.Template, replaceCode.ReplaceRef, replaceCode.ReplaceCommand);
        }
    }

    internal static TemplatedContent GenerateTemplateFromFile(string fileContent)
    {
        TemplatedContent templatedContent = new TemplatedContent { Template = fileContent };

        #region Generate Obj Looop
        Match regexLoopMatch = Regex.Match(templatedContent.Template, "{{(.+?)for-each-start:(.+?)}}", RegexOptions.IgnoreCase);
        Match regexLoopEnd = Regex.Match(templatedContent.Template, "{{(.+?)for-each-end:(.+?)}}", RegexOptions.IgnoreCase);
        while (regexLoopMatch.Success && regexLoopEnd.Success)
        {
            int startAtIndex = templatedContent.Template.IndexOf(regexLoopMatch.Value); //Getting again index just incase it was replaced
            int endOfCodeIndex = templatedContent.Template.IndexOf(regexLoopEnd.Value); //Getting again index just incase it was replaced
            int endAtIndex = (endOfCodeIndex + regexLoopEnd.Length) - startAtIndex;
            string subBlock = templatedContent.Template.Substring(startAtIndex, endAtIndex);
            //Replace Code Block
            string replaceCode = string.Format("REPLACE_LOOP_{0}", Guid.NewGuid().ToString().ToUpper());
            templatedContent.Template = Regex.Replace(templatedContent.Template, subBlock, replaceCode, RegexOptions.IgnoreCase);
            //Obj

            //Move Next (Both)
            regexLoopMatch = regexLoopMatch.NextMatch();
            regexLoopEnd = regexLoopEnd.NextMatch();
        }


        #endregion

        #region Generate direct targets
        Match regexMatch = Regex.Match(templatedContent.Template, @"{{(.+?)}}", RegexOptions.IgnoreCase);
        while (regexMatch.Success)
        {
            //Reclean again
            string actualCmdProperty = string.Format(@"{0}{1}{2}", "{{", regexMatch.Groups[1].Value.Trim(), "}}");
            //Replace Command Occurrence
            string replaceCode = string.Format("REPLACE_{0}", Guid.NewGuid().ToString().ToUpper());
            templatedContent.Template = ReplaceFirstOccurrence(templatedContent.Template, regexMatch.Value, replaceCode);
            templatedContent.ReplaceCodes.Add(new ReplaceCode { ReplaceCommand = actualCmdProperty, ReplaceRef = replaceCode });
            //Proceed To Next
            regexMatch = regexMatch.NextMatch();
        }
        #endregion

        return templatedContent;
    }

    private static string ReplaceFirstOccurrence(this string text, string search, string replace)
    {
        int pos = text.IndexOf(search);
        if (pos < 0)
            return text;
        return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
    }
    private static List<ExtractedObjProperty> GetObjectProperties<T>(T value, List<ObjectSemanticsKeyValue> parameters = null) where T : new()
    {
        List<ExtractedObjProperty> extractedObjProperties = new List<ExtractedObjProperty>();
        foreach (PropertyInfo prop in typeof(T).GetProperties())
        {
            try
            {
                extractedObjProperties.Add(new ExtractedObjProperty
                {
                    Type = prop.PropertyType,
                    Name = prop.Name,
                    OriginalValue = value == null ? null : prop.GetValue(value)
                });
            }
            catch { }
        }
        //Append Parameters
        if (parameters != null && parameters.Count != 0)
            foreach (var param in parameters)
                extractedObjProperties.Add(new ExtractedObjProperty { Type = param.Type, Name = param.Key, OriginalValue = param.Value });
        return extractedObjProperties;
    }

    private static string GetValueFromPropertyFormatted(ExtractedObjProperty p, string customFormattingValue)
    {
        if (string.IsNullOrWhiteSpace(customFormattingValue))
            return p.StringFormatted;
        if (p.Type.Equals(typeof(int)))
            return int.Parse(p.StringFormatted).ToString(customFormattingValue);
        else if (p.Type.Equals(typeof(double)))
            return double.Parse(p.StringFormatted).ToString(customFormattingValue);
        else if (p.Type.Equals(typeof(long)))
            return long.Parse(p.StringFormatted).ToString(customFormattingValue);
        else if (p.Type.Equals(typeof(float)))
            return float.Parse(p.StringFormatted).ToString(customFormattingValue);
        else if (p.Type.Equals(typeof(decimal)))
            return decimal.Parse(p.StringFormatted).ToString(customFormattingValue);
        else if (p.Type.Equals(typeof(DateTime)))
            return DateTime.Parse(p.StringFormatted).ToString(customFormattingValue);
        //Custom Formats
        else if (customFormattingValue.ToLower().Equals("uppercase"))
            return p.StringFormatted?.ToUpper();
        else if (customFormattingValue.ToLower().Equals("lowercase"))
            return p.StringFormatted?.ToLower();
        else
            return p.StringFormatted?.ToUpper();
    }
}

public class TemplatedContent
{
    public string Template { get; set; }
    public List<ReplaceObjLoopCode> ReplaceObjLoopCodes { get; set; } = new List<ReplaceObjLoopCode>();
    public List<ReplaceCode> ReplaceCodes { get; set; } = new List<ReplaceCode>();
}
public class ReplaceObjLoopCode
{
    public string ReplaceRef { get; set; }
    public string ObjLoopTemplate { get; set; }
    public List<ReplaceCode> ReplaceCodes { get; set; } = new List<ReplaceCode>();
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
