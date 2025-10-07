using ObjectSemantics.NET.Engine.Extensions;
using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace ObjectSemantics.NET.Engine
{
    internal static class EngineAlgorithim
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();

        private static readonly Regex IfConditionRegex = new Regex(@"{{\s*#if\s*\(\s*(?<param>\w+)\s*(?<operator>==|!=|>=|<=|>|<)\s*(?<value>[^)]+)\s*\)\s*}}(?<code>[\s\S]*?)(?:{{\s*#else\s*}}(?<else>[\s\S]*?))?{{\s*#endif\s*}}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex LoopBlockRegex = new Regex(@"{{\s*#foreach\s*\(\s*(\w+)\s*\)\s*\}\}([\s\S]*?)\{\{\s*#endforeach\s*}}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex DirectParamRegex = new Regex(@"{{(.+?)}}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string GenerateFromTemplate<T>(T record, EngineRunnerTemplate template, Dictionary<string, object> parameterKeyValues = null, TemplateMapperOptions options = null) where T : new()
        {
            List<ExtractedObjProperty> objProperties = GetObjectProperties(record, parameterKeyValues);
            Dictionary<string, ExtractedObjProperty> propMap = objProperties.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

            StringBuilder result = new StringBuilder(template.Template ?? string.Empty, (template.Template?.Length ?? 0) * 2);

            // ---- IF Conditions ----
            foreach (ReplaceIfOperationCode ifCondition in template.ReplaceIfConditionCodes)
            {
                if (!propMap.TryGetValue(ifCondition.IfPropertyName, out ExtractedObjProperty property))
                {
                    result.Replace(ifCondition.ReplaceRef, "[IF-CONDITION EXCEPTION]: unrecognized property: [" + ifCondition.IfPropertyName + "]");
                    continue;
                }

                bool conditionPassed = property.IsPropertyValueConditionPassed(ifCondition.IfOperationValue, ifCondition.IfOperationType);
                string replacement;

                if (conditionPassed)
                {
                    EngineRunnerTemplate trueContent = GenerateRunnerTemplate(ifCondition.IfOperationTrueTemplate);
                    replacement = GenerateFromTemplate(record, trueContent, parameterKeyValues, options);
                }
                else if (!string.IsNullOrEmpty(ifCondition.IfOperationFalseTemplate))
                {
                    EngineRunnerTemplate falseContent = GenerateRunnerTemplate(ifCondition.IfOperationFalseTemplate);
                    replacement = GenerateFromTemplate(record, falseContent, parameterKeyValues, options);
                }
                else
                {
                    replacement = string.Empty;
                }

                result.Replace(ifCondition.ReplaceRef, replacement);
            }

            // ---- Object Loops ----
            foreach (ReplaceObjLoopCode objLoop in template.ReplaceObjLoopCodes)
            {
                if (!propMap.TryGetValue(objLoop.TargetObjectName, out ExtractedObjProperty targetObj) || !(targetObj.OriginalValue is IEnumerable enumerable))
                {
                    result.Replace(objLoop.ReplaceRef, string.Empty);
                    continue;
                }

                StringBuilder loopResult = new StringBuilder();

                foreach (object row in enumerable)
                {
                    List<ExtractedObjProperty> rowProps = GetObjPropertiesFromUnknown(row);
                    Dictionary<string, ExtractedObjProperty> rowMap = rowProps.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

                    StringBuilder activeRow = new StringBuilder(objLoop.ObjLoopTemplate);

                    foreach (ReplaceCode objLoopCode in objLoop.ReplaceObjCodes)
                    {
                        string propName = objLoopCode.GetTargetPropertyName();
                        if (propName == ".")
                        {
                            ExtractedObjProperty tempProp = new ExtractedObjProperty
                            {
                                Name = ".",
                                Type = row.GetType(),
                                OriginalValue = row
                            };
                            activeRow.Replace(objLoopCode.ReplaceRef, tempProp.GetPropertyDisplayString(objLoopCode.GetFormattingCommand(), options));
                        }
                        else
                        {
                            if (rowMap.TryGetValue(propName, out ExtractedObjProperty p))
                                activeRow.Replace(objLoopCode.ReplaceRef, p.GetPropertyDisplayString(objLoopCode.GetFormattingCommand(), options));
                            else
                                activeRow.Replace(objLoopCode.ReplaceRef, objLoopCode.ReplaceCommand);
                        }
                    }

                    loopResult.Append(activeRow);
                }

                result.Replace(objLoop.ReplaceRef, loopResult.ToString());
            }

            // ---- Direct Replacements ----
            foreach (ReplaceCode replaceCode in template.ReplaceCodes)
            {
                if (propMap.TryGetValue(replaceCode.GetTargetPropertyName(), out ExtractedObjProperty property))
                    result.Replace(replaceCode.ReplaceRef, property.GetPropertyDisplayString(replaceCode.GetFormattingCommand(), options));
                else
                    result.Replace(replaceCode.ReplaceRef, "{{ " + replaceCode.ReplaceCommand + " }}");
            }
            return result.ToString();
        }

        internal static EngineRunnerTemplate GenerateRunnerTemplate(string fileContent)
        {
            EngineRunnerTemplate templatedContent = new EngineRunnerTemplate { Template = fileContent };
            long key = 0;

            // ---- IF Conditions ----
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

            // ---- FOREACH Loops ----
            templatedContent.Template = LoopBlockRegex.Replace(templatedContent.Template, m =>
            {
                key++;
                string refKey = "RLB_" + key;
                ReplaceObjLoopCode objLoop = new ReplaceObjLoopCode
                {
                    ReplaceRef = refKey,
                    TargetObjectName = m.Groups[1].Value?.Trim() ?? string.Empty
                };

                string loopBlock = m.Groups[2].Value.Trim('\r', '\n');
                loopBlock = DirectParamRegex.Replace(loopBlock, pm =>
                {
                    key++;
                    string loopRef = "RLBR_" + key;
                    objLoop.ReplaceObjCodes.Add(new ReplaceCode
                    {
                        ReplaceCommand = pm.Groups[1].Value.Trim(),
                        ReplaceRef = loopRef
                    });
                    return loopRef;
                });

                objLoop.ObjLoopTemplate = loopBlock;
                templatedContent.ReplaceObjLoopCodes.Add(objLoop);
                return refKey;
            });

            // ---- Direct Parameters ----
            templatedContent.Template = DirectParamRegex.Replace(templatedContent.Template, m =>
            {
                key++;
                string refKey = "RP_" + key;
                templatedContent.ReplaceCodes.Add(new ReplaceCode
                {
                    ReplaceCommand = m.Groups[1].Value.Trim(),
                    ReplaceRef = refKey
                });
                return refKey;
            });

            return templatedContent;
        }

        private static List<ExtractedObjProperty> GetObjectProperties<T>(T value, Dictionary<string, object> parameters) where T : new()
        {
            Type type = typeof(T);
            if (!PropertyCache.TryGetValue(type, out PropertyInfo[] props))
            {
                props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                PropertyCache[type] = props;
            }

            List<ExtractedObjProperty> result = new List<ExtractedObjProperty>(props.Length + (parameters != null ? parameters.Count : 0));

            foreach (PropertyInfo prop in props)
            {
                result.Add(new ExtractedObjProperty
                {
                    Type = prop.PropertyType,
                    Name = prop.Name,
                    OriginalValue = value == null ? null : prop.GetValue(value, null)
                });
            }

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> p in parameters)
                    result.Add(new ExtractedObjProperty { Type = p.Value.GetType(), Name = p.Key, OriginalValue = p.Value });
            }

            return result;
        }

        private static List<ExtractedObjProperty> GetObjPropertiesFromUnknown(object value)
        {
            if (value == null) return new List<ExtractedObjProperty>();

            Type type = value.GetType();
            if (!PropertyCache.TryGetValue(type, out PropertyInfo[] props))
            {
                props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.GetIndexParameters().Length == 0).ToArray();
                PropertyCache[type] = props;
            }

            List<ExtractedObjProperty> result = new List<ExtractedObjProperty>(props.Length);
            foreach (PropertyInfo prop in props)
            {
                result.Add(new ExtractedObjProperty
                {
                    Type = prop.PropertyType,
                    Name = prop.Name,
                    OriginalValue = prop.GetValue(value, null)
                });
            }

            return result;
        }
    }
}