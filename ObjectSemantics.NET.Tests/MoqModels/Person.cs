using System.Collections.Generic;

namespace ObjectSemantics.NET.Tests.MoqModels
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public List<Car> MyCars { get; set; } = new List<Car>();
    }
}
