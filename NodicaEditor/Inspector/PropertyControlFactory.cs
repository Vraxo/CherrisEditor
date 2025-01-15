using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Nodica;
using Button = System.Windows.Controls.Button;
using Color = Raylib_cs.Color;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using TextBlock = System.Windows.Controls.TextBlock;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace NodicaEditor;

public static class PropertyControlFactory
{
    private static readonly SolidColorBrush BackgroundColor = new(new() { R = 16, G = 16, B = 16, A = 255 });
    private static readonly SolidColorBrush ForegroundColor = new(Colors.LightGray);
    private static readonly SolidColorBrush SeparatorColor = new(Colors.Gray);

    public static FrameworkElement? CreateControl(Node node, PropertyInfo property, string fullPath = "", Dictionary<string, object?> nodePropertyValues = null)
    {
        return property.PropertyType switch
        {
            Type t when t == typeof(string) => StringControlGenerator.CreateStringControl(node, property, fullPath, nodePropertyValues),
            Type t when t == typeof(float) => FloatControlGenerator.CreateFloatControl(node, property, fullPath, nodePropertyValues),
            Type t when t == typeof(bool) => BoolControlGenerator.CreateBoolControl(node, property, fullPath, nodePropertyValues),
            Type t when t == typeof(Vector2) => Vector2ControlGenerator.CreateVector2Control(node, property, fullPath, nodePropertyValues),
            Type t when t == typeof(Color) => ColorControlGenerator.CreateColorControl(node, property, fullPath, nodePropertyValues),
            Type t when t.IsEnum => EnumControlGenerator.CreateEnumControl(node, property, fullPath, nodePropertyValues),
            _ => null
        };
    }

    public static Border CreateSeparatorLine()
    {
        return new()
        {
            Height = 1,
            Background = SeparatorColor,
            Margin = new(0, 5, 0, 5)
        };
    }

    public static TextBlock CreateTypeLabel(Type type)
    {
        return new()
        {
            Text = type.Name,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            FontWeight = FontWeights.Bold,
            Margin = new(0, 5, 0, 5),
            Foreground = SeparatorColor
        };
    }

    public static Button CreateResetButton(Node node, PropertyInfo property, string propertyName, Dictionary<string, object?> nodePropertyValues, FrameworkElement propertyControl)
    {
        Button resetButton = new()
        {
            Content = "Reset",
            Width = 50,
            Margin = new Thickness(5),
            Tag = (node, property, propertyName, propertyControl),
            Background = BackgroundColor,
            Foreground = ForegroundColor,
            BorderBrush = BackgroundColor,
            Style = null
        };

        resetButton.Click += (sender, e) =>
        {
            if (sender is Button button && button.Tag is ValueTuple<Node, PropertyInfo, string, FrameworkElement> tag)
            {
                var (resetNode, resetProperty, resetPropertyName, associatedControl) = tag;
                object? defaultValue = DefaultValueProvider.GetDefaultValue(resetProperty, resetNode, resetPropertyName);
                SetPropertyValue(nodePropertyValues, resetPropertyName, defaultValue);

                // Update the control based on its type
                if (associatedControl is StackPanel panel)
                {
                    if (panel.Tag is string controlType)
                    {
                        switch (controlType)
                        {
                            case "ColorControl":
                                ColorControlGenerator.UpdateColorControl(panel, (Color?)defaultValue);
                                break;
                            case "Vector2Control":
                                Vector2ControlGenerator.UpdateVector2Control(panel, (Vector2?)defaultValue);
                                break;
                        }
                    }
                }
                else
                {
                    UpdateControlValue(associatedControl, defaultValue);
                }
            }
        };

        return resetButton;
    }

    private static void UpdateControlValue(FrameworkElement control, object? value)
    {
        switch (control)
        {
            case TextBox textBox:
                textBox.Text = value?.ToString() ?? "";
                break;
            case System.Windows.Controls.CheckBox checkBox:
                checkBox.IsChecked = (bool?)value;
                break;
            case ComboBox comboBox:
                comboBox.SelectedItem = value;
                break;
        }
    }

    private static void SetPropertyValue(Dictionary<string, object?> propertyValues, string propertyName, object? newValue)
    {
        propertyValues[propertyName] = newValue;
    }
}