using ObjectSemantics.NET.Tests.MoqModels;
using System.Collections.Generic;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class IfConditionTests
    {
        [Theory]
        [InlineData(1, "Valid")]
        [InlineData(0, "Invalid")]
        public void Should_Render_If_Block_When_Condition_Is_True(int id, string expected)
        {
            var model = new Invoice { Id = id };

            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"{{ #if(Id == 1) }}Valid{{ #else }}Invalid{{ #endif }}"
            };

            string result = template.Map(model);
            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(18, "Minor")]
        [InlineData(21, "Adult")]
        [InlineData(5, "Minor")]
        public void Should_Handle_LessThan_Or_Equal(int age, string expected)
        {
            var model = new Student { Age = age };

            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"{{ #if(Age <= 18) }}Minor{{ #else }}Adult{{ #endif }}"
            };

            var result = template.Map(model);
            Assert.Equal(expected, result);
        }


        [Theory]
        [InlineData(1, "1")]
        [InlineData(0, "Error")]
        [InlineData(-1, "Error")]
        [InlineData(5, "5")]
        [InlineData(+2, "2")]
        public void Should_Handle_Whitespace_And_Case_Insensitive_Condition(int id, string expected)
        {
            var model = new Invoice { Id = id };

            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"{{ #if( id > 0 ) }}{{id}}{{ #else }}Error{{ #endif }}"
            };

            var result = template.Map(model);
            Assert.Equal(expected, result);
        }


        [Fact]
        public void Should_Render_If_Block_Without_Else_When_True()
        {
            var model = new Student { IsActive = true };

            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Student: John {{ #if(IsActive == true) }}[Is Active]{{ #endif }}"
            };

            var result = template.Map(model);
            Assert.Equal("Student: John [Is Active]", result);
        }

        [Fact]
        public void Should_Evaluate_If_Enumerable_Count()
        {
            var model = new Student
            {
                Invoices = new List<Invoice>
                {
                     new Invoice{ Id = 2, RefNo = "INV_002" },
                     new Invoice{ Id = 1, RefNo = "INV_001" }
                }
            };

            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"{{ #if(Invoices == 2) }}Matched{{ #else }}Not Matched{{ #endif }}"
            };

            var result = template.Map(model);
            Assert.Equal("Matched", result);
        }

        [Fact]
        public void Should_Evaluate_Empty_Enumerable_As_Zero()
        {
            var model = new Student
            {
                Invoices = new List<Invoice>()
            };

            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"{{ #if(Invoices == 0) }}No invoices available{{ #else }}Invoices Found{{ #endif }}"
            };

            var result = template.Map(model);
            Assert.Equal("No invoices available", result);
        }

    }
}
