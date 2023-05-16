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

    public static string GenerateFromTemplate<T>(T record, TemplatedContent clonedTemplate, List<ObjectSemanticsKeyValue> parameterKeyValues = null, TemplateMapperOptions options = null) where T : new()
    {
        //Get Object's Properties
        List<ExtractedObjProperty> objProperties = GetObjectProperties(record, parameterKeyValues);

        #region Replace If Conditions
        foreach (ReplaceIfOperationCode ifCondition in clonedTemplate.ReplaceIfConditionCodes)
        {
            ExtractedObjProperty property = objProperties.FirstOrDefault(x => x.Name.ToUpper().Equals(ifCondition.IfPropertyName.ToUpper()));
            if (property != null)
            {
                if (property.IsPropertyValueConditionPassed(ifCondition.IfOperationValue, ifCondition.IfOperationType))
                {
                    //Condition Passed
                    TemplatedContent templatedIfContent = GenerateTemplateFromFileContents(ifCondition.IfOperationTrueTemplate, options);
                    string templatedIfContentMapped = GenerateFromTemplate(record, templatedIfContent, parameterKeyValues, options);
                    clonedTemplate.Template = clonedTemplate.Template.ReplaceFirstOccurrence(ifCondition.ReplaceRef, templatedIfContentMapped);
                }
                else if (!string.IsNullOrEmpty(ifCondition.IfOperationFalseTemplate))
                {
                    //If Else Condition Block
                    TemplatedContent templatedIfContent = GenerateTemplateFromFileContents(ifCondition.IfOperationFalseTemplate, options);
                    string templatedIfElseContentMapped = GenerateFromTemplate(record, templatedIfContent, parameterKeyValues, options);
                    clonedTemplate.Template = clonedTemplate.Template.ReplaceFirstOccurrence(ifCondition.ReplaceRef, templatedIfElseContentMapped);
                }
                else
                    clonedTemplate.Template = clonedTemplate.Template.ReplaceFirstOccurrence(ifCondition.ReplaceRef, string.Empty);
            }
            else
                clonedTemplate.Template = clonedTemplate.Template.ReplaceFirstOccurrence(ifCondition.ReplaceRef, $"[IF-CONDITION EXCEPTION]: unrecognized property: [{ifCondition.IfPropertyName}]");
        }
        #endregion

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
                            activeRow = activeRow.ReplaceFirstOccurrence(objLoopCode.ReplaceRef, objProperty.GetPropertyDisplayString(objLoopCode.FormattingCommand, options));
                        else
                            activeRow = activeRow.ReplaceFirstOccurrence(objLoopCode.ReplaceRef, objLoopCode.ReplaceCommand);
                    }
                    //Append Record row
                    rowContentTemplater.Append(activeRow);
                }
                objLoop.ObjLoopTemplate = rowContentTemplater.ToString().RemoveLastInstanceOfString('\r', '\n'); //Assign Auto Generated
                //Replace the main Loop area
                clonedTemplate.Template = clonedTemplate.Template.ReplaceFirstOccurrence(objLoop.ReplaceRef, objLoop.ObjLoopTemplate);
            }
            else
                clonedTemplate.Template = clonedTemplate.Template.ReplaceFirstOccurrence(objLoop.ReplaceRef, string.Empty);

        }
        #endregion

        #region Replace Direct Target Attributes
        foreach (ReplaceCode replaceCode in clonedTemplate.ReplaceCodes)
        {
            ExtractedObjProperty property = objProperties.FirstOrDefault(x => x.Name.ToUpper().Equals(replaceCode.TargetPropertyName.ToUpper()));
            if (property != null)
                clonedTemplate.Template = clonedTemplate.Template.ReplaceFirstOccurrence(replaceCode.ReplaceRef, property.GetPropertyDisplayString(replaceCode.FormattingCommand, options));
            else
                clonedTemplate.Template = clonedTemplate.Template.ReplaceFirstOccurrence(replaceCode.ReplaceRef, @"{{ ##command## }}".Replace("##command##", replaceCode.ReplaceCommand));
        }
        #endregion
        return clonedTemplate.Template;
    }

    internal static TemplatedContent GenerateTemplateFromFileContents(string fileContent, TemplateMapperOptions options)
    {
        TemplatedContent templatedContent = new TemplatedContent { Template = fileContent };
        long _replaceKey = 0;
        #region If Condition
        //Matches ==, !=, >, >=, <, and <=
        string _matchWithElseIf = @"{{\s*#if\s*\(\s*(?<param>\w+)\s*(?<operator>==|!=|>=|<=|>|<)\s*(?<value>[^)]+)\s*\)\s*}}(?<code>[\s\S]*?)(?:{{\s*#else\s*}}(?<else>[\s\S]*?))?{{\s*#endif\s*}}";
        while (Regex.IsMatch(templatedContent.Template, _matchWithElseIf, RegexOptions.IgnoreCase))
        {
            templatedContent.Template = Regex.Replace(templatedContent.Template, _matchWithElseIf, match =>
            {
                _replaceKey++;
                string _replaceCode = string.Format("RIB_{0}", _replaceKey);
                templatedContent.ReplaceIfConditionCodes.Add(new ReplaceIfOperationCode
                {
                    ReplaceRef = _replaceCode,
                    IfPropertyName = match.Groups[1].Value?.ToString().Trim().ToLower().Replace(" ", string.Empty),
                    IfOperationType = match.Groups[2].Value?.ToString().Trim().ToLower().Replace(" ", string.Empty),
                    IfOperationValue = match.Groups[3].Value?.ToString().Trim(),
                    IfOperationTrueTemplate = match.Groups[4].Value?.ToString(),
                    IfOperationFalseTemplate = (match.Groups.Count >= 6) ? match.Groups[5].Value?.ToString() : string.Empty
                });
                // Return Replacement
                return _replaceCode;
            }, RegexOptions.IgnoreCase);
        }
        #endregion

        #region Generate Obj Looop
        string _matchLoopBlock = @"{{\s*#\s*foreach\s*\(\s*(\w+)\s*\)\s*\}\}([\s\S]*?)\{\{\s*#\s*endforeach\s*}}";
        while (Regex.IsMatch(templatedContent.Template, _matchLoopBlock, RegexOptions.IgnoreCase))
        {
            templatedContent.Template = Regex.Replace(templatedContent.Template, _matchLoopBlock, match =>
            {
                _replaceKey++;
                string _replaceCode = string.Format("RLB_{0}", _replaceKey);
                ReplaceObjLoopCode objLoopCode = new ReplaceObjLoopCode
                {
                    ReplaceRef = _replaceCode,
                    TargetObjectName = match.Groups[1].Value?.ToString().Trim().ToLower().Replace(" ", string.Empty)
                };
                //Determine Forloop Elements
                string loopBlock = match.Groups[2].Value?.ToString() ?? string.Empty;
                string loopBlockRegex = @"{{(.+?)}}";
                while (Regex.IsMatch(loopBlock, loopBlockRegex, RegexOptions.IgnoreCase))
                {
                    loopBlock = Regex.Replace(loopBlock, loopBlockRegex, loopBlockMatch =>
                    {
                        _replaceKey++;
                        string _replaceLoopBlockCode = string.Format("RLBR_{0}", _replaceKey);
                        objLoopCode.ReplaceObjCodes.Add(new ReplaceCode
                        {
                            ReplaceCommand = loopBlockMatch.Groups[1].Value?.Trim(),
                            ReplaceRef = _replaceLoopBlockCode
                        });
                        return _replaceLoopBlockCode;
                    }, RegexOptions.IgnoreCase);
                }
                //#Just before return 
                objLoopCode.ObjLoopTemplate = loopBlock;
                templatedContent.ReplaceObjLoopCodes.Add(objLoopCode);
                // Return Replacement
                return _replaceCode;
            }, RegexOptions.IgnoreCase);
        }

        #endregion

        #region Generate direct targets
        string _paramRegex = @"{{(.+?)}}";
        while (Regex.IsMatch(templatedContent.Template, _paramRegex, RegexOptions.IgnoreCase))
        {
            templatedContent.Template = Regex.Replace(templatedContent.Template, _paramRegex, propertyMatch =>
            {
                _replaceKey++;
                string _replaceCode = string.Format("RP_{0}", _replaceKey);
                templatedContent.ReplaceCodes.Add(new ReplaceCode
                {
                    ReplaceCommand = propertyMatch.Groups[1].Value?.Trim(),
                    ReplaceRef = _replaceCode
                });
                return _replaceCode;
            }, RegexOptions.IgnoreCase);
        }

        #endregion

        return templatedContent;
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


}