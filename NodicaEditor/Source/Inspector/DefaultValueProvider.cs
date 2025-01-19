using System;
using System.Numerics;
using System.Reflection;
using Cherris;
using Color = Raylib_cs.Color;

namespace CherrisEditor;

public class DefaultValueProvider
{
    public static object? GetDefaultValue(PropertyInfo property, Node node, string fullPath = "")
    {
        try
        {
            if (string.IsNullOrEmpty(fullPath) || !fullPath.Contains("/"))
            {
                // Non-nested properties
                return property.GetValue(Activator.CreateInstance(node.GetType()));
            }
            else
            {
                // Nested properties
                return GetNestedDefaultValue(property, node, fullPath);
            }
        }
        catch (Exception ex)
        {
            // Log the exception (consider using a logging framework)
            Console.WriteLine($"Exception in GetDefaultValue: {ex.Message}");

            // Fallback for unsupported types or exceptions
            return GetFallbackValue(property);
        }
    }

    private static object? GetNestedDefaultValue(PropertyInfo property, Node node, string fullPath)
    {
        string[] pathParts = fullPath.Split('/');
        object currentObject = node;
        PropertyInfo currentProperty = null;

        for (int i = 0; i < pathParts.Length; i++)
        {
            currentProperty = currentObject.GetType().GetProperty(pathParts[i]);
            if (currentProperty == null)
            {
                throw new InvalidOperationException($"Property '{pathParts[i]}' not found in path '{fullPath}'.");
            }

            if (i < pathParts.Length - 1)
            {
                currentObject = currentProperty.GetValue(currentObject);
                if (currentObject == null)
                {
                    // If any intermediate object is null, we can't proceed
                    return null;
                }
            }
        }

        // Create a default instance of the parent object and get the default value
        object parentObject = Activator.CreateInstance(currentObject.GetType());
        return currentProperty.GetValue(parentObject);
    }

    private static object? GetFallbackValue(PropertyInfo property)
    {
        return property.PropertyType switch
        {
            Type t when t == typeof(bool) => false,
            Type t when t == typeof(int) => 0,
            Type t when t == typeof(float) => 0f,
            Type t when t == typeof(Vector2) => Vector2.Zero,
            Type t when t == typeof(Color) => default(Color),
            _ => property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null
        };
    }
}