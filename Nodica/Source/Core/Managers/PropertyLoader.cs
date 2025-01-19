using System.Reflection;

namespace Cherris;

public static class PropertyLoader
{
    public static T Load<T>(string filePath) where T : new()
    {
        //T stylePack = new();
        //string[] fileLines = File.ReadAllLines(filePath);
        //
        //foreach (string line in fileLines)
        //{
        //    string trimmedLine = line.Trim();
        //    
        //    if (string.IsNullOrEmpty(trimmedLine) || !trimmedLine.Contains('='))
        //    {
        //        continue;
        //    }
        //
        //    int equalsIndex = trimmedLine.IndexOf("=");
        //    string propertyPath = trimmedLine.Substring(0, equalsIndex).Trim();
        //    string value = trimmedLine.Substring(equalsIndex + 1).Trim();
        //
        //    SetPropertyValue(stylePack, propertyPath, value);
        //}
        //
        //return stylePack;

        return default;
    }

    private static void SetPropertyValue(object obj, string propertyPath, string value)
    {
        string[] segments = propertyPath.Split('/');
        Type type = obj.GetType();
        PropertyInfo propertyInfo = null;

        for (int i = 0; i < segments.Length; i++)
        {
            string segment = segments[i];
            propertyInfo = type.GetProperty(segment, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!;

            if (propertyInfo == null)
                throw new Exception($"Property '{segment}' not found on type '{type.Name}'.");

            if (i < segments.Length - 1)
            {
                obj = propertyInfo.GetValue(obj)!;
                type = obj.GetType();
            }
        }

        if (propertyInfo != null && propertyInfo.CanWrite)
        {
            object convertedValue = ConvertValue(propertyInfo.PropertyType, value);
            propertyInfo.SetValue(obj, convertedValue);
        }
    }

    private static object ConvertValue(Type propertyType, string value) => propertyType switch
    {
        { } when propertyType == typeof(uint) => Convert.ToUInt32(value),
        { } when propertyType == typeof(Color) => ParseColor(value),
        { } when propertyType == typeof(Font) => ResourceLoader.Load<Font>(value),
        { } when propertyType == typeof(float) => float.Parse(value),
        { } when propertyType == typeof(int) => int.Parse(value),
        { } when propertyType == typeof(bool) => bool.Parse(value),
        { } when propertyType == typeof(Vector2) => ParseVector2(value),
        { } when propertyType.IsEnum => Enum.Parse(propertyType, value),
        _ => value
    };

    private static Vector2 ParseVector2(string value)
    {
        string[] components = value.Trim('(', ')').Split(',');
        return new Vector2(float.Parse(components[0]), float.Parse(components[1]));
    }

    private static Color ParseColor(string value)
    {
        string numericPart = value.Replace("Color", "").Trim('(', ')');
        string[] components = numericPart.Split(',');

        return new(
            byte.Parse(components[0].Trim()),
            byte.Parse(components[1].Trim()),
            byte.Parse(components[2].Trim()),
            byte.Parse(components[3].Trim())
        );
    }
}