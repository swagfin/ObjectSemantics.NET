using System;
using System.Collections.Generic;
using System.IO;

namespace ObjectSemantics.NET.Logic
{
    public class ObjectSemanticsLogic : IObjectSemantics
    {
        private ObjectSemanticsOptions Options { get; set; } = new ObjectSemanticsOptions();
        private string DefaultTemplatesPath { get; } = Path.Combine(Environment.CurrentDirectory, "Templates");

        public ObjectSemanticsLogic(ObjectSemanticsOptions objectSemanticsOptions)
        {
            this.Options = objectSemanticsOptions;
            if (this.Options.CreateTemplatesDirectoryIfNotExist)
                RunDirectoryChecks();
        }
        private void RunDirectoryChecks()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.Options.TemplatesDirectory))
                    this.Options.TemplatesDirectory = DefaultTemplatesPath;
                if (!Directory.Exists(this.Options.TemplatesDirectory))
                    Directory.CreateDirectory(this.Options.TemplatesDirectory);
            }
            catch { }
        }

        private TemplatedContent GetTemplateContents(string templateName)
        {
            //Look in Directory 
            string altenativePath = Path.Combine(this.Options.TemplatesDirectory, templateName);
            if (!File.Exists(altenativePath))
                throw new Exception($"Template doesn't seem to exist in directory: {altenativePath}");
            string allFileContents = File.ReadAllText(altenativePath);
            TemplatedContent template = GavinsAlgorithim.GenerateTemplateFromFile(allFileContents);
            if (template == null)
                throw new Exception($"Error Generating template from specified File: {altenativePath}");
            return template;
        }
        public string GenerateTemplate<T>(T record, string templateName, List<ObjectSemanticsKeyValue> additionalKeyValues = null) where T : new()
        {
            if (record == null)
                return string.Empty;
            TemplatedContent template = GetTemplateContents(templateName);
            return GavinsAlgorithim.GenerateFromTemplate(record, template, additionalKeyValues);
        }
    }
}