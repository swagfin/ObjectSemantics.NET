using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections.Concurrent;

namespace ObjectSemantics.NET.Engine
{
    internal static class EngineTemplateCache
    {
        private const int MaxEntries = 2048;

        private static readonly ConcurrentDictionary<string, EngineRunnerTemplate> Cache = new ConcurrentDictionary<string, EngineRunnerTemplate>(StringComparer.Ordinal);

        public static EngineRunnerTemplate GetOrAdd(string templateContent, Func<string, EngineRunnerTemplate> factory)
        {
            string key = templateContent ?? string.Empty;
            if (Cache.TryGetValue(key, out EngineRunnerTemplate cachedTemplate))
                return cachedTemplate;

            EngineRunnerTemplate createdTemplate = factory == null ? new EngineRunnerTemplate { Template = key } : factory(key);

            if (Cache.Count >= MaxEntries)
                Cache.Clear();

            Cache.TryAdd(key, createdTemplate);
            return createdTemplate;
        }
    }
}
