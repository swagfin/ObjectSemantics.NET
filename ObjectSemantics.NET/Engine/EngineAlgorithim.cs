using ObjectSemantics.NET.Engine.Models;
using System.Collections.Generic;

namespace ObjectSemantics.NET.Engine
{
    internal static class EngineAlgorithim
    {
        public static string GenerateFromTemplate<T>(T record, EngineRunnerTemplate template, Dictionary<string, object> parameterKeyValues = null, TemplateMapperOptions options = null) where T : new()
        {
            return EngineTemplateRenderer.Render(record, template, parameterKeyValues, options);
        }

        internal static EngineRunnerTemplate GenerateRunnerTemplate(string fileContent)
        {
            return EngineTemplateCache.GetOrAdd(fileContent, EngineTemplateParser.Parse);
        }
    }
}
