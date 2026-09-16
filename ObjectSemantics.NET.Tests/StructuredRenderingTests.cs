using ObjectSemantics.NET.Engine;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class StructuredRenderingTests
    {
        [Fact]
        public void KeepsLiteralAndModelMarkersUnchanged()
        {
            Model model = new Model { Name = "RP_2" };
            Assert.Equal("RP_1|RP_2|Bob", model.Map("RP_1|{{ Name }}|{{ Other }}"));
            Assert.Equal("{{ Other }}|Bob", new Model { Name = "{{ Other }}" }.Map("{{ Name }}|{{ Other }}"));
        }

        [Fact]
        public void SupportsNestedConditionsAndRowConditions()
        {
            Model model = new Model();
            Assert.Equal("ABC", model.Map("{{ #if(Active == true) }}A{{ #if(Active == true) }}B{{ #endif }}C{{ #endif }}"));
            Assert.Equal("YN", model.Map("{{ #foreach(Items) }}{{ #if(Active == true) }}Y{{ #else }}N{{ #endif }}{{ #endforeach }}"));
        }

        [Fact]
        public void SupportsNestedLoopsAndScalarRows()
        {
            Model model = new Model();
            Assert.Equal("[1][2][3]", model.Map("{{ #foreach(Items) }}{{ #foreach(Numbers) }}[{{ . }}]{{ #endforeach }}{{ #endforeach }}"));
        }

        [Theory]
        [InlineData("{{ #if(Active == true) }}", "Unclosed block")]
        [InlineData("\n{{ #endif }}", "Unexpected block")]
        [InlineData("{{ Name", "Unclosed template")]
        [InlineData("{{ __calc(1 + ()) }}", "Invalid arithmetic")]
        [InlineData("{{ __calc(1 2 +) }}", "Invalid arithmetic")]
        [InlineData("{{ __calc(2(3)) }}", "Invalid arithmetic")]
        public void ReportsMalformedTemplates(string template, string message)
        {
            Assert.Contains(EngineTemplateParser.Parse(template).Diagnostics, d => d.Message.StartsWith(message, StringComparison.Ordinal));
            Assert.Throws<FormatException>(() => new Model().Map(template, options: new TemplateMapperOptions { StrictMode = true }));
        }

        [Fact]
        public void EnforcesOutputIterationAndNestingLimits()
        {
            Model model = new Model();
            Assert.Throws<TemplateLimitExceededException>(() => model.Map("1234", options: new TemplateMapperOptions { MaximumOutputCharacters = 3 }));
            Assert.Throws<TemplateLimitExceededException>(() => model.Map("{{ #foreach(Items) }}x{{ #endforeach }}", options: new TemplateMapperOptions { MaximumIterations = 1 }));
            Assert.Throws<TemplateLimitExceededException>(() => model.Map("{{ #foreach(Items) }}{{ #foreach(Numbers) }}x{{ #endforeach }}{{ #endforeach }}", options: new TemplateMapperOptions { MaximumNestingDepth = 1 }));
            Assert.Throws<OperationCanceledException>(() => model.Map("plain", options: new TemplateMapperOptions { CancellationToken = new CancellationToken(true) }));
            Assert.Throws<FormatException>(() => model.Map("{{ Unknown }}", options: new TemplateMapperOptions { StrictMode = true }));
        }

        public class Model
        {
            public string Name { get; set; } = "Alice";
            public string Other { get; set; } = "Bob";
            public bool Active { get; set; } = true;
            public List<Item> Items { get; set; } = new List<Item>
            {
                new Item { Active = true, Numbers = new[] { 1, 2 } },
                new Item { Active = false, Numbers = new[] { 3 } }
            };
        }

        public class Item
        {
            public bool Active { get; set; }
            public int[] Numbers { get; set; }
        }
    }
}
