using ObjectSemantics.NET.Tests.MoqModels;
using System.Collections.Generic;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class EnumerableLoopTests
    {

        [Fact]
        public void Should_Map_Enumerable_Collection_SingleLine()
        {
            //Create Model
            Person person = new Person
            {
                MyCars = new List<Car>
                {
                     new Car { Make = "BMW", Year = 2023 },
                     new Car { Make = "Rolls-Royce", Year = 2020 }
                }
            };
            //Template
            string template = @"{{ #foreach(MyCars)  }}I own a {{ Year }} {{ Make }}.{{ #endforeach }}";

            string result = person.Map(template);

            string expectedResult = $"I own a 2023 BMW.I own a 2020 Rolls-Royce.";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Should_Map_Enumerable_Collection_MultiLine()
        {
            //Create Model
            Person person = new Person
            {
                Name = "John Doe",
                MyCars = new List<Car>
                {
                     new Car { Make = "BMW", Year = 2023 },
                     new Car { Make = "Rolls-Royce", Year = 2020 }
                }
            };
            //Template
            string template = @"
{{ Name }}'s Cars
{{ #foreach(MyCars)  }}
 - {{ Year }} {{ Make }}
{{ #endforeach }}";

            string result = person.Map(template);

            string expectedResult = @"
John Doe's Cars

 - 2023 BMW

 - 2020 Rolls-Royce
";
            Assert.Equal(expectedResult, result);
        }


        [Fact]
        public void Should_Map_Multiple_Enumerable_Collections()
        {
            //Create Model
            Person person = new Person
            {
                MyCars = new List<Car>
                {
                     new Car { Make = "Honda" },
                     new Car { Make = "Toyota" }
                },
                MyDreamCars = new List<Car>
                {
                     new Car { Make = "BWM" },
                     new Car { Make = "Rolls-Royce" }
                }
            };
            //Template
            string template = @"
My Cars
{{ #foreach(MyCars)  }}
 - {{ Make }}
{{ #endforeach }}
My Dream Cars
{{ #foreach(MyDreamCars)  }}
 * {{ Make }}
{{ #endforeach }}
";

            string result = person.Map(template);

            string expectedResult = @"
My Cars

 - Honda

 - Toyota

My Dream Cars

 * BWM

 * Rolls-Royce

";
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void Should_Map_Array_Of_String()
        {
            //Create Model
            Person person = new Person
            {
                MyFriends = new string[] { "Morgan", "George", "Jane" }
            };
            //Template
            string template = @"{{ #foreach(MyFriends)  }} {{ . }} {{ #endforeach }}";

            string generatedTemplate = person.Map(template);
            string expectedResult = " Morgan  George  Jane ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }

        [Fact]
        public void Should_Map_Array_Of_String_With_Formatting()
        {
            //Create Model
            Person person = new Person
            {
                MyFriends = new string[] { "Morgan", "George", "Jane" }
            };
            //Template
            string template = @"{{ #foreach(MyFriends)  }} {{ .:uppercase }} {{ #endforeach }}";

            string generatedTemplate = person.Map(template);
            string expectedResult = " MORGAN  GEORGE  JANE ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }

        [Theory]
        [InlineData(@"{{ #foreach(MyFriends)  }}[{{ .:uppercase }}]{{ #endforeach }}")]
        [InlineData(@"{{# foreach(MyFriends)  }}[{{ .:uppercase }}]{{# endforeach }}")]
        [InlineData(@"{{ #foreach(MyFriends)  }}[{{ .:uppercase }}]{{  #endforeach }}")]
        public void Should_Evaluate_ForEach_Having_Spaces_Before_And_After_Parentheses(string template)
        {
            Person person = new Person
            {
                MyFriends = new string[] { "Morgan", "George" }
            };

            string result = person.Map(template);
            Assert.Equal("[MORGAN][GEORGE]", result);
        }
    }
}
