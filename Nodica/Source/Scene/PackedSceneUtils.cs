using System.Reflection;
using System.Resources;

namespace Cherris;

public static class PackedSceneUtils
{
    private static readonly string[] SpecialProperties = ["type", "name", "parent", "path"];

    public static void SetProperties(Node node, Dictionary<string, object> element)
    {
        foreach (KeyValuePair<string, object> property in element)
        {
            string propertyName = property.Key;
            if (SpecialProperties.Contains(propertyName))
            {
                continue;
            }

            object value = property.Value;
            SetNestedProperty(node, propertyName, value);
        }
    }

    private static void SetNestedProperty(object target, string propertyPath, object value)
    {
        string[] pathParts = propertyPath.Split('/');
        object currentObject = target;

        for (int i = 0; i < pathParts.Length; i++)
        {
            string part = pathParts[i];
            PropertyInfo? propertyInfo = currentObject!.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
            {
                throw new Exception($"Property '{part}' not found on type '{currentObject.GetType().Name}'.");
            }

            // Throw an exception if the property doesn't have a setter
            if (!propertyInfo.CanWrite)
            {
                throw new InvalidOperationException($"Property '{part}' does not have a setter and cannot be modified.");
            }

            if (i == pathParts.Length - 1)
            {
                object propertyValue = ConvertValue(propertyInfo.PropertyType, value);
                propertyInfo.SetValue(currentObject, propertyValue);
            }
            else
            {
                object? nextObject = propertyInfo.GetValue(currentObject);
                if (nextObject == null)
                {
                    nextObject = Activator.CreateInstance(propertyInfo.PropertyType);
                    propertyInfo.SetValue(currentObject, nextObject);
                }
                currentObject = nextObject!;
            }
        }
    }

    public static Type ResolveType(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type? type = assembly.GetType(typeName, false, true);
            
            if (type is not null)
            {
                return type;
            }

            string defaultNamespace = assembly.GetName().Name!;
            string namespacedTypeName = defaultNamespace + "." + typeName;
            type = assembly.GetType(namespacedTypeName, false, true);
            
            if (type is not null)
            {
                return type;
            }
        }

        throw new Exception($"Type '{typeName}' not found.");
    }

    public static object ConvertValue(Type targetType, object value)
    {
        var stringValue = (string)value;

        return targetType switch
        {
            Type t when t == typeof(Vector2) => ParseVector2(stringValue),
            Type t when t == typeof(Color) => ParseColor(stringValue),
            Type t when t == typeof(Audio) => ParseAudio(stringValue),
            Type t when t == typeof(Texture) => ParseTexture(stringValue),
            Type t when t == typeof(Font) => ResourceLoader.Load<Font>(stringValue),
            Type t when t.IsEnum => Enum.Parse(targetType, stringValue),
            Type t when t == typeof(int) => int.Parse(stringValue),
            Type t when t == typeof(uint) => uint.Parse(stringValue),
            Type t when t == typeof(float) => float.Parse(stringValue),
            Type t when t == typeof(double) => double.Parse(stringValue),
            Type t when t == typeof(bool) => bool.Parse(stringValue),
            Type t when t == typeof(string) => stringValue,
            _ => throw new Exception($"Unsupported type {targetType.Name} for value {stringValue}")
        };
    }

    private static Vector2 ParseVector2(string value)
    {
        string[] parts = value.Trim('(', ')').Split(',');

        float x = float.Parse(parts[0]);
        float y = float.Parse(parts[1]);

        return new(x, y);
    }

    private static Color ParseColor(string value)
    {
        string[] parts = value.Trim('(', ')').Split(',');

        byte r = byte.Parse(parts[0]);
        byte g = byte.Parse(parts[1]);
        byte b = byte.Parse(parts[2]);
        byte a = byte.Parse(parts[3]);

        return new(r, g, b, a);
    }

    private static Audio ParseAudio(string path)
    {
        return new(path);
    }

    private static Texture ParseTexture(string path)
    {
        return ResourceLoader.Load<Texture>(path);
    }
}