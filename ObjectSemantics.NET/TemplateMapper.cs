using ObjectSemantics.NET.Engine;
using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections.Generic;

namespace ObjectSemantics.NET
{
    public static class TemplateMapper
    {
        /// <summary>
        /// Generate a mapped string from string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="template"></param>
        /// <param name="record"></param>
        /// <param name="additionalKeyValues"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static string Map<T>(this string template, T record, Dictionary<string, object> additionalKeyValues = null, TemplateMapperOptions options = null) where T : class, new()
        {
            return Map(record, template, additionalKeyValues, options);
        }

        /// <summary>
        /// Generates a mapped string from a T record and a template
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record"></param>
        /// <param name="template"></param>
        /// <param name="additionalKeyValues"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string Map<T>(this T record, string template, Dictionary<string, object> additionalKeyValues = null, TemplateMapperOptions options = null) where T : class, new()
        {
            if (record == null) return string.Empty;
            if (template == null) throw new Exception("Template Object can't be NULL");
            if (options == null) options = new TemplateMapperOptions();
            EngineRunnerTemplate runnerTemplate = EngineAlgorithim.GenerateRunnerTemplate(template);
            return runnerTemplate == null ? throw new Exception($"Error Mapping!") : EngineAlgorithim.GenerateFromTemplate(record, runnerTemplate, additionalKeyValues, options);
        }
    }
}