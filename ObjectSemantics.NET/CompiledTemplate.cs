using ObjectSemantics.NET.Engine;
using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace ObjectSemantics.NET
{
    /// <summary>A reusable parsed template. Each render owns its model values and execution state.</summary>
    public class CompiledTemplate
    {
        private readonly EngineRunnerTemplate _template;

        internal CompiledTemplate(EngineRunnerTemplate template)
        {
            _template = template;
        }

        /// <summary>Returns a diagnostic snapshot; changing it does not modify the cached template.</summary>
        public IReadOnlyList<TemplateDiagnostic> Diagnostics
        {
            get
            {
                TemplateDiagnostic[] diagnostics = new TemplateDiagnostic[_template.Diagnostics.Length];
                for (int i = 0; i < diagnostics.Length; i++)
                {
                    TemplateDiagnostic source = _template.Diagnostics[i];
                    diagnostics[i] = new TemplateDiagnostic { Message = source.Message, Position = source.Position, Line = source.Line, Column = source.Column };
                }
                return Array.AsReadOnly(diagnostics);
            }
        }

        public string Render<T>(T model, Dictionary<string, object> parameters = null, TemplateMapperOptions options = null) where T : class
        {
            if (model == null) return string.Empty;
            return EngineTemplateRenderer.Render(model, _template, parameters, options);
        }

        /// <summary>Writes without buffering the entire output. Failures can leave partial output; the writer remains open.</summary>
        public void RenderTo<T>(TextWriter writer, T model, Dictionary<string, object> parameters = null, TemplateMapperOptions options = null) where T : class
        {
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            if (model == null) return;
            EngineTemplateRenderer.RenderTo(writer, model, typeof(T), _template, parameters, options);
        }
    }
}
