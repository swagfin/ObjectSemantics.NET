using System;

namespace ObjectSemantics.NET
{
    public class ObjectSemanticsTemplate
    {
        public string Name { get; set; } = "default.template";
        public string FileContents { get; set; }
        public string Author { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}