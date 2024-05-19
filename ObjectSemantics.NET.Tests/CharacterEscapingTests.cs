using ObjectSemantics.NET.Tests.MoqModels;
using System.Collections.Generic;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class CharacterEscapingTests
    {

        [Fact]
        public void Should_Escape_Xml_Char_If_Option_Is_Enabled()
        {
            //Create Model
            Student student = new Student { StudentName = "I've got \"special\" < & also >" };
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"{{ StudentName }}"
            };
            string generatedTemplate = template.Map(student, null, new TemplateMapperOptions
            {
                XmlCharEscaping = true
            });
            string expectedString = "I&apos;ve got &quot;special&quot; &lt; &amp; also &gt;";
            Assert.Equal(expectedString, generatedTemplate, false, true, true);
        }

        [Fact]
        public void Should_Escape_Xml_Char_In_LOOPS_If_Option_Is_Enabled()
        {
            //Create Model
            Student student = new Student
            {
                Invoices = new List<Invoice>
                {
                     new Invoice{  Narration="I've got \"special\""},
                     new Invoice{  Narration="I've got  < & also >"}
                }
            };
            //Template
            var template = new ObjectSemanticsTemplate
            {
                FileContents = @"{{ #foreach(invoices)  }} [{{ Narration }}] {{ #endforeach }}"
            };
            string generatedTemplate = template.Map(student, null, new TemplateMapperOptions
            {
                XmlCharEscaping = true
            });
            string expectedResult = @" [I&apos;ve got &quot;special&quot;]  [I&apos;ve got &lt; &amp; also &gt;] ";
            Assert.Equal(expectedResult, generatedTemplate, false, true, true);
        }
    }
}
