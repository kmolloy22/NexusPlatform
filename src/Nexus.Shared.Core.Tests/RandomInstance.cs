using System.Collections;
using System.Reflection;

namespace Nexus.Shared.Core.Tests;

public static class RandomInstance
{
    private static readonly Dictionary<Type, Func<object>> TypeToValueMapper = new()
    {
        { typeof(int),        () => RandomValue.Int },
        { typeof(decimal),    () => RandomValue.Decimal },
        { typeof(bool),       () => RandomValue.Bool },
        { typeof(bool?),      () => RandomValue.Bool },
        { typeof(string),     () => RandomValue.String },
        { typeof(byte),       () => RandomValue.Byte },
        { typeof(DateTime),   () => RandomValue.DateTime },
        { typeof(DateTime?),  () => RandomValue.DateTime },
        { typeof(DateTimeOffset), () => RandomValue.DateTimeOffset },
        { typeof(DateTimeOffset?), () => RandomValue.DateTimeOffset },
        { typeof(HashSet<string>), () => RandomValue.HashSet }
    };

    public static T Single<T>(Action<T>? decorator = null)
        where T : class, new()
    {
        var instance = new T();
        PopulateObject(instance);

        decorator?.Invoke(instance);
        return instance;
    }

    // -----------------------------
    // Core population logic
    // -----------------------------
    private static void PopulateObject(object obj)
    {
        var type = obj.GetType();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.SetMethod == null || !prop.SetMethod.IsPublic)
                continue;

            var randomValue = ResolveValue(prop.PropertyType);
            prop.SetValue(obj, randomValue);
        }
    }

    // -----------------------------
    // Value resolver
    // -----------------------------
    private static object? ResolveValue(Type type)
    {
        // Direct mapping (int, decimal, string, bool,…)
        if (TypeToValueMapper.TryGetValue(type, out var mapped))
        {
            return mapped();
        }

        // Nullable<T>
        if (Nullable.GetUnderlyingType(type) is Type inner)
        {
            if (RandomValue.Bool) // randomly null or populated
                return null;

            return ResolveValue(inner);
        }

        // Arrays
        if (type.IsArray)
        {
            return CreateArray(type.GetElementType()!);
        }

        // List<T>
        if (type.IsGenericType &&
            type.GetGenericTypeDefinition() == typeof(List<>))
        {
            var itemType = type.GenericTypeArguments[0];
            return CreateList(itemType);
        }

        // Classes (recursive population)
        if (type.IsClass && type.GetConstructor(Type.EmptyTypes) != null)
        {
            var instance = Activator.CreateInstance(type)!;
            PopulateObject(instance);
            return instance;
        }

        // Unsupported types: return default
        return null;
    }

    // -----------------------------
    // Collections
    // -----------------------------
    private static Array CreateArray(Type elementType)
    {
        var length = RandomValue.Int % 5 + 1;
        var arr = Array.CreateInstance(elementType, length);

        for (var i = 0; i < length; i++)
        {
            arr.SetValue(ResolveValue(elementType), i);
        }

        return arr;
    }

    private static object CreateList(Type elementType)
    {
        var listType = typeof(List<>).MakeGenericType(elementType);
        var list = (IList)Activator.CreateInstance(listType)!;

        var count = RandomValue.Int % 5 + 1;
        for (var i = 0; i < count; i++)
        {
            list.Add(ResolveValue(elementType));
        }

        return list;
    }
}