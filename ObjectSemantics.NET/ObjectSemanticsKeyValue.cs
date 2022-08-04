using System;

namespace ObjectSemantics.NET
{
    public class ObjectSemanticsKeyValue
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public Type Type { get { return Value.GetType(); } }
    }
}
