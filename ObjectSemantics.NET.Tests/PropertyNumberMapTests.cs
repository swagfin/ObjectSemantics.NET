using ObjectSemantics.NET.Tests.MoqModels;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class PropertyNumberMapTests
    {
        [Theory]
        [InlineData(2013)]
        [InlineData(2025000)]
        public void Should_Map_From_Int(int year)
        {
            Car car = new Car()
            {
                Year = year
            };
            string result = car.Map("Year: {{ Year }}");
            Assert.Equal($"Year: {year}", result);
        }

        [Theory]
        [InlineData(3.5)]
        [InlineData(1.3)]
        public void Should_Map_From_Double(double size)
        {
            Car car = new Car()
            {
                EngineSize = size
            };
            string result = car.Map("Engine size: {{ EngineSize }}");
            Assert.Equal($"Engine size: {size}", result);
        }

        [Theory]
        [InlineData(14.7f)]
        [InlineData(10.2f)]
        public void Should_Map_From_Float(float efficiency)
        {
            Car car = new Car()
            {
                FuelEfficiency = efficiency
            };
            string result = car.Map("Fuel efficiency: {{ FuelEfficiency }}");
            Assert.Equal($"Fuel efficiency: {efficiency}", result);
        }

        [Theory]
        [InlineData(1050000.75)]
        [InlineData(800.25)]
        public void Should_Map_From_Decimal(decimal price)
        {
            Car car = new Car()
            {
                Price = price
            };
            string result = car.Map("Price: {{ Price }}");
            Assert.Equal($"Price: {price}", result);
        }

        [Theory]
        [InlineData(20000)]
        [InlineData(50_000)]
        [InlineData(100000)]
        public void Should_Support_Number_To_String_Formatting(decimal price)
        {
            Car car = new Car
            {
                Price = price
            };
            string generatedTemplate = car.Map("{{ Price:#,##0 }}|{{ Price:N5 }}");
            Assert.Equal($"{price:#,##0}|{price:N5}", generatedTemplate);
        }
    }
}
