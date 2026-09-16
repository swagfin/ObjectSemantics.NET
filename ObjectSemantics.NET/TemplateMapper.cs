using ObjectSemantics.NET.Engine;
using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections.Generic;

namespace ObjectSemantics.NET
{
    public static class TemplateMapper
    {
        /// <summary>Prepare a reusable template. Options validate compilation; pass render options separately.</summary>
        public static CompiledTemplate Compile(string template, TemplateMapperOptions options = null)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            TemplateMapperOptions activeOptions = (options ?? new TemplateMapperOptions()).Snapshot();
            EngineTemplateRenderer.CheckSource(template, activeOptions);
            EngineRunnerTemplate parsed = EngineTemplateCache.GetOrAdd(template, EngineTemplateParser.Parse);
            EngineTemplateRenderer.CheckTemplate(parsed, activeOptions);
            return new CompiledTemplate(parsed);
        }

        /// <summary>Validate syntax without evaluating model getters. Missing model values are checked by strict rendering.</summary>
        public static IReadOnlyList<TemplateDiagnostic> Validate(string template)
        {
            return Compile(template).Diagnostics;
        }

        /// <summary>Replaces the process-wide template cache. Existing renders remain valid.</summary>
        public static void ConfigureCache(int capacity = 2048, long maximumSourceCharacters = 16777216)
        {
            EngineTemplateCache.Configure(capacity, maximumSourceCharacters);
        }

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
            options = (options ?? new TemplateMapperOptions()).Snapshot();
            EngineTemplateRenderer.CheckSource(template, options);
            EngineRunnerTemplate runnerTemplate = EngineTemplateCache.GetOrAdd(template, EngineTemplateParser.Parse);
            return EngineTemplateRenderer.Render(record, runnerTemplate, additionalKeyValues, options);
        }
    }
}
