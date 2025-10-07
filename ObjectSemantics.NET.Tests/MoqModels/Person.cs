using System.Collections.Generic;

namespace ObjectSemantics.NET.Tests.MoqModels
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public List<Car> MyCars { get; set; } = new List<Car>();
        public List<Car> MyDreamCars { get; set; } = new List<Car>();
        public string[] MyFriends { get; set; }
    }
}
