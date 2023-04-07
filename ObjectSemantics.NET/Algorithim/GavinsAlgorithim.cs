using ObjectSemantics.NET;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

public static class GavinsAlgorithim
{

    public static string GenerateFromTemplate<T>(T record, TemplatedContent clonedTemplate, List<ObjectSemanticsKeyValue> parameterKeyValues = null, TemplateGeneratorOptions options = null) where T : new()
    {
        //Get Object's Properties
        List<ExtractedObjProperty> objProperties = GetObjectProperties(record, parameterKeyValues);

        #region Replace Obj Loop Attributes
        foreach (ReplaceObjLoopCode objLoop in clonedTemplate.ReplaceObjLoopCodes)
        {
            //First Loop lets look at its target property Object
            ExtractedObjProperty targetObj = objProperties.Where(x => x.IsEnumerableObject).FirstOrDefault(x => x.Name.ToUpper().Equals(objLoop.TargetObjectName.ToUpper()));
            //Since this Object is of Type Enumerable
            if (targetObj != null && targetObj.OriginalValue != null)
            {
                //Loop all data records inside target Property
                string rowTemplate = objLoop.ObjLoopTemplate;
                StringBuilder rowContentTemplater = new StringBuilder();
                foreach (object loopRow in (IEnumerable)targetObj.OriginalValue)
                {
                    List<ExtractedObjProperty> rowRecordValues = GetObjPropertiesFromUnknown(loopRow);
                    //Loop Through class template Loops
                    string activeRow = rowTemplate;
                    foreach (ReplaceCode objLoopCode in objLoop.ReplaceObjCodes)
                    {
                        ExtractedObjProperty objProperty = rowRecordValues.FirstOrDefault(x => x.Name.ToUpper().Equals(objLoopCode.TargetPropertyName.ToUpper()));
                        if (objProperty != null)
                            activeRow = ReplaceFirstOccurrence(activeRow, objLoopCode.ReplaceRef, GetValueFromPropertyFormatted(objProperty, objLoopCode.FormattingCommand));
                        else
                            activeRow = ReplaceFirstOccurrence(activeRow, objLoopCode.ReplaceRef, objLoopCode.ReplaceCommand);
                    }
                    //Append Record row
                    rowContentTemplater.Append(activeRow);
                }

                objLoop.ObjLoopTemplate = rowContentTemplater.ToString().RemoveLastInstanceOfString('\r', '\n'); //Assign Auto Generated
                //Replace the main Loop area
                clonedTemplate.Template = ReplaceFirstOccurrence(clonedTemplate.Template, objLoop.ReplaceRef, objLoop.ObjLoopTemplate);
            }
            else
                clonedTemplate.Template = ReplaceFirstOccurrence(clonedTemplate.Template, objLoop.ReplaceRef, string.Empty);

        }
        #endregion

        #region Replace Direct Target Attributes
        foreach (ReplaceCode replaceCode in clonedTemplate.ReplaceCodes)
        {
            ExtractedObjProperty property = objProperties.FirstOrDefault(x => x.Name.ToUpper().Equals(replaceCode.TargetPropertyName.ToUpper()));
            if (property != null)
                clonedTemplate.Template = ReplaceFirstOccurrence(clonedTemplate.Template, replaceCode.ReplaceRef, GetValueFromPropertyFormatted(property, replaceCode.FormattingCommand));
            else
                clonedTemplate.Template = ReplaceFirstOccurrence(clonedTemplate.Template, replaceCode.ReplaceRef, replaceCode.ReplaceCommand);
        }
        #endregion

        return clonedTemplate.Template;
    }

    internal static TemplatedContent GenerateTemplateFromFile(string fileContent, TemplateGeneratorOptions options)
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
            //Loop Target Object Name
            string targetObjName = regexLoopMatch.Value.Replace("{", string.Empty).Replace("}", string.Empty).Trim().Split(new string[] { ":" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            //Obj
            ReplaceObjLoopCode replaceObjLoopCode = new ReplaceObjLoopCode { ReplaceRef = replaceCode, TargetObjectName = targetObjName };
            //Extra Loop Contents
            Match subMatch = Regex.Match(subBlock, @"{{(.+?)}}", RegexOptions.IgnoreCase);
            while (subMatch.Success)
            {
                //Reclean again
                string actualCmdProperty = string.Format(@"{0}{1}{2}", "{{", subMatch.Groups[1].Value.Trim(), "}}");
                //Skip Loop Block
                if (!actualCmdProperty.StartsWith("{{for-each-start:") && !actualCmdProperty.StartsWith("{{for-each-end:"))
                {
                    //Replace Command Occurrence
                    string replaceSubCode = string.Format("REPLACE_LOOP_RECORD_{0}", Guid.NewGuid().ToString().ToUpper());
                    subBlock = ReplaceFirstOccurrence(subBlock, subMatch.Value, replaceSubCode);
                    replaceObjLoopCode.ReplaceObjCodes.Add(new ReplaceCode { ReplaceCommand = actualCmdProperty, ReplaceRef = replaceSubCode });
                }
                else
                    subBlock = ReplaceFirstOccurrence(subBlock, subMatch.Value, string.Empty);
                //Proceed To Next
                subMatch = subMatch.NextMatch();
            }

            replaceObjLoopCode.ObjLoopTemplate = Regex.Replace(subBlock, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline); //Assigning it here because we are modifying it above
            templatedContent.ReplaceObjLoopCodes.Add(replaceObjLoopCode);
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