using System;
using System.Collections.Generic;

namespace ObjectSemantics.NET
{
    public class ObjectSemantics
    {
        private readonly ObjectSemanticsOptions _options;
        public ObjectSemantics()
        {
            this._options = new ObjectSemanticsOptions();
        }
        public ObjectSemantics(ObjectSemanticsOptions objectSemanticsOptions)
        {
            this._options = objectSemanticsOptions ?? new ObjectSemanticsOptions();
        }
        /// <summary>
        /// Generate a Data Template From Object Properties
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="record">Single Record of T that may include a Collection inside it</param>
        /// <param name="template">Template File containing Template Name and Template Contents</param>
        /// <param name="additionalKeyValues">Additional Key Value parameters that you may need mapped to file</param>
        /// <returns></returns>
        public string GenerateTemplate<T>(T record, ObjectSemanticsTemplate template, List<ObjectSemanticsKeyValue> additionalKeyValues = null) where T : new()
        {
            if (record == null) return string.Empty;
            if (template == null) throw new Exception("Template Object can't be NULL");
            TemplatedContent templatedContent = GavinsAlgorithim.GenerateTemplateFromFile(template.FileContents);
            if (templatedContent == null) throw new Exception($"Error Generating template from specified Template Name: {template.Name}");
            return GavinsAlgorithim.GenerateFromTemplate(record, templatedContent, additionalKeyValues);
        }
    }
}