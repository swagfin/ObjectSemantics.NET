using ObjectSemantics.NET.Engine.Extensions;
using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ObjectSemantics.NET.Engine
{
    internal static class EngineTemplateRenderer
    {
        public static string Render<T>(T record, EngineRunnerTemplate template, Dictionary<string, object> parameterKeyValues = null, TemplateMapperOptions options = null) where T : new()
        {
            EngineRunnerTemplate activeTemplate = template ?? new EngineRunnerTemplate();
            Dictionary<string, ExtractedObjProperty> propMap = EngineTypeMetadataCache.BuildPropertyMap(record, typeof(T), parameterKeyValues);

            string templateText = activeTemplate.Template ?? string.Empty;
            StringBuilder result = new StringBuilder(templateText, templateText.Length * 2);

            List<ReplaceIfOperationCode> ifConditions = activeTemplate.ReplaceIfConditionCodes;
            for (int i = 0; i < ifConditions.Count; i++)
            {
                ReplaceIfOperationCode ifCondition = ifConditions[i];
                if (!EnginePropertyResolver.TryResolveProperty(propMap, ifCondition.IfPropertyName, out ExtractedObjProperty property))
                {
                    result.ReplaceFirstOccurrence(ifCondition.ReplaceRef, "[IF-CONDITION EXCEPTION]: unrecognized property: [" + ifCondition.IfPropertyName + "]");
                    continue;
                }

                bool conditionPassed = property.IsPropertyValueConditionPassed(ifCondition.IfOperationValue, ifCondition.IfOperationType);
                string replacement;

                if (conditionPassed)
                {
                    EngineRunnerTemplate trueContent = EngineAlgorithim.GenerateRunnerTemplate(ifCondition.IfOperationTrueTemplate);
                    replacement = Render(record, trueContent, parameterKeyValues, options);
                }
                else if (!string.IsNullOrEmpty(ifCondition.IfOperationFalseTemplate))
                {
                    EngineRunnerTemplate falseContent = EngineAlgorithim.GenerateRunnerTemplate(ifCondition.IfOperationFalseTemplate);
                    replacement = Render(record, falseContent, parameterKeyValues, options);
                }
                else
                {
                    replacement = string.Empty;
                }

                result.ReplaceFirstOccurrence(ifCondition.ReplaceRef, replacement);
            }

            List<ReplaceObjLoopCode> loopCodes = activeTemplate.ReplaceObjLoopCodes;
            for (int loopIndex = 0; loopIndex < loopCodes.Count; loopIndex++)
            {
                ReplaceObjLoopCode objLoop = loopCodes[loopIndex];
                if (!EnginePropertyResolver.TryResolveProperty(propMap, objLoop.TargetObjectName, out ExtractedObjProperty targetObj) || !(targetObj.OriginalValue is IEnumerable enumerable))
                {
                    result.ReplaceFirstOccurrence(objLoop.ReplaceRef, string.Empty);
                    continue;
                }

                StringBuilder loopResult = new StringBuilder();
                List<ReplaceCode> objLoopReplaceCodes = objLoop.ReplaceObjCodes;

                foreach (object row in enumerable)
                {
                    Dictionary<string, ExtractedObjProperty> rowMap = null;
                    Type rowType = row != null ? row.GetType() : typeof(object);
                    ExtractedObjProperty currentRowProperty = null;
                    StringBuilder activeRow = new StringBuilder(objLoop.ObjLoopTemplate ?? string.Empty);

                    for (int codeIndex = 0; codeIndex < objLoopReplaceCodes.Count; codeIndex++)
                    {
                        ReplaceCode objLoopCode = objLoopReplaceCodes[codeIndex];
                        string propName = objLoopCode.TargetPropertyName ?? objLoopCode.GetTargetPropertyName();
                        string formattingCommand = objLoopCode.FormattingCommand ?? objLoopCode.GetFormattingCommand();

                        if (propName == ".")
                        {
                            if (currentRowProperty == null)
                            {
                                currentRowProperty = new ExtractedObjProperty
                                {
                                    Name = ".",
                                    Type = rowType,
                                    OriginalValue = row
                                };
                            }

                            activeRow.ReplaceFirstOccurrence(objLoopCode.ReplaceRef, currentRowProperty.GetPropertyDisplayString(formattingCommand, options));
                        }
                        else
                        {
                            if (rowMap == null)
                                rowMap = EngineTypeMetadataCache.BuildPropertyMap(row, rowType, null);

                            if (EnginePropertyResolver.TryResolveProperty(rowMap, propName, out ExtractedObjProperty rowProperty))
                                activeRow.ReplaceFirstOccurrence(objLoopCode.ReplaceRef, rowProperty.GetPropertyDisplayString(formattingCommand, options));
                            else if (EngineExpressionEvaluator.TryEvaluate(propName, row, rowMap, out ExtractedObjProperty expressionProperty, out bool renderEmptyOnExpressionFailure, out bool isExpressionCommand))
                                activeRow.ReplaceFirstOccurrence(objLoopCode.ReplaceRef, expressionProperty.GetPropertyDisplayString(formattingCommand, options));
                            else if (isExpressionCommand && renderEmptyOnExpressionFailure)
                                activeRow.ReplaceFirstOccurrence(objLoopCode.ReplaceRef, string.Empty);
                            else
                                activeRow.ReplaceFirstOccurrence(objLoopCode.ReplaceRef, objLoopCode.ReplaceCommand);
                        }
                    }

                    loopResult.Append(activeRow);
                }

                result.ReplaceFirstOccurrence(objLoop.ReplaceRef, loopResult.ToString());
            }

            List<ReplaceCode> replaceCodes = activeTemplate.ReplaceCodes;
            for (int i = 0; i < replaceCodes.Count; i++)
            {
                ReplaceCode replaceCode = replaceCodes[i];
                string targetPropertyName = replaceCode.TargetPropertyName ?? replaceCode.GetTargetPropertyName();
                string formattingCommand = replaceCode.FormattingCommand ?? replaceCode.GetFormattingCommand();

                if (EnginePropertyResolver.TryResolveProperty(propMap, targetPropertyName, out ExtractedObjProperty property))
                    result.ReplaceFirstOccurrence(replaceCode.ReplaceRef, property.GetPropertyDisplayString(formattingCommand, options));
                else if (EngineExpressionEvaluator.TryEvaluate(targetPropertyName, record, propMap, out ExtractedObjProperty expressionProperty, out bool renderEmptyOnExpressionFailure, out bool isExpressionCommand))
                    result.ReplaceFirstOccurrence(replaceCode.ReplaceRef, expressionProperty.GetPropertyDisplayString(formattingCommand, options));
                else if (isExpressionCommand && renderEmptyOnExpressionFailure)
                    result.ReplaceFirstOccurrence(replaceCode.ReplaceRef, string.Empty);
                else
                    result.ReplaceFirstOccurrence(replaceCode.ReplaceRef, "{{ " + replaceCode.ReplaceCommand + " }}");
            }

            return result.ToString();
        }
    }
}
