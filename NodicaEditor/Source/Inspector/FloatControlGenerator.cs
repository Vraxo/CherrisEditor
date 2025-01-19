using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using Cherris;

namespace CherrisEditor;

public class FloatControlGenerator
{
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);

    public static TextBox CreateFloatControl(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        TextBox textBox = new()
        {
            Text = GetFloatValue(nodePropertyValues, propertyName).ToString() ?? "",
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
                OnFloatTextChanged(tb, nodePropertyValues, propertyName);
            }
        };

        return textBox;
    }

    private static void OnFloatTextChanged(TextBox tb, Dictionary<string, object?> nodePropertyValues, string propertyName)
    {
        if (float.TryParse(tb.Text, out float value))
        {
            SetFloatValue(nodePropertyValues, propertyName, value);
        }
    }

    private static float GetFloatValue(Dictionary<string, object?> propertyValues, string propertyName)
    {
        return propertyValues.TryGetValue(propertyName, out object? value) && value is float f
            ? f
            : 0f;
    }

    private static void SetFloatValue(Dictionary<string, object?> propertyValues, string propertyName, float value)
    {
        propertyValues[propertyName] = value;
    }
}