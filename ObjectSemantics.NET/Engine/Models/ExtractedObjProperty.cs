using System;
using System.Collections;
using System.Globalization;

namespace ObjectSemantics.NET.Engine.Models
{
    internal class ExtractedObjProperty
    {
        public Type Type { get; set; }
        public string Name { get; set; }
        public object OriginalValue { get; set; }
        public string StringFormatted => Convert.ToString(OriginalValue, CultureInfo.InvariantCulture);

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