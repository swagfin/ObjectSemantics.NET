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
        private ConcurrentDictionary<string, List<string>> TemplateLines { get; set; } = new ConcurrentDictionary<string, List<string>>();
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

        private List<string> GetTemplateContents(string templateName)
        {
            if (this.TemplateLines.TryGetValue(templateName, out List<string> _lines))
                return _lines;
            //Look in Directory 
            string altenativePath = Path.Combine(this.Options.TemplatesDirectory, templateName);
            if (!File.Exists(altenativePath))
                throw new Exception($"Template doesn't seem to exist in directory: {altenativePath}");
            string[] allLines = File.ReadAllLines(altenativePath);
            if (allLines.Length == 0)
                return new List<string>();
            //Try Upsert in Memory
            if (this.Options.ReserveTemplatesInMemory)
                this.TemplateLines.TryAdd(templateName, allLines.ToList());
            return allLines.ToList();
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
                            string[] fileLines = File.ReadAllLines(file.FullName);
                            TemplateLines.TryAdd(file.Name, (fileLines.Length > 0) ? fileLines.ToList() : new List<string>());
                        }
                        catch (Exception ex)
                        {
                            TemplateLines.TryAdd(file.Name, new List<string> { ex.Message });
                        }
            }
            catch { }
        }

        public string GenerateTemplate<T>(T record, string templateName) where T : new()
        {
            if (record == null)
                return string.Empty;
            List<string> templateLines = GetTemplateContents(templateName);
            return record.GeneratFromObj(templateLines);
        }

        public string GenerateTemplate<T>(List<T> records, string templateName) where T : new()
        {
            if (records == null || records.Count == 0)
                return string.Empty;
            List<string> templateLines = GetTemplateContents(templateName);
            return records.GeneratFromObjCollection(templateLines);
        }

    }
}
