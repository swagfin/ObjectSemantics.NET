using System;
using System.Collections.Generic;

namespace ObjectSemantics.NET
{
    public static class TemplateMapper
    {
        /// <summary>
        /// Generate a Data Template From Object Properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record">Single Record of T that may include a Collection inside it</param>
        /// <param name="template">Template File containing Template Name and Template Contents</param>
        /// <param name="additionalKeyValues">Additional Key Value parameters that you may need mapped to file</param>
        /// <param name="options">Custom Options and configurations for the Template Generator</param>
        /// <returns></returns>
        public static string Map<T>(T record, ObjectSemanticsTemplate template, List<ObjectSemanticsKeyValue> additionalKeyValues = null, TemplateMapperOptions options = null) where T : new()
        {
            if (record == null) return string.Empty;
            if (template == null) throw new Exception("Template Object can't be NULL");
            if (options == null) options = new TemplateMapperOptions();
            TemplatedContent templatedContent = GavinsAlgorithim.GenerateTemplateFromFile(template.FileContents, options);
            if (templatedContent == null) throw new Exception($"Error Generating template from specified Template Name: {template.Name}");
            return GavinsAlgorithim.GenerateFromTemplate(record, templatedContent, additionalKeyValues, options);
        }
    }
}