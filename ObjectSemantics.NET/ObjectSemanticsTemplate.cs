using System;

namespace ObjectSemantics.NET
{
    public class ObjectSemanticsTemplate
    {
        public string Name { get; set; } = Guid.NewGuid().ToString();
        public string FileContents { get; set; }
    }
}