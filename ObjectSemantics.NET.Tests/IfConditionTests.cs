using ObjectSemantics.NET.Tests.MoqModels;
using System.Collections.Generic;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class IfConditionTests
    {
        [Theory]
        [InlineData(40, "Adult")]
        [InlineData(18, "Adult")]
        [InlineData(0, "Minor")]
        [InlineData(-7, "Minor")]
        public void Should_Render_If_GreaterOrEqual_Condition_Block(int age, string expected)
        {
            Person person = new Person
            {
                Age = age
            };

            string template = @"{{ #if(Age >= 18) }}Adult{{ #else }}Minor{{ #endif }}";

            string result = person.Map(template);
            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(40, "Adult 40")]
        [InlineData(18, "Adult 18")]
        [InlineData(0, "Minor 0")]
        [InlineData(-7, "Minor -7")]
        public void Should_Resolve_Variables_Inside_If_Condition(int age, string expected)
        {
            Person person = new Person
            {
                Age = age
            };

            string template = @"{{ #if( Age > 17 ) }}Adult {{Age}}{{ #else }}Minor {{Age}}{{ #endif }}";

            var result = person.Map(template);
            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(40, "")]
        [InlineData(18, "")]
        [InlineData(0, "NoDrinkingBeer")]
        [InlineData(-7, "NoDrinkingBeer")]
        public void Should_Render_If_Block_Without_Else_When_True(int age, string expected)
        {
            Person person = new Person
            {
                Age = age
            };

            string template = @"{{ #if(Age < 18) }}NoDrinkingBeer{{ #endif }}";

            string result = person.Map(template);
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Should_Evaluate_Enumerable_Count_Inside_If_Block()
        {

            Person person = new Person
            {
                MyCars = new List<Car>
                {
                     new Car { Make = "BMW" },
                     new Car { Make = "Rolls-Royce" }
                }
            };

            string template = @"{{ #if(MyCars == 2) }}Has 2 Cars{{ #endif }}";

            string result = person.Map(template);
            Assert.Equal("Has 2 Cars", result);
        }

        [Fact]
        public void Should_Evaluate_Empty_Enumerable_As_Zero()
        {
            Person person = new Person
            {
                MyCars = null
            };

            string template = @"{{ #if(MyCars == 0) }}Zero Cars{{ #endif }}";

            string result = person.Map(template);
            Assert.Equal("Zero Cars", result);
        }

        [Theory]
        [InlineData("John DoeX2", "Yes")]
        [InlineData("John Doe", "No")]
        [InlineData("Doe", "No")]
        [InlineData("John ", "No")]
        public void Should_Evaluate_String_Conditions_If_Blocks(string name, string expected)
        {
            Person person = new Person
            {
                Name = name
            };

            string template = @"{{ #if(Name == John DoeX2) }}Yes{{ #else }}No{{ #endif }}";

            string result = person.Map(template);
            Assert.Equal(expected, result);
        }
    }
}
