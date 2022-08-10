namespace ObjectSemantics.NET
{
    public class ObjectSemanticsOptions
    {
        /// <summary>
        /// Main Base Directory where template files are stored or Leave BLANK to use default path Environment/Templates/
        /// </summary>
        public string TemplatesDirectory { get; set; }
        /// <summary>
        /// Will create template Base Directory if it Doesn't exist
        /// </summary>
        public bool CreateTemplatesDirectoryIfNotExist { get; set; } = true;
        /// <summary>
        /// This if is the filtering for supported File Extension types, Note that this Library looks in subdirectories as well
        /// </summary>
        public string[] SupportedTemplateFileExtensions { get; set; } = new string[] { ".html", ".txt" };
    }
}
