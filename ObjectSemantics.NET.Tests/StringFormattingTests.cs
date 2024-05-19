using ObjectSemantics.NET.Tests.MoqModels;
using System;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class StringFormattingTests
    {
        [Fact]
        public void Should_Accept_String_To_UpperCase_or_LowerCase_Formatting()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "WILLiaM"
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Original StudentName: {{ StudentName }}, Uppercase StudentName: {{ StudentName:UPPERCASE }}, Lowercase StudentName: {{ StudentName:lowercase }}"
            };
            string generatedTemplate = template.Map(student);
            string expectedString = "Original StudentName: WILLiaM, Uppercase StudentName: WILLIAM, Lowercase StudentName: william";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Accept_Number_To_String_Formatting()
        {
            //Create Model
            Student student = new Student
            {
                Balance = 20000.5788
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Original Balance: {{ Balance }}, #,##0 Balance: {{ Balance:#,##0 }}, N5 Balance: {{ Balance:N5 }}"
            };
            string generatedTemplate = template.Map(student);
            string expectedString = "Original Balance: 20000.5788, #,##0 Balance: 20,001, N5 Balance: 20,000.57880";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }

        [Fact]
        public void Should_Accept_DateTime_To_String_Formatting()
        {
            //Lets see how it can handle multiple : {{ RegDate:yyyy-MM-dd HH:mm tt }}
            Student student = new Student
            {
                RegDate = new DateTime(2022, 11, 27, 18, 13, 59)
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Original RegDate: {{ RegDate }}, yyyy RegDate: {{ RegDate:yyyy }}, yyyy-MM-dd HH:mm tt RegDate: {{ RegDate:yyyy-MM-dd HH:mm  tt }}"
            };
            string generatedTemplate = template.Map(student);
            string expectedString = "Original RegDate: 11/27/2022 6:13:59 PM, yyyy RegDate: 2022, yyyy-MM-dd HH:mm tt RegDate: 2022-11-27 18:13 PM";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Accept_String_To_MD5_Formatting()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John DOE"
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Original String: {{ StudentName }} | To MD5 String: {{ StudentName:ToMD5 }}"
            };
            string generatedTemplate = template.Map(student);
            string expectedString = "Original String: John DOE | To MD5 String: 82AF64057A5F0D528CEE6F55D05823D7";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }


        [Fact]
        public void Should_Accept_String_To_BASE64_Formatting()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "John DOE"
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Original String: {{ StudentName }} | To BASE64 String: {{ StudentName:ToBase64 }}"
            };
            string generatedTemplate = template.Map(student);
            string expectedString = "Original String: John DOE | To BASE64 String: Sm9obiBET0U=";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }

        [Fact]
        public void Should_Accept_String_From_BASE64_Formatting()
        {
            //Create Model
            Student student = new Student
            {
                StudentName = "Sm9obiBET0U="
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"Original String: {{ StudentName }} | From BASE64 String: {{ StudentName:FromBase64 }}"
            };
            string generatedTemplate = template.Map(student);
            string expectedString = "Original String: Sm9obiBET0U= | From BASE64 String: John DOE";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }
    }
}
