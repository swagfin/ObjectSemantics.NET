using ObjectSemantics.NET.Engine.Models;
using System;

namespace ObjectSemantics.NET.Engine
{
    internal static class EnginePropertyResolver
    {
        public static bool TryResolveProperty(RenderScope propMap, PropertyPath propertyPath, out ExtractedObjProperty result)
        {
            result = null;
            if (propMap == null || propertyPath == null || string.IsNullOrWhiteSpace(propertyPath.Text))
                return false;

            string path = propertyPath.Text;
            if (propMap.TryGetValue(path, out result))
                return true;

            string rootName = propertyPath.Root;
            if (propertyPath.Segments.Length == 0)
                return false;

            if (!propMap.TryGetValue(rootName, out ExtractedObjProperty rootProperty))
                return false;

            return TryResolveNestedProperty(rootProperty, propertyPath.Segments, out result);
        }

        private static bool TryResolveNestedProperty(ExtractedObjProperty rootProperty, string[] segments, out ExtractedObjProperty result)
        {
            result = null;
            if (rootProperty == null)
                return false;

            if (segments.Length == 0)
                return false;

            object currentValue = rootProperty.OriginalValue;
            Type currentType = rootProperty.Type;

            for (int i = 0; i < segments.Length; i++)
            {
                if (currentType == null)
                    return false;

                string segment = segments[i];
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
                        Type = currentType,
                        OriginalValue = nextValue
                    };
                    return true;
                }

                currentValue = nextValue;
            }

            return false;
        }
    }
}
