using System.Reflection;
using System.Windows.Media;
using Nodica;
using CheckBox = System.Windows.Controls.CheckBox;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace NodicaEditor;

public class BoolControlGenerator
{
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);

    public static CheckBox CreateBoolControl(Node node, PropertyInfo property, string fullPath, Dictionary<string, object?> nodePropertyValues)
    {
        string propertyName = fullPath != "" ? fullPath : property.Name;
        CheckBox checkBox = new()
        {
            IsChecked = GetBoolValue(nodePropertyValues, propertyName),
            Background = BackgroundColor,
            Foreground = ForegroundColor,
            BorderBrush = BackgroundColor,
            VerticalAlignment = VerticalAlignment.Center,
            Style = null
        };

        checkBox.Checked += (sender, _) => SetBoolValue(nodePropertyValues, propertyName, true);
        checkBox.Unchecked += (sender, _) => SetBoolValue(nodePropertyValues, propertyName, false);

        return checkBox;
    }

    private static bool GetBoolValue(Dictionary<string, object?> propertyValues, string propertyName)
    {
        return propertyValues.TryGetValue(propertyName, out object? value) && value is bool b
            ? b
            : false; // Default to false if not found or not a bool
    }

    private static void SetBoolValue(Dictionary<string, object?> propertyValues, string propertyName, bool value)
    {
        propertyValues[propertyName] = value;
    }
}