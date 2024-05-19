using ObjectSemantics.NET.Tests.MoqModels;
using System;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class PropertyNullableTests
    {
        [Fact]
        public void Should_Map_Nullable_DateTime_Property_Given_NULL()
        {
            //Create Model
            StudentClockInDetail clockInDetails = new StudentClockInDetail { LastClockedInDate = null };
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Last Clocked In: {{ LastClockedInDate:yyyy-MM-dd }}"
            };
            string generatedTemplate = TemplateMapper.Map(clockInDetails, template);
            string expectedString = "Last Clocked In: ";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }

        [Fact]
        public void Should_Map_Nullable_DateTime_Property_Given_A_Value()
        {
            //Create Model
            StudentClockInDetail clockInDetails = new StudentClockInDetail { LastClockedInDate = DateTime.Now };
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Last Clocked In: {{ LastClockedInDate:yyyy-MM-dd }}"
            };
            string generatedTemplate = TemplateMapper.Map(clockInDetails, template);
            string expectedString = $"Last Clocked In: {DateTime.Now:yyyy-MM-dd}";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }

        [Fact]
        public void Should_Map_Nullable_Number_Property_Given_NULL()
        {
            //Create Model
            StudentClockInDetail clockInDetails = new StudentClockInDetail { LastClockedInPoints = null };
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Last Clocked In Points: {{ LastClockedInPoints:N2 }}"
            };
            string generatedTemplate = TemplateMapper.Map(clockInDetails, template);
            string expectedString = "Last Clocked In Points: ";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(2500)]
        [InlineData(200)]
        public void Should_Map_Nullable_Number_Property_Given_A_Value(long? number)
        {
            //Create Model
            StudentClockInDetail clockInDetails = new StudentClockInDetail { LastClockedInPoints = number };
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Last Clocked In Points: {{ LastClockedInPoints:N2 }}"
            };
            string generatedTemplate = TemplateMapper.Map(clockInDetails, template);
            string expectedString = $"Last Clocked In Points: {number:N2}";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }
    }
}
