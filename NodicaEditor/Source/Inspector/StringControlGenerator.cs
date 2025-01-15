using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using Nodica;

namespace NodicaEditor;

public class StringControlGenerator
{
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);

    public static TextBox CreateStringControl(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        TextBox textBox = new()
        {
            Text = GetStringValue(nodePropertyValues, propertyName) ?? "",
            Width = 100,
            Height = 22,
            Background = BackgroundColor,
            Foreground = ForegroundColor,
            BorderBrush = BackgroundColor,
            Style = null
        };

        textBox.TextChanged += (sender, _) =>
        {
            if (sender is TextBox tb)
            {
                OnStringTextChanged(tb, nodePropertyValues, propertyName);
            }
        };

        return textBox;
    }

    private static void OnStringTextChanged(TextBox tb, Dictionary<string, object?> nodePropertyValues, string propertyName)
    {
        SetStringValue(nodePropertyValues, propertyName, tb.Text);
    }

    private static string? GetStringValue(Dictionary<string, object?> propertyValues, string propertyName)
    {
        return propertyValues.TryGetValue(propertyName, out object? value) && value is string str
            ? str
            : null;
    }

    private static void SetStringValue(Dictionary<string, object?> propertyValues, string propertyName, string? value)
    {
        propertyValues[propertyName] = value;
    }
}