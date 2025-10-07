using ObjectSemantics.NET.Tests.MoqModels;
using System.Collections.Generic;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class ComplexityTests
    {

        [Fact]
        public void Should_Handle_ForEach_Loop_Inside_If_Block_Inline()
        {
            Person person = new Person
            {
                MyCars = new List<Car>
                {
                     new Car { Make = "BMW" },
                     new Car { Make = "Rolls-Royce" }
                }
            };

            string template = @"{{ #if(MyCars > 0) }}{{ #foreach(MyCars) }}[{{ Make }}]{{ #endforeach }}{{ #endif }}";

            string result = person.Map(template);

            string expected = "[BMW][Rolls-Royce]";
            Assert.Equal(expected, result);
        }


        [Fact]
        public void Should_Handle_ForEach_Loop_Inside_If_Block_Multiline()
        {
            Person person = new Person
            {
                MyCars = new List<Car>
                {
                     new Car { Make = "Toyota" },
                     new Car { Make = "BMW" }
                }
            };

            string template = @"
{{ #if(MyCars > 0) }}
  {{ #foreach(MyCars) }}
    - {{ Make:uppercase }}
   {{ #endforeach }}
{{ #endif }}";

            string result = person.Map(template);

            string expected = @"

  
    - TOYOTA
   
    - BMW
   
";
            Assert.Equal(expected, result);
        }
    }
}
