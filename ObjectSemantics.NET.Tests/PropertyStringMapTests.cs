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

        [Fact]
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

        [Theory]
        [InlineData("BMW", "BCB48DDDFF8C14B5F452EE573B4DB770")]
        [InlineData("Mercedes-Benz", "BE956E74642D5B18C6A4864AAD39F494")]
        [InlineData("Peugeot 308", "8C617F9B2E4DE166E277FEE781D32DAF")]
        public void Should_Format_String_To_MD5(string make, string expectedString)
        {
            Car car = new Car
            {
                Make = make
            };
            string generatedTemplate = car.Map("{{ Make:ToMD5 }}");
            Assert.Equal(expectedString, generatedTemplate);
        }

        [Theory]
        [InlineData("BMW", "Qk1X")]
        [InlineData("Mercedes-Benz", "TWVyY2VkZXMtQmVueg==")]
        [InlineData("Peugeot 308", "UGV1Z2VvdCAzMDg=")]
        public void Should_Format_String_To_Base64_Format(string make, string expectedString)
        {
            Car car = new Car
            {
                Make = make
            };
            string generatedTemplate = car.Map("{{ Make:ToBase64 }}");
            Assert.Equal(expectedString, generatedTemplate);
        }

        [Theory]
        [InlineData("Qk1X", "BMW")]
        [InlineData("TWVyY2VkZXMtQmVueg==", "Mercedes-Benz")]
        [InlineData("UGV1Z2VvdCAzMDg=", "Peugeot 308")]
        public void Should_Format_String_From_Base64_Format(string make, string expectedString)
        {
            Car car = new Car
            {
                Make = make
            };
            string generatedTemplate = car.Map("{{ Make:FromBase64 }}");
            Assert.Equal(expectedString, generatedTemplate);
        }

        [Theory]
        [InlineData("BMW")]
        [InlineData("Mercedes-Benz")]
        public void Should_Accept_Length_String_Format(string make)
        {
            Car car = new Car
            {
                Make = make
            };
            string generatedTemplate = car.Map("{{ Make:length }}");
            Assert.Equal(make.Length.ToString(), generatedTemplate);
        }
    }
}
