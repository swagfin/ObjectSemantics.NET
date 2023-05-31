using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class ExtractedObjPropertyTests
    {
        [Theory]
        [InlineData("==", "John")]
        [InlineData("==", "John Doe")]
        [InlineData("==", "John_Doe")]
        [InlineData("==", "John.Doe")]
        public void Should_Pass_ObjProperty_Equal_Condition_String(string criteria, string textString)
        {
            ExtractedObjProperty mockObj = new ExtractedObjProperty
            {
                Type = typeof(string),
                OriginalValue = textString
            };
            bool isPassed = mockObj.IsPropertyValueConditionPassed(textString, criteria);
            Assert.True(isPassed);
        }

        [Fact]
        public void Should_Pass_ObjProperty_NotEqual_Condition_String()
        {
            ExtractedObjProperty mockObj = new ExtractedObjProperty
            {
                Type = typeof(string),
                OriginalValue = "johnX"
            };
            bool isPassed = mockObj.IsPropertyValueConditionPassed("janeX", "!=");
            Assert.True(isPassed);
        }

        [Fact]
        public void Should_Test_Comparison_ObjProperty_Unsupported_Condition_String()
        {
            ExtractedObjProperty mockObj = new ExtractedObjProperty
            {
                Type = typeof(string),
                OriginalValue = "john"
            };
            bool isPassed = mockObj.IsPropertyValueConditionPassed("john", "===");
            Assert.True(isPassed);
        }



        [Theory]
        [InlineData("==", 200)]
        [InlineData("==", 200.5402)]
        public void Should_Pass_ObjProperty_Equal_Condition_Number(string criteria, double number)
        {
            ExtractedObjProperty mockObj = new ExtractedObjProperty
            {
                Type = typeof(double),
                OriginalValue = number
            };
            bool isPassed = mockObj.IsPropertyValueConditionPassed(number.ToString(), criteria);
            Assert.True(isPassed);
        }

        [Fact]
        public void Should_Pass_ObjProperty_NotEqual_Condition_Number()
        {
            ExtractedObjProperty mockObj = new ExtractedObjProperty
            {
                Type = typeof(double),
                OriginalValue = 200
            };
            bool isPassed = mockObj.IsPropertyValueConditionPassed("200.5", "!=");
            Assert.True(isPassed);
        }

        [Fact]
        public void Should_Pass_ObjProperty_GreaterOrEqual_Condition_Number()
        {
            ExtractedObjProperty mockObj = new ExtractedObjProperty
            {
                Type = typeof(double),
                OriginalValue = 200
            };
            bool isPassed = mockObj.IsPropertyValueConditionPassed("100.5", ">=");
            Assert.True(isPassed);
        }

        [Fact]
        public void Should_Pass_ObjProperty_Unsupported_Condition_Number()
        {
            ExtractedObjProperty mockObj = new ExtractedObjProperty
            {
                Type = typeof(double),
                OriginalValue = 200
            };
            bool isPassed = mockObj.IsPropertyValueConditionPassed("200", "====");
            Assert.False(isPassed);
        }
    }
}
