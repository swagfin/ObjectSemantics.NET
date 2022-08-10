using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ObjectSemantics.NET.Logic
{
    public class ObjectSemanticsLogic : IObjectSemantics
    {
        private ObjectSemanticsOptions Options { get; set; } = new ObjectSemanticsOptions();
        private ConcurrentDictionary<string, TemplatedContent> TemplateFiles { get; set; } = new ConcurrentDictionary<string, TemplatedContent>();
        private string DefaultTemplatesPath { get; } = Path.Combine(Environment.CurrentDirectory, "Templates");

        public ObjectSemanticsLogic(ObjectSemanticsOptions objectSemanticsOptions)
        {
            this.Options = objectSemanticsOptions;
            if (this.Options.CreateTemplatesDirectoryIfNotExist)
                RunDirectoryChecks();
            if (Options.ReserveTemplatesInMemory)
                InitTemplatesToMemory();
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
            if (this.TemplateFiles.TryGetValue(templateName, out TemplatedContent _templateContents))
                return _templateContents;
            //Look in Directory 
            string altenativePath = Path.Combine(this.Options.TemplatesDirectory, templateName);
            if (!File.Exists(altenativePath))
                throw new Exception($"Template doesn't seem to exist in directory: {altenativePath}");
            string allFileContents = File.ReadAllText(altenativePath);
            TemplatedContent template = GavinsAlgorithim.GenerateTemplateFromFile(allFileContents);
            if (template == null)
                throw new Exception($"Error Generating template from specified File: {altenativePath}");
            //Check if Memory persistance Enabled
            if (this.Options.ReserveTemplatesInMemory)
                this.TemplateFiles.TryAdd(templateName, template);
            return template;
        }
        private void InitTemplatesToMemory()
        {
            try
            {
                List<FileInfo> allTemplateFiles = Directory.GetFiles(this.Options.TemplatesDirectory, "*", SearchOption.AllDirectories)
                .Select(f => new FileInfo(f))
                .Where(f => this.Options.SupportedTemplateFileExtensions.Contains(f.Extension))
                .ToList();
                //Attenpt Fetch All Old Files
                if (allTemplateFiles.Count > 0)
                    foreach (FileInfo file in allTemplateFiles)
                        try
                        {
                            string allFileContents = File.ReadAllText(file.FullName);
                            TemplatedContent template = GavinsAlgorithim.GenerateTemplateFromFile(allFileContents);
                            if (template != null)
                                this.TemplateFiles.TryAdd(file.Name, template);
                        }
                        catch { }
            }
            catch { }
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
