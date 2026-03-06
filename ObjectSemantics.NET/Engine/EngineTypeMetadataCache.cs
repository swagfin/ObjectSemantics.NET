using ObjectSemantics.NET.Engine.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ObjectSemantics.NET.Engine
{
    internal static class EngineTypeMetadataCache
    {
        private static readonly ConcurrentDictionary<Type, TypePropertyCache> TypeCacheMap = new ConcurrentDictionary<Type, TypePropertyCache>();

        private static readonly TypePropertyCache EmptyTypePropertyCache = new TypePropertyCache(Array.Empty<PropertyAccessor>(), new Dictionary<string, PropertyAccessor>(StringComparer.OrdinalIgnoreCase));

        public static Dictionary<string, ExtractedObjProperty> BuildPropertyMap(object value, Type declaredType, Dictionary<string, object> parameters)
        {
            TypePropertyCache typeCache = GetTypePropertyCache(declaredType);
            int capacity = typeCache.Accessors.Length + (parameters != null ? parameters.Count : 0);
            Dictionary<string, ExtractedObjProperty> propertyMap = new Dictionary<string, ExtractedObjProperty>(capacity, StringComparer.OrdinalIgnoreCase);

            PropertyAccessor[] accessors = typeCache.Accessors;
            for (int i = 0; i < accessors.Length; i++)
            {
                PropertyAccessor accessor = accessors[i];
                propertyMap.Add(accessor.Name, new ExtractedObjProperty
                {
                    Type = accessor.PropertyType,
                    Name = accessor.Name,
                    OriginalValue = value == null ? null : accessor.Getter(value)
                });
            }

            if (parameters != null)
            {
                foreach (KeyValuePair<string, object> p in parameters)
                {
                    propertyMap.Add(p.Key, new ExtractedObjProperty
                    {
                        Type = p.Value != null ? p.Value.GetType() : typeof(object),
                        Name = p.Key,
                        OriginalValue = p.Value
                    });
                }
            }

            return propertyMap;
        }

        public static bool TryGetPropertyAccessor(Type type, string propertyName, out PropertyAccessor accessor)
        {
            accessor = null;
            if (type == null || string.IsNullOrWhiteSpace(propertyName))
                return false;

            TypePropertyCache typeCache = GetTypePropertyCache(type);
            return typeCache.PropertyMap.TryGetValue(propertyName, out accessor);
        }

        private static TypePropertyCache GetTypePropertyCache(Type type)
        {
            if (type == null)
                return EmptyTypePropertyCache;

            return TypeCacheMap.GetOrAdd(type, BuildTypePropertyCache);
        }

        private static TypePropertyCache BuildTypePropertyCache(Type type)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            List<PropertyAccessor> accessors = new List<PropertyAccessor>(properties.Length);
            Dictionary<string, PropertyAccessor> propertyMap = new Dictionary<string, PropertyAccessor>(properties.Length, StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (property.GetIndexParameters().Length != 0)
                    continue;

                PropertyAccessor accessor = new PropertyAccessor(property.Name, property.PropertyType, CreatePropertyGetter(type, property));
                accessors.Add(accessor);
                propertyMap.Add(accessor.Name, accessor);
            }

            return new TypePropertyCache(accessors.ToArray(), propertyMap);
        }

        private static Func<object, object> CreatePropertyGetter(Type declaringType, PropertyInfo propertyInfo)
        {
            if (propertyInfo == null || propertyInfo.GetMethod == null)
                return _ => null;

            try
            {
                ParameterExpression instanceParam = Expression.Parameter(typeof(object), "instance");
                UnaryExpression typedInstance = Expression.Convert(instanceParam, declaringType);
                MemberExpression propertyAccess = Expression.Property(typedInstance, propertyInfo);
                UnaryExpression boxResult = Expression.Convert(propertyAccess, typeof(object));
                return Expression.Lambda<Func<object, object>>(boxResult, instanceParam).Compile();
            }
            catch
            {
                return obj => propertyInfo.GetValue(obj, null);
            }
        }
    }

    internal sealed class PropertyAccessor
    {
        public PropertyAccessor(string name, Type propertyType, Func<object, object> getter)
        {
            Name = name;
            PropertyType = propertyType ?? typeof(object);
            Getter = getter ?? (_ => null);
        }

        public string Name { get; }
        public Type PropertyType { get; }
        public Func<object, object> Getter { get; }
    }

    internal sealed class TypePropertyCache
    {
        public TypePropertyCache(PropertyAccessor[] accessors, Dictionary<string, PropertyAccessor> propertyMap)
        {
            Accessors = accessors ?? Array.Empty<PropertyAccessor>();
            PropertyMap = propertyMap ?? new Dictionary<string, PropertyAccessor>(StringComparer.OrdinalIgnoreCase);
        }

        public PropertyAccessor[] Accessors { get; }
        public Dictionary<string, PropertyAccessor> PropertyMap { get; }
    }
}
