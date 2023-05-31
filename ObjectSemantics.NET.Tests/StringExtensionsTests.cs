using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("i am john doe", "john", "jane", "i am jane doe")]
        [InlineData("I am coding", "coding", "learning", "I am learning")]
        public void Should_ReplaceFirstOccurrence(string text, string search, string replace, string expected)
        {
            string results = text.ReplaceFirstOccurrence(search, replace);
            Assert.Equal(expected, results);
        }

        [Theory]
        [InlineData("i am john doe", "johnie", "lucy", "i am john doe")]
        [InlineData("I am coding", "swimming", "walking", "I am coding")]
        public void Should_Return_Passed_Text_If_ReplaceFirstOccurrence_Failed(string text, string search, string replace, string expected)
        {
            string results = text.ReplaceFirstOccurrence(search, replace);
            Assert.Equal(expected, results);
        }

    }
}
