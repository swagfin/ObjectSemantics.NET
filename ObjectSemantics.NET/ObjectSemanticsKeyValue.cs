namespace ObjectSemantics.NET
{
    public class ObjectSemanticsKeyValue
    {
        public ObjectSemanticsKeyValue() { }
        public ObjectSemanticsKeyValue(string key, object value)
        {
            Key = key;
            Value = value;
        }

        public string Key { get; set; }
        public object Value { get; set; }
    }
}
