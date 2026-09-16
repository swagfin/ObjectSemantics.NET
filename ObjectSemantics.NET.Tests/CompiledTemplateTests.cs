using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace ObjectSemantics.NET.Tests
{
    public class CompiledTemplateTests
    {
        [Fact]
        public void CompiledTemplateSupportsConcurrentIndependentRenders()
        {
            CompiledTemplate template = TemplateMapper.Compile("{{ Name }}={{ __calc(Value * 2) }}");
            Parallel.For(0, 250, i =>
            {
                Model model = new Model(i.ToString(), i);
                Assert.Equal(i + "=" + (i * 2), template.Render(model));
            });
        }

        [Fact]
        public void WriterOutputMatchesStringAndWriterRemainsOpen()
        {
            CompiledTemplate template = TemplateMapper.Compile("{{ Name }}|{{ #foreach(Items) }}[{{ . }}]{{ #endforeach }}");
            Model model = new Model("<Customer>", 3);
            TemplateMapperOptions options = new TemplateMapperOptions { XmlCharEscaping = true };
            using (StringWriter writer = new StringWriter(CultureInfo.InvariantCulture))
            {
                template.RenderTo(writer, model, options: options);
                Assert.Equal(template.Render(model, options: options), writer.ToString());
                writer.Write("!");
                Assert.EndsWith("!", writer.ToString());
            }
        }

        [Fact]
        public void WriterLimitLeavesOnlyOutputWithinBudget()
        {
            CompiledTemplate template = TemplateMapper.Compile("A{{ Name }}");
            using (StringWriter writer = new StringWriter())
            {
                Assert.Throws<TemplateLimitExceededException>(() => template.RenderTo(writer, new Model("Long", 1), options: new TemplateMapperOptions { MaximumOutputCharacters = 2 }));
                Assert.Equal("A", writer.ToString());
            }
        }

        [Fact]
        public void DiagnosticsHaveLocationsAndCannotMutateCachedTemplate()
        {
            CompiledTemplate template = TemplateMapper.Compile("First\n  {{ #endif }}");
            IReadOnlyList<TemplateDiagnostic> diagnostics = template.Diagnostics;
            Assert.Single(diagnostics);
            Assert.Equal(2, diagnostics[0].Line);
            Assert.Equal(3, diagnostics[0].Column);
            diagnostics[0].Message = "changed";
            Assert.StartsWith("Unexpected", template.Diagnostics[0].Message);
            Assert.Throws<FormatException>(() => TemplateMapper.Compile("{{ #endif }}", new TemplateMapperOptions { StrictMode = true }));
        }

        [Fact]
        public void LimitsApplyToCachedAndCompiledTemplates()
        {
            CompiledTemplate template = TemplateMapper.Compile("Hello {{ Name }}");
            TemplateMapperOptions options = new TemplateMapperOptions { MaximumTemplateCharacters = 3 };
            Assert.Throws<TemplateLimitExceededException>(() => TemplateMapper.Compile("Hello {{ Name }}", options));
            Assert.Throws<TemplateLimitExceededException>(() => template.Render(new Model("Test", 1), options: options));
            Assert.Throws<ArgumentOutOfRangeException>(() => template.Render(new Model("Test", 1), options: new TemplateMapperOptions { MaximumIterations = -1 }));
        }

        [Fact]
        public void OverflowFollowsExpressionFailurePolicy()
        {
            CompiledTemplate template = TemplateMapper.Compile("A{{ __calc(79228162514264337593543950335 + 1) }}B");
            Assert.Equal("AB", template.Render(new Model("Test", 1)));
            Assert.Throws<FormatException>(() => template.Render(new Model("Test", 1), options: new TemplateMapperOptions { StrictMode = true }));
        }

        public class Model
        {
            public Model(string name, int value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; set; }
            public int Value { get; set; }
            public int[] Items { get; set; } = new[] { 1, 2, 3 };
        }
    }
}
