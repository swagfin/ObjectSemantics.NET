using ObjectSemantics.NET.Tests.MoqModels;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class PropertyStringMapTests
    {
        [Theory]
        [InlineData("BMW")]
        [InlineData("Toyota")]
        [InlineData("Mercedes-Benz")]
        [InlineData("Land Rover")]
        public void Should_Map_From_Simple_String(string make)
        {
            Car car = new Car
            {
                Make = make
            };
            string generatedTemplate = car.Map("My car make is {{ Make }}.");
            Assert.Equal($"My car make is {make}.", generatedTemplate);
        }

        [Theory]
        [InlineData("L(rover")]
        [InlineData("Toy >> tA")]
        [InlineData("Peugeot 308")]
        [InlineData("Fiat 500X")]
        public void Should_Map_From_SpecialChar_String(string make)
        {
            Car car = new Car
            {
                Make = make
            };
            string generatedTemplate = car.Map("My car make is {{ Make }}.");
            Assert.Equal($"My car make is {make}.", generatedTemplate);
        }

        [Fact]
        public void Should_Map_From_Null_String()
        {
            Car car = new Car
            {
                Make = null
            };
            string generatedTemplate = car.Map("My car make is {{ Make }}.");
            Assert.Equal($"My car make is {car.Make}.", generatedTemplate);
        }

        public void Should_Format_String_To_UpperCase()
        {
            Car car = new Car
            {
                Make = "Toyota"
            };
            string generatedTemplate = car.Map("{{ Make:uppercase }}");
            Assert.Equal(car.Make.ToUpper(), generatedTemplate);
        }

        [Fact]
        public void Should_Format_String_To_LowerCase()
        {
            Car car = new Car
            {
                Make = "ToYota"
            };
            string generatedTemplate = car.Map("{{ Make:lowercase }}");
            Assert.Equal(car.Make.ToLower(), generatedTemplate);
        }

        [Theory]
        [InlineData("toYota", "Toyota")]
        [InlineData("BMW", "Bmw")]
        [InlineData("Peugeot 308", "Peugeot 308")]
        public void Should_Format_String_To_TitleCase(string make, string expectedTitleCase)
        {
            Car car = new Car
            {
                Make = make
            };
            string generatedTemplate = car.Map("{{ Make:titlecase }}");
            Assert.Equal(expectedTitleCase, generatedTemplate);
        }
    }
}
