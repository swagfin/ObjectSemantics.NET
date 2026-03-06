using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace ObjectSemantics.NET.Engine
{
    internal static class EnginePropertyResolver
    {
        private static readonly ConcurrentDictionary<string, string[]> PropertyPathSegmentsCache = new ConcurrentDictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        public static bool TryResolveProperty(Dictionary<string, ExtractedObjProperty> propMap, string propertyPath, out ExtractedObjProperty result)
        {
            result = null;
            if (propMap == null || string.IsNullOrWhiteSpace(propertyPath))
                return false;

            string path = propertyPath.Trim();
            if (propMap.TryGetValue(path, out result))
                return true;

            int dotIndex = path.IndexOf('.');
            if (dotIndex < 0)
                return false;

            string rootName = path.Substring(0, dotIndex).Trim();
            string nestedPath = path.Substring(dotIndex + 1).Trim();
            if (string.IsNullOrEmpty(rootName) || string.IsNullOrEmpty(nestedPath))
                return false;

            if (!propMap.TryGetValue(rootName, out ExtractedObjProperty rootProperty))
                return false;

            return TryResolveNestedProperty(rootProperty, nestedPath, path, out result);
        }

        private static bool TryResolveNestedProperty(ExtractedObjProperty rootProperty, string nestedPath, string fullPath, out ExtractedObjProperty result)
        {
            result = null;
            if (rootProperty == null || string.IsNullOrWhiteSpace(nestedPath))
                return false;

            string[] segments = GetPathSegments(nestedPath);
            if (segments.Length == 0)
                return false;

            object currentValue = rootProperty.OriginalValue;
            Type currentType = rootProperty.Type;

            for (int i = 0; i < segments.Length; i++)
            {
                if (currentType == null)
                    return false;

                string segment = segments[i].Trim();
                if (string.IsNullOrEmpty(segment))
                    return false;

                if (!EngineTypeMetadataCache.TryGetPropertyAccessor(currentType, segment, out PropertyAccessor nextAccessor))
                    return false;

                object nextValue = currentValue == null ? null : nextAccessor.Getter(currentValue);
                currentType = nextAccessor.PropertyType;

                if (i == segments.Length - 1)
                {
                    result = new ExtractedObjProperty
                    {
                        Name = fullPath,
                        Type = currentType,
                        OriginalValue = nextValue
                    };
                    return true;
                }

                currentValue = nextValue;
            }

            return false;
        }

        private static string[] GetPathSegments(string nestedPath)
        {
            return PropertyPathSegmentsCache.GetOrAdd(nestedPath, SplitPathSegments);
        }

        private static string[] SplitPathSegments(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return Array.Empty<string>();

            string[] rawSegments = path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (rawSegments.Length == 0)
                return rawSegments;

            int validCount = 0;
            for (int i = 0; i < rawSegments.Length; i++)
            {
                string trimmed = rawSegments[i].Trim();
                if (trimmed.Length == 0)
                    continue;

                rawSegments[validCount] = trimmed;
                validCount++;
            }

            if (validCount == rawSegments.Length)
                return rawSegments;

            string[] segments = new string[validCount];
            Array.Copy(rawSegments, segments, validCount);
            return segments;
        }
    }
}
