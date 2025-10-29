using ObjectSemantics.NET.Tests.MoqModels;
using System;
using System.Collections.Generic;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class CognitiveMapTests
    {
        [Theory]
        [InlineData("John Doe")]
        [InlineData("Jane Doe")]
        public void Library_Entry_Point_T_Extension_Should_Work(string personName)
        {
            Person person = new Person
            {
                Name = personName
            };
            string generatedTemplate = person.Map("I am {{ Name }}!");
            Assert.Equal($"I am {personName}!", generatedTemplate);
        }

        [Theory]
        [InlineData("John Doe")]
        [InlineData("Jane Doe")]
        public void Library_Entry_Point_String_Extension_Should_Work(string personName)
        {
            Person person = new Person
            {
                Name = personName
            };
            string generatedTemplate = "I am {{ Name }}!".Map(person);
            Assert.Equal($"I am {personName}!", generatedTemplate);
        }

        [Fact]
        public void Additional_Headers_Should_Also_Be_Mapped()
        {
            Person person = new Person
            {
                Name = "John Doe"
            };

            //additional params (outside the class)
            Dictionary<string, object> additionalParams = new Dictionary<string, object>
            {
                { "Occupation", "Developer"},
                { "DateOfBirth", new DateTime(1995, 01, 01) }
            };

            string generatedTemplate = "Name: {{ Name }} | Occupation: {{ Occupation }} | DOB: {{ DateOfBirth }}".Map(person, additionalParams);

            Assert.Equal($"Name: {person.Name} | Occupation: {additionalParams["Occupation"]} | DOB: {additionalParams["DateOfBirth"]}", generatedTemplate);
        }


        [Theory]
        [InlineData("I am {{       Name }}")]
        [InlineData("I am {{ Name       }}")]
        [InlineData("I am {{              Name       }}")]
        [InlineData("I am {{Name}}")]
        [InlineData("I am {{ Name }}")]
        public void Whitespaces_Inside_Curl_Braces_Should_Be_Ignored(string template)
        {
            Person person = new Person
            {
                Name = "John Doe"
            };
            string generatedTemplate = person.Map(template);
            Assert.Equal($"I am John Doe", generatedTemplate);
        }

        [Theory]
        [InlineData("{{ MissingPropX }}", "{{ MissingPropX }}")]
        [InlineData("{{ MissingProp123 }}", "{{ MissingProp123 }}")]
        [InlineData("{{  UnknownPropertyXyz     }}", "{{ UnknownPropertyXyz }}")] //it trims the excess whitespaces
        [InlineData("{{   Unknown3x}}", "{{ Unknown3x }}")] //it trims the excess whitespaces
        public void None_Existing_Properties_Should_Be_Returned_If_Not_Mapped(string template, string expected)
        {
            Person person = new Person
            {
                Name = "John Doe"
            };
            string generatedTemplate = person.Map(template);
            Assert.Equal(expected, generatedTemplate);
        }


        [Theory]
        [InlineData("I've got \"special\" < & also >", "I&apos;ve got &quot;special&quot; &lt; &amp; also &gt;")]
        [InlineData("I've got < & also >", "I&apos;ve got &lt; &amp; also &gt;")]
        public void Should_Escape_Xml_Char_Values_If_Option_Is_Enabled(string value, string expected)
        {
            Person person = new Person()
            {
                Name = value
            };
            string generatedTemplate = person.Map("{{ Name }}", null, new TemplateMapperOptions
            {
                XmlCharEscaping = true
            });
            Assert.Equal(expected, generatedTemplate);
        }


        [Fact]
        public void Additional_Headers_And_Class_Properties_Should_Also_Be_Mapped_Combined()
        {
            Payment payment = new Payment
            {
                Id = 1,
                Amount = 1000,
                PayMethod = "CHEQUE",
                PayMethodId = 2,
                ReferenceNo = "CHEQUE0001",
                UserId = 242
            };
            //additional params (outside the class)
            Dictionary<string, object> additionalParams = new Dictionary<string, object>
            {
                { "ReceivedBy", "John Doe"},
                { "NewBalance", 1050 }
            };

            string generatedTemplate = payment.Map("{{Id}}-{{ ReferenceNo }} Confirmed. ${{ Amount:N2 }} received via {{ PayMethod }}({{PayMethodId}}) from user-id {{ UserId }}. New Balance: {{ NewBalance:N2 }}, Received By: {{ReceivedBy}}.", additionalParams);


            string expectedResponse = "1-CHEQUE0001 Confirmed. $1,000.00 received via CHEQUE(2) from user-id 242. New Balance: 1,050.00, Received By: John Doe.";

            Assert.Equal(generatedTemplate, expectedResponse);
        }
    }
}
