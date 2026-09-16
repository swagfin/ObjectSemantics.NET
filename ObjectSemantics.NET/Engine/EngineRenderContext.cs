using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace ObjectSemantics.NET.Engine
{
    internal class EngineRenderContext
    {
        private readonly TextWriter _writer;
        private long _outputLength;
        private long _iterations;

        public EngineRenderContext(TextWriter writer, TemplateMapperOptions options)
        {
            _writer = writer;
            Options = options;
        }

        public TemplateMapperOptions Options { get; set; }

        public void Check(int depth)
        {
            Options.CancellationToken.ThrowIfCancellationRequested();
            if (Options.MaximumNestingDepth > 0 && depth > Options.MaximumNestingDepth)
                throw new TemplateLimitExceededException("Template nesting limit exceeded.");
        }

        public void CountIteration()
        {
            Options.CancellationToken.ThrowIfCancellationRequested();
            if (Options.MaximumIterations > 0 && ++_iterations > Options.MaximumIterations)
                throw new TemplateLimitExceededException("Template iteration limit exceeded.");
        }

        public void Write(string value)
        {
            Options.CancellationToken.ThrowIfCancellationRequested();
            long length = value == null ? 0 : value.Length;
            if (Options.MaximumOutputCharacters > 0 && length > Options.MaximumOutputCharacters - _outputLength)
                throw new TemplateLimitExceededException("Template output limit exceeded.");
            _outputLength += length;
            _writer.Write(value);
        }
    }

    internal class RenderScope
    {
        private Dictionary<string, ExtractedObjProperty> _properties;

        public RenderScope(object model, Type type, Dictionary<string, object> parameters, bool isRow, TemplateMapperOptions options)
        {
            Model = model;
            Type = type;
            Parameters = parameters;
            IsRow = isRow;
            Options = options;
            if (options.LazyPropertyAccess)
            {
                _properties = new Dictionary<string, ExtractedObjProperty>(StringComparer.OrdinalIgnoreCase);
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> parameter in parameters)
                    {
                        if (EngineTypeMetadataCache.TryGetPropertyAccessor(type, parameter.Key, out _))
                            throw new ArgumentException("Additional parameter duplicates a model property: " + parameter.Key);
                        _properties.Add(parameter.Key, new ExtractedObjProperty { Type = parameter.Value == null ? typeof(object) : parameter.Value.GetType(), OriginalValue = parameter.Value });
                    }
                }
            }
            else if (!isRow)
                _properties = EngineTypeMetadataCache.BuildPropertyMap(model, type, parameters);
        }

        public object Model { get; set; }
        public Type Type { get; set; }
        public Dictionary<string, object> Parameters { get; set; }
        public bool IsRow { get; set; }
        public TemplateMapperOptions Options { get; set; }
        public EngineRenderContext Context { get; set; }

        public bool TryGetValue(string name, out ExtractedObjProperty property)
        {
            if (_properties == null)
                _properties = EngineTypeMetadataCache.BuildPropertyMap(Model, Type, Parameters);
            if (_properties.TryGetValue(name, out property))
                return true;
            if (!Options.LazyPropertyAccess || !EngineTypeMetadataCache.TryGetPropertyAccessor(Type, name, out PropertyAccessor accessor))
                return false;
            property = new ExtractedObjProperty { Type = accessor.PropertyType, OriginalValue = Model == null ? null : accessor.Getter(Model) };
            _properties.Add(name, property);
            return true;
        }
    }
}
