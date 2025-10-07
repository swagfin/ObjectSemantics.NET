using ObjectSemantics.NET.Tests.MoqModels;
using System;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class PropertyDateTimeMapTests
    {
        [Fact]
        public void Should_Map_From_Date()
        {
            DateTime date = new DateTime(2022, 11, 27, 18, 13, 59);
            Car car = new Car()
            {
                ManufactureDate = date
            };
            string result = car.Map("{{ ManufactureDate:yyyy }}|{{ ManufactureDate:yyyy-MM-dd HH:mm tt }}");
            Assert.Equal($"{car.ManufactureDate:yyyy}|{car.ManufactureDate:yyyy-MM-dd HH:mm tt}", result, false, true, true);
        }

        [Fact]
        public void Should_Map_From_Null_Date()
        {
            Car car = new Car()
            {
                LastServiceDate = null
            };
            string result = car.Map("Last serviced: {{ LastServiceDate }}");
            Assert.Equal($"Last serviced: {car.LastServiceDate}", result, false, true, true);
        }

        [Fact]
        public void Should_Map_From_Nullable_Date()
        {
            Car car = new Car()
            {
                LastServiceDate = new DateTime(2025, 1, 1)
            };
            string result = car.Map("Last serviced: {{ LastServiceDate }}");
            Assert.Equal($"Last serviced: {car.LastServiceDate}", result, false, true, true);
        }
    }
}
