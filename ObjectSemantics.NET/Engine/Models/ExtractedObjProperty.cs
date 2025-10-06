using System;
using System.Collections;

namespace ObjectSemantics.NET.Engine.Models
{
    public class ExtractedObjProperty
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public object OriginalValue { get; set; }
        public string StringFormatted { get { return string.Format("{0}", OriginalValue); } }
        public bool IsEnumerableObject
        {
            get { return typeof(IEnumerable).IsAssignableFrom(Type) && Type != typeof(string); }
        }
        public bool IsClassObject
        {
            get { return Type.IsClass && Type != typeof(string); }
        }
    }
}